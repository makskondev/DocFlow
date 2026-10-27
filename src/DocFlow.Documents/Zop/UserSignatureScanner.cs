namespace DocFlow.Documents;

/// <summary>
/// Ищет {ProcessId}.sig в каталоге подписи для процессов, ожидающих сигнал <see cref="Signals.UserSignature"/>.
/// Файл считается готовым, когда размер и время записи не менялись между двумя сканами.
/// </summary>
public sealed class UserSignatureScanner(
    IFileStorage fs,
    IOptions<StorageOptions> storage,
    IProcessRepository repo,
    ProcessEngine engine,
    ILogger<UserSignatureScanner> log) : ISignalSource
{
    private Dictionary<string, (long Size, DateTime Stamp)> _seen = new();

    public string Signal => Signals.UserSignature;

    public async Task ScanAsync(CancellationToken ct)
    {
        var waiting = await repo.ListWaitingAsync(Signal, ct);
        if (waiting.Count == 0)
        {
            _seen.Clear();
            return;
        }

        // Один листинг на скан; ключ по Guid, дубли имён (регистр) скан не роняют
        var byId = new Dictionary<Guid, FileEntry>();
        foreach (var file in await fs.ListAsync(storage.Value.SignatureDir, "*.sig", ct))
            if (Guid.TryParse(Path.GetFileNameWithoutExtension(file.Name), out var id))
                byId[id] = file;

        var current = new Dictionary<string, (long Size, DateTime Stamp)>();
        foreach (var process in waiting)
        {
            if (!byId.TryGetValue(process.Id, out var entry)) continue;

            var snapshot = (entry.Size, entry.LastWriteUtc);
            current[entry.FullPath] = snapshot;

            if (entry.Size > 0 && _seen.TryGetValue(entry.FullPath, out var prev) && prev == snapshot
                && await engine.SignalAsync(process.Id, Signal, ct))
            {
                log.LogInformation("Подпись получена для процесса {Id}", process.Id);
            }
        }
        _seen = current;
    }
}
