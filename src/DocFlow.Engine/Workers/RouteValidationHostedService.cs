namespace DocFlow.Engine;

public sealed class RouteValidationHostedService(
    RouteRegistry routes,
    IEnumerable<ISignalSource> signalSources,
    IServiceProvider services) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var errors = RouteValidator.Validate(routes, signalSources, services);
        if (errors.Count > 0)
            throw new InvalidOperationException("Ошибки в маршрутах:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
