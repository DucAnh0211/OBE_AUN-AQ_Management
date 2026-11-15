using System.Net.Http.Json;
using System.Text.Json;
namespace ObeAunQa.SharedKernel;

/// <summary>Shared authenticated JSON transport; feature modules own their prompts, DTOs and HTTP timeout.</summary>
public static class AiHttp
{
    public static async Task<T> PostAsync<T>(HttpClient http,string baseUrl,string sharedKey,string path,object body,CancellationToken ct)
    {
        if(sharedKey.Length<32) throw new AiServiceException(503,"Chưa cấu hình AI_SERVICE_KEY.");
        if(!Uri.TryCreate(baseUrl,UriKind.Absolute,out var url) || url.Scheme is not ("http" or "https") || Uri.TryCreate(path,UriKind.Absolute,out _))
            throw new AiServiceException(503,"Địa chỉ dịch vụ AI không hợp lệ.");
        using var request=new HttpRequestMessage(HttpMethod.Post,baseUrl.TrimEnd('/')+"/"+path.TrimStart('/'));
        request.Headers.Add("X-Service-Key",sharedKey);request.Content=JsonContent.Create(body);
        try {
            using var response=await http.SendAsync(request,ct);
            if(!response.IsSuccessStatusCode) {
                var error=await response.Content.ReadFromJsonAsync<JsonElement>(ct);
                var detail=error.ValueKind==JsonValueKind.Object && error.TryGetProperty("detail",out var d) && d.ValueKind==JsonValueKind.String ? d.GetString() : null;
                throw new AiServiceException(502,detail is {Length:>0 and <=500}?detail:"Dịch vụ AI chưa xử lý được yêu cầu.");
            }
            return await response.Content.ReadFromJsonAsync<T>(ct) ?? throw new AiServiceException(502,"Dịch vụ AI không trả kết quả.");
        }
        catch(HttpRequestException){throw new AiServiceException(503,"Chưa kết nối được dịch vụ AI.");}
        catch(JsonException){throw new AiServiceException(502,"Dịch vụ AI trả dữ liệu không hợp lệ.");}
        catch(OperationCanceledException) when(!ct.IsCancellationRequested){throw new AiServiceException(504,"Dịch vụ AI vượt thời gian chờ.");}
    }
}
