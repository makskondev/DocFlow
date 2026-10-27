namespace DocFlow.Documents;

/// <summary>Перенести PDF и подпись в каталог результата и убрать рабочие копии. Повтор безопасен.</summary>
public sealed class MoveToResultStep(IFileStorage fs, IOptions<StorageOptions> storage) : IStep
{
    public async Task ExecuteAsync(StepContext ctx, CancellationToken ct)
    {
        var s = storage.Value;
        ctx.Bag[BagKeys.PdfPath] = await MoveAsync(ctx.Bag[BagKeys.PdfPath], s.Result(ctx.Id, ".pdf"), ct);
        ctx.Bag[BagKeys.SigPath] = await MoveAsync(ctx.Bag[BagKeys.SigPath], s.Result(ctx.Id, ".sig"), ct);

        foreach (var suffix in new[] { ".pdf", ".orig.pdf", ".sig" })
            await fs.DeleteAsync(s.Work(ctx.Id, suffix), ct);
    }

    private async Task<string> MoveAsync(string source, string destination, CancellationToken ct)
    {
        if (source == destination) return destination;
        if (await fs.ExistsAsync(source, ct))
            await fs.MoveAsync(source, destination, overwrite: true, ct);
        else if (!await fs.ExistsAsync(destination, ct))
            throw new FileNotFoundException($"Нет ни источника, ни результата: {source}");
        return destination;   // источника нет, результат есть = уже перенесено на прошлой попытке
    }
}
