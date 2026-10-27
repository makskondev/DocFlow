namespace DocFlow.Documents;

public sealed class StorageOptions
{
    /// <summary>Наша рабочая копия документа: {id}.pdf, {id}.orig.pdf, {id}.sig.</summary>
    public string WorkingDir { get; set; } = "./data/work";
    /// <summary>Сюда кладём PDF на подпись; сюда же пользователь кладёт {id}.sig.</summary>
    public string SignatureDir { get; set; } = "./data/signature";
    public string ResultDir { get; set; } = "./data/result";

    public string Work(Guid id, string suffix) => Path.Combine(WorkingDir, id + suffix);
    public string Signature(Guid id, string suffix) => Path.Combine(SignatureDir, id + suffix);
    public string Result(Guid id, string suffix) => Path.Combine(ResultDir, id + suffix);
}
