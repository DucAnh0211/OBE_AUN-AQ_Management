using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ObeAunQa.Modules.Knowledge.Application;
using ObeAunQa.Modules.Knowledge.Infrastructure;
namespace ObeAunQa.Modules.Knowledge.Tests;

public sealed class PythonPipelineHttpTests
{
    private sealed class Handler(Func<HttpRequestMessage,CancellationToken,Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        public int Calls {get;private set;}
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct) {Calls++;return send(request,ct);}
    }
    private static PythonKnowledgeChatPipeline Pipeline(Handler handler,string key="shared-test-service-key-0123456789") => new(new HttpClient(handler),Options.Create(new KnowledgeAiOptions {Url="http://fake-ai.test",SharedKey=key}));
    [Fact] public async Task UsesConfiguredEndpointAndSecretWithStructuredBody()
    {
        var h=new Handler(async (request,ct)=> {
            Assert.Equal("http://fake-ai.test/internal/knowledge/document-keywords",request.RequestUri!.ToString());
            Assert.Equal("shared-test-service-key-0123456789",request.Headers.GetValues("X-Service-Key").Single());
            var json=JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));Assert.Equal("CLO results",json.RootElement.GetProperty("question").GetString());
            return new(HttpStatusCode.OK){Content=new StringContent("{\"question\":\"CLO results\",\"queries\":[\"CLO\"]}",Encoding.UTF8,"application/json")};
        });
        Assert.Equal("CLO",(await Pipeline(h).PlanAsync("CLO results",[],default)).Queries.Single());
    }
    [Fact] public async Task MissingSecretDoesNotSendRequest()
    {
        var h=new Handler((r,c)=>throw new InvalidOperationException("Must not call network"));
        var error=await Assert.ThrowsAsync<KnowledgeException>(()=>Pipeline(h,"").PlanAsync("Test",[],default));
        Assert.Equal(503,error.StatusCode);Assert.Equal(0,h.Calls);
    }
    [Fact] public async Task UpstreamErrorsAndCancellationArePreserved()
    {
        var h=new Handler((r,c)=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest){Content=new StringContent("{\"detail\":\"Invalid source\"}",Encoding.UTF8,"application/json")}));
        var error=await Assert.ThrowsAsync<KnowledgeException>(()=>Pipeline(h).PlanAsync("Test",[],default));Assert.Equal(502,error.StatusCode);
        using var cts=new CancellationTokenSource();cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>Pipeline(h).PlanAsync("Test",[],cts.Token));
    }
}
