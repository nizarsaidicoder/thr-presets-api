namespace ThrPresetsApi.Tests.Infrastructure;

public abstract class BaseTest(ApiFactory factory)
{
    protected readonly ApiFactory Factory = factory;
    protected readonly HttpClient Client = factory.CreateClient();

    [Before(HookType.Test)]
    public async Task Setup()
    {
        await DatabaseHelper.ResetAsync(Factory);
    }

    public virtual async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await ValueTask.CompletedTask;
    }
}