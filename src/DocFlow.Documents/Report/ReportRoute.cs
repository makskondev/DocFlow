namespace DocFlow.Documents;

/// <summary>Внешний отчёт: подписываем автоматически через REST-сервис.</summary>
public sealed class ReportRoute : IRoute
{
    public string Key => DocumentTypes.Report;
    public int Version => 1;

    public void Build(IRouteBuilder r) => r
        .Step<IntakeStep>()
        .Step<AutoSignStep>()
        .Include(RouteFragments.Finish);
}
