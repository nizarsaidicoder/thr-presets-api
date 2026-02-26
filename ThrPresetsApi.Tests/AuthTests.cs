using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ThrPresetsApi.Tests.Infrastructure;

namespace ThrPresetsApi.Tests;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerClass)]
[NotInParallel]
public class AuthTests(ApiFactory factory) : BaseTest(factory)
{
    private const string SignupUrl = "/api/auth/signup";
    private const string SigninUrl = "/api/auth/signin";
    private const string RefreshUrl = "/api/auth/refresh";
    private const string LogoutUrl = "/api/auth/logout";

    #region Signup Tests

    [Test]
    public async Task Signup_ValidData_ReturnsCreatedAndToken()
    {
        var payload = new { email = "test@example.com", username = "testuser", password = "Test1234!" };
        var response = await Client.PostAsJsonAsync(SignupUrl, payload);
        
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        await Assert.That(body.GetProperty("accessToken").GetString()).IsNotNull();
        
        var user = body.GetProperty("user");
        await Assert.That(user.GetProperty("email").GetString()).IsEqualTo("test@example.com");
        await Assert.That(user.TryGetProperty("passwordHash", out _)).IsFalse();
    }

    [Test]
    public async Task Signup_DuplicateEmail_ReturnsConflict()
    {
        var payload = new { email = "test@example.com", username = "user1", password = "Password123!" };
        await Client.PostAsJsonAsync(SignupUrl, payload);
        
        var response = await Client.PostAsJsonAsync(SignupUrl, payload with { username = "user2" });
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Signup_DuplicateUsername_ReturnsConflict()
    {
        var payload = new { email = "user1@test.com", username = "testuser", password = "Password123!" };
        await Client.PostAsJsonAsync(SignupUrl, payload);
        
        var response = await Client.PostAsJsonAsync(SignupUrl, payload with { email = "user2@test.com" });
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    [Arguments("not-an-email", "user", "Pass123!")] // Invalid Email
    [Arguments("test@test.com", "u", "Pass123!")]    // Username too short
    [Arguments("test@test.com", "user", "123")]      // Password too short
    [Arguments("test@test.com", "user!", "Pass123!")] // Special chars in username
    public async Task Signup_InvalidData_ReturnsBadRequest(string email, string username, string password)
    {
        var response = await Client.PostAsJsonAsync(SignupUrl, new { email, username, password });
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Signup_EmptyBody_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync(SignupUrl, new { });
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Signin Tests

    [Test]
    public async Task Signin_ValidCredentials_SetsCookieAndReturnsToken()
    {
        var user = new { email = "login@test.com", username = "loginuser", password = "Password123!" };
        await Client.PostAsJsonAsync(SignupUrl, user);

        var response = await Client.PostAsJsonAsync(SigninUrl, new { user.email, user.password });
        
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.Headers.Contains("Set-Cookie")).IsTrue();
        
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        await Assert.That(body.GetProperty("accessToken").GetString()).IsNotNull();
    }

    [Test]
    [Arguments("login@test.com", "wrongpassword")] // Wrong password
    [Arguments("unknown@test.com", "Password123!")] // Unknown email
    [Arguments("", "Password123!")]                // Empty email
    [Arguments("login@test.com", "")]               // Empty password
    public async Task Signin_InvalidCredentials_ReturnsBadRequest(string email, string password)
    {
        var user = new { email = "login@test.com", username = "loginuser", password = "Password123!" };
        await Client.PostAsJsonAsync(SignupUrl, user);

        var response = await Client.PostAsJsonAsync(SigninUrl, new { email, password });
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Refresh & Logout Tests

    [Test]
    public async Task Refresh_ValidCookie_ReturnsNewToken()
    {
        var user = new { email = "refresh@test.com", username = "refreshuser", password = "Password123!" };
        await Client.PostAsJsonAsync(SignupUrl, user);
        await Client.PostAsJsonAsync(SigninUrl, new { user.email, user.password });

        var response = await Client.PostAsync(RefreshUrl, null);
        
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        await Assert.That(body.GetProperty("accessToken").GetString()).IsNotNull();
    }

    [Test]
    public async Task Logout_WhenAuthenticated_ReturnsNoContent()
    {
        var user = new { email = "logout@test.com", username = "logoutuser", password = "Password123!" };
        var signupRes = await Client.PostAsJsonAsync(SignupUrl, user);
        var token = (await signupRes.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString();

        var request = new HttpRequestMessage(HttpMethod.Post, LogoutUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.SendAsync(request);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        // Verify refresh is now impossible
        var refreshRes = await Client.PostAsync(RefreshUrl, null);
        await Assert.That(refreshRes.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Logout_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        var response = await Client.PostAsync(LogoutUrl, null);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Refresh_WithTamperedToken_ReturnsBadRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, RefreshUrl);
        request.Headers.Add("Cookie", "refresh_token=invalid.token.payload");

        var response = await Client.SendAsync(request);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    #endregion
}