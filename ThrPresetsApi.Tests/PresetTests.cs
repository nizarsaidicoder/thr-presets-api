using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ThrPresetsApi.Api.Data;
using ThrPresetsApi.Api.Features.Presets.DTOs;
using ThrPresetsApi.Api.Models;
using ThrPresetsApi.Tests.Infrastructure;

namespace ThrPresetsApi.Tests;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerClass)]
[NotInParallel]
public class PresetTests(ApiFactory factory) : BaseTest(factory)
{
    private const string PresetsUrl = "/api/presets";

    [Test]
    public async Task CreatePreset_WithTags_ReturnsCreatedWithTags()
    {
        // Arrange
        await Factory.ResetDatabaseAsync();
        var tagId = await GetFirstSeedTagId();
        
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Tagged Preset"), "Name");
        content.Add(new StringContent("true"), "IsPublic");
        content.Add(new StringContent(tagId), "TagIds[0]");
        
        var fileContent = new ByteArrayContent("data"u8.ToArray());
        content.Add(fileContent, "File", "tagged.thrl6p");

        // Act
        var response = await Client.PostAsync(PresetsUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        var preset = await response.Content.ReadFromJsonAsync<PresetDto>();
        // Future verification: Ensure Tags are returned in DTO
    }

    [Test]
    public async Task RatePreset_ValidRating_UpdatesWilsonScore()
    {
        // Arrange
        var token = await GetAuthToken();
        var preset = await CreatePresetForRating(token);

        // Act - Submit a 5-star rating
        var rateReq = new HttpRequestMessage(HttpMethod.Post, $"{PresetsUrl}/{preset.Slug}/rate");
        rateReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        rateReq.Content = JsonContent.Create(new { Stars = 5 });
        var response = await Client.SendAsync(rateReq);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        
        var updated = await Client.GetFromJsonAsync<PresetDto>($"{PresetsUrl}/{preset.Slug}");
        await Assert.That(updated!.WilsonScore).IsGreaterThan(0);
    }

    [Test]
    public async Task GetPresets_FilterByMultipleTags_ReturnsIntersection()
    {
        // Arrange
        var tags = await GetTagsAsync();
        var metalId = tags.First(t => t.Name == "Metal").Id;
        var leadId = tags.First(t => t.Name == "Lead").Id;

        // Act - Query string format: ?tagIds=ID1&tagIds=ID2
        var response = await Client.GetAsync($"{PresetsUrl}?tagIds={metalId}&tagIds={leadId}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    private async Task<List<Tag>> GetTagsAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Tags.ToListAsync();
    }

    private async Task CreatePresetWithTags(string name, string[] tagIds)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(name), "Name");
    
        // Multipart form data indexing for arrays
        for (int i = 0; i < tagIds.Length; i++)
        {
            content.Add(new StringContent(tagIds[i]), $"TagIds[{i}]");
        }

        var fileContent = new ByteArrayContent("data"u8.ToArray());
        content.Add(fileContent, "File", $"{name}.thrl6p");
        await Client.PostAsync(PresetsUrl, content);
    }


    private async Task<PresetDto> CreatePresetForRating(string token, string name = "RateMe")
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(name), "Name");
        content.Add(new StringContent("true"), "IsPublic");
        var fileContent = new ByteArrayContent("data"u8.ToArray());
        content.Add(fileContent, "File", $"{name}.thrl6p");

        var request = new HttpRequestMessage(HttpMethod.Post, PresetsUrl) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await Client.SendAsync(request);
        return (await res.Content.ReadFromJsonAsync<PresetDto>())!;
    }

    private async Task<string> GetAuthToken()
    {
        var email = $"user_{Guid.NewGuid()}@test.com";
        var payload = new { email, username = "tester", password = "Password123!" };
        var response = await Client.PostAsJsonAsync("/api/auth/signup", payload);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }
    private async Task<string> GetFirstSeedTagId()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
        var tag = await db.Tags.FirstOrDefaultAsync();
    
        return tag?.Id ?? throw new Exception("Tags were not seeded. Check DbSeeder or ResetDatabaseAsync.");
    }
}