using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocFlow.Tests;

public sealed class Harness
{
    public TestClock Clock { get; } = new();
    public Probe Probe { get; } = new();
    public ServiceProvider Services { get; }
    public ProcessEngine Engine { get; }
    public IProcessRepository Repo { get; }
    public DocumentRegistrar Registrar { get; }
    public RouteRegistry Routes { get; }
    public IProcessAudit Audit { get; }

    public Harness()
    {
        var s = new ServiceCollection();
        s.AddSingleton(Microsoft.Extensions.Options.Options.Create(new EngineOptions { MaxAttempts = 3, RetryBaseDelaySeconds = 10 }));
        s.AddSingleton<TimeProvider>(Clock);
        s.AddSingleton(Probe);
        s.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        s.AddSingleton<IProcessRepository, InMemoryProcessRepository>();
        s.AddSingleton<IProcessAudit, InMemoryProcessAudit>();
        s.AddSingleton<IEscalationNotifier, RecordingNotifier>();
        s.AddSingleton<ProcessLocks>();
        s.AddSingleton<IRoute, LinearRoute>();
        s.AddSingleton<IRoute, WaitRoute>();
        s.AddSingleton<IRoute, DataRoute>();
        s.AddTransient<StepA>();
        s.AddTransient<StepB>();
        s.AddSingleton<RouteRegistry>();
        s.AddSingleton<ProcessEngine>();
        s.AddSingleton<DocumentRegistrar>();
        Services = s.BuildServiceProvider();

        Engine = Services.GetRequiredService<ProcessEngine>();
        Repo = Services.GetRequiredService<IProcessRepository>();
        Registrar = Services.GetRequiredService<DocumentRegistrar>();
        Routes = Services.GetRequiredService<RouteRegistry>();
        Audit = Services.GetRequiredService<IProcessAudit>();
    }

    /// <summary>Зарегистрировать документ и вернуть id процесса (или null, если процесс не создан).</summary>
    public async Task<Guid?> RegisterAsync(
        string type, string externalId = "doc1", DateTime? pollStartedAt = null, Dictionary<string, string>? data = null)
    {
        var ids = await Registrar.RegisterAsync(
            new[] { new DocumentRegistration("test", externalId, type, "/x", data) },
            pollStartedAt ?? Clock.Now, CancellationToken.None);
        return ids.Count == 0 ? null : ids[0];
    }

    public async Task<DocumentProcess> GetAsync(Guid id) => (await Repo.GetAsync(id, CancellationToken.None))!;
    public Task AdvanceAsync(Guid id) => Engine.AdvanceAsync(id, CancellationToken.None);
}
