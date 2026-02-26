namespace ThrPresetsApi.Tests.Infrastructure;

public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> PostJsonAsync<T>(
        this HttpClient client, string url, T body)
        => client.PostAsJsonAsync(url, body);

    public static async Task<T?> ReadJsonAsync<T>(this HttpResponseMessage response)
        => await response.Content.ReadFromJsonAsync<T>();
}