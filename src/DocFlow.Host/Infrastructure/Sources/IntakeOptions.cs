namespace DocFlow.Host;

public sealed class IntakeDirectory
{
    public string Path { get; set; } = "";
    /// <summary>Тип документа = ключ маршрута.</summary>
    public string DocumentType { get; set; } = "";
}

public sealed class IntakeOptions
{
    public List<IntakeDirectory> Directories { get; set; } = new();
}
