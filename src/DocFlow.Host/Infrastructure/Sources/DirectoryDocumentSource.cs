namespace DocFlow.Host;

/// <summary>
/// Документ появился в каталоге. Файл считается готовым, когда размер и время записи не менялись между двумя опросами.
/// Исходный файл не перемещаем при регистрации: его заберёт IntakeStep после надёжного копирования (идемпотентно, без окна потери).
/// </summary>
public sealed class DirectoryDocumentSource(IFileStorage fs, IOptions<IntakeOptions> options) : IDocumentSource
{
    private Dictionary<string, (long Size, DateTime Stamp)> _seen = new();

    public string SourceId => "dir";

    public async Task<IReadOnlyList<DocumentRegistration>> PollAsync(CancellationToken ct)
    {
        var result = new List<DocumentRegistration>();
        var current = new Dictionary<string, (long Size, DateTime Stamp)>();

        foreach (var dir in options.Value.Directories)
        {
            var files = await fs.ListAsync(dir.Path, "*.pdf", ct);
            foreach (var f in files)
            {
                var snapshot = (f.Size, f.LastWriteUtc);
                current[f.FullPath] = snapshot;
                if (f.Size > 0 && _seen.TryGetValue(f.FullPath, out var prev) && prev == snapshot)
                    result.Add(new DocumentRegistration(SourceId, System.IO.Path.GetFileNameWithoutExtension(f.Name), dir.DocumentType, f.FullPath));
            }
        }

        _seen = current;
        return result;
    }

    public async Task AcknowledgeAsync(DocumentProcess process, CancellationToken ct)
    {
        if (!await fs.ExistsAsync(process.ContentPath, ct)) return;   // уже забрали

        var directory = System.IO.Path.GetDirectoryName(process.ContentPath)!;
        var name = System.IO.Path.GetFileNameWithoutExtension(process.ContentPath);
        await fs.MoveAsync(process.ContentPath,
            System.IO.Path.Combine(directory, "_processed", $"{name}.{process.Id}.pdf"), overwrite: true, ct);
    }
}
