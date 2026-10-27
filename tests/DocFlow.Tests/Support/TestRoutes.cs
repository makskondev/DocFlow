namespace DocFlow.Tests;

public sealed class LinearRoute : IRoute
{
    public string Key => "linear";
    public int Version => 1;
    public void Build(IRouteBuilder r) => r.Step<StepA>().Step<StepB>();
}

public sealed class WaitRoute : IRoute
{
    public string Key => "wait";
    public int Version => 1;
    public void Build(IRouteBuilder r) => r.Step<StepA>().WaitFor("go", TimeSpan.FromHours(1)).Step<StepB>();
}

public sealed class DataRoute : IRoute
{
    public string Key => "data";
    public int Version => 1;
    public IReadOnlyCollection<string> RequiredData => new[] { "fio" };
    public void Build(IRouteBuilder r) => r.Step<StepA>();
}
