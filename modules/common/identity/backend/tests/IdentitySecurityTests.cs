using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Identity.Tests;

public sealed class IdentitySecurityTests
{
    [Fact]
    public void School_headers_cannot_change_member_scope_and_internal_execution_is_restored()
    {
        var http=new DefaultHttpContext();
        http.User=new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier,"12"), new Claim(ClaimTypes.Role,SystemRoles.SchoolAdmin),new Claim("university_id","7")
        },"test"));
        http.Request.Headers["X-University-Id"]="99";
        var user=new HttpCurrentUser(new HttpContextAccessor {HttpContext=http});
        Assert.Equal(7,user.UniversityId); Assert.True(SystemRoles.IsAdmin(user.Role));
        using(user.ExecuteAs(23,SystemRoles.Student,8)) { Assert.Equal(8,user.UniversityId); Assert.Equal(23,user.UserId); }
        Assert.Equal(7,user.UniversityId); Assert.Equal(12,user.UserId);
        http.User=new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim(ClaimTypes.Role,SystemRoles.Admin)},"test"));
        Assert.Equal(99,user.UniversityId);
        Assert.False(SystemRoles.IsAdmin(SystemRoles.Lecturer)); Assert.False(SystemRoles.IsAdmin(SystemRoles.Student));
    }
    [Fact]
    public void Password_requires_length_upper_lower_and_digit()
    {
        Assert.Throws<IdentityApiException>(()=>IdentityService.ValidatePassword("short1A"));
        Assert.Throws<IdentityApiException>(()=>IdentityService.ValidatePassword("alllowercase1"));
        Assert.Throws<IdentityApiException>(()=>IdentityService.ValidatePassword("ALLUPPERCASE1"));
        Assert.Throws<IdentityApiException>(()=>IdentityService.ValidatePassword("NoDigitsHere"));
        IdentityService.ValidatePassword("ValidPass123");
    }

    [Fact]
    public void Access_token_contains_role_version_and_password_state()
    {
        var service=new TokenService(Options.Create(new JwtOptions{
            Issuer="issuer",Audience="audience",SigningKey="a-development-key-with-more-than-32-bytes"}));
        var user=new SecurityUser(42,"lecturer@example.edu.vn","lecturer@example.edu.vn","Lecturer","hash",
            "lecturer","active",false,7,0,null,null,DateTime.UtcNow,DateTime.UtcNow,7);
        var issued=service.CreateAccessToken(user);
        var token=new JwtSecurityTokenHandler().ReadJwtToken(issued.Token);
        Assert.Contains(token.Claims,x=>x.Type==ClaimTypes.Role&&x.Value=="lecturer");
        Assert.Contains(token.Claims,x=>x.Type=="token_version"&&x.Value=="7");
        Assert.Contains(token.Claims,x=>x.Type=="university_id"&&x.Value=="7");
        Assert.Contains(token.Claims,x=>x.Type=="must_change_password"&&x.Value=="false");
        Assert.True(issued.ExpiresAt>DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Refresh_tokens_are_random_and_only_hashes_need_storage()
    {
        var service=new TokenService(Options.Create(new JwtOptions{SigningKey="a-development-key-with-more-than-32-bytes"}));
        var first=service.CreateRefreshToken(); var second=service.CreateRefreshToken();
        Assert.NotEqual(first.Raw,second.Raw);
        Assert.NotEqual(first.Hash,second.Hash);
        Assert.Equal(64,first.Hash.Length);
        Assert.Equal(first.Hash,TokenService.Hash(first.Raw));
    }

    [Fact]
    public void Current_user_reads_single_role_and_password_state_from_claims()
    {
        var context=new DefaultHttpContext();
        context.User=new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier,"15"),new Claim(ClaimTypes.Role,SystemRoles.Student),
            new Claim("must_change_password","false")],"test"));
        var current=new HttpCurrentUser(new HttpContextAccessor{HttpContext=context});
        Assert.True(current.IsAuthenticated);
        Assert.Equal(15,current.UserId);
        Assert.Equal(SystemRoles.Student,current.Role);
        Assert.False(current.MustChangePassword);
    }
}
