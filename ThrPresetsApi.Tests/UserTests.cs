using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ThrPresetsApi.Tests.Infrastructure;

namespace ThrPresetsApi.Tests;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerClass)]
[NotInParallel]
public class UserTests(ApiFactory factory) : BaseTest(factory)
{
    private const string UsersUrl = "/api/users";
    private const string AuthUrl = "/api/auth";

    private async Task<(string Token, string UserId)> CreateAndLoginUser(string email, string username)
    {
        var payload = new { email, username, password = "Password123!" };
        var response = await Client.PostAsJsonAsync($"{AuthUrl}/signup", payload);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        
        return (
            body.GetProperty("accessToken").GetString()!,
            body.GetProperty("user").GetProperty("id").GetString()!
        );
    }

    #region Profile Tests

    [Test]
    public async Task GetMe_Authenticated_ReturnsProfile()
    {
        // Arrange
        var (token, _) = await CreateAndLoginUser("me@test.com", "meuser");
        var request = new HttpRequestMessage(HttpMethod.Get, $"{UsersUrl}/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        await Assert.That(body.GetProperty("email").GetString()).IsEqualTo("me@test.com");
        await Assert.That(body.GetProperty("username").GetString()).IsEqualTo("meuser");
    }

    [Test]
    public async Task GetMe_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync($"{UsersUrl}/me");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task UpdateMe_ValidData_UpdatesUser()
    {
        // Arrange
        var (token, _) = await CreateAndLoginUser("update@test.com", "oldname");
        var updatePayload = new { username = "newname", avatarUrl = "https://picsum.photos/200" };
        
        var request = new HttpRequestMessage(HttpMethod.Patch, $"{UsersUrl}/me")
        {
            Content = JsonContent.Create(updatePayload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        await Assert.That(body.GetProperty("username").GetString()).IsEqualTo("newname");
        await Assert.That(body.GetProperty("avatarUrl").GetString()).IsEqualTo("https://picsum.photos/200");
    }

    [Test]
    public async Task UpdateMe_DuplicateUsername_ReturnsConflict()
    {
        // Arrange
        await CreateAndLoginUser("existing@test.com", "taken_name");
        var (token, _) = await CreateAndLoginUser("updater@test.com", "updater");
        
        var request = new HttpRequestMessage(HttpMethod.Patch, $"{UsersUrl}/me")
        {
            Content = JsonContent.Create(new { username = "taken_name" })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task DeleteMe_Authenticated_DeletesAccount()
    {
        // Arrange
        var (token, _) = await CreateAndLoginUser("delete@test.com", "deleteuser");
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{UsersUrl}/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        // Verify user is gone from public search
        var publicRes = await Client.GetAsync($"{UsersUrl}/deleteuser");
        await Assert.That(publicRes.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    #endregion

    #region Public Profile Tests

    [Test]
    public async Task GetPublicProfile_ExistingUser_ReturnsData()
    {
        // Arrange
        await CreateAndLoginUser("public@test.com", "guitarist");

        // Act
        var response = await Client.GetAsync($"{UsersUrl}/guitarist");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        await Assert.That(body.GetProperty("user").GetProperty("username").GetString()).IsEqualTo("guitarist");
        await Assert.That(body.TryGetProperty("presets", out _)).IsTrue();
    }

    [Test]
    public async Task GetPublicProfile_NonExistentUser_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"{UsersUrl}/ghost_user");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    #endregion
}