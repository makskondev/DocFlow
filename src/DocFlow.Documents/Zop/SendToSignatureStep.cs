namespace DocFlow.Documents;

/// <summary>Положить PDF в каталог подписи. Дальше процесс ждёт {id}.sig от пользователя.</summary>
public sealed class SendToSignatureStep(IFileStorage fs, IOptions<StorageOptions> storage) : IStep
{
    public async Task ExecuteAsync(StepContext ctx, CancellationToken ct)
    {
        var s = storage.Value;
        var destination = s.Signature(ctx.Id, ".pdf");
        await fs.CopyAsync(s.Work(ctx.Id, ".pdf"), destination, overwrite: true, ct);
        ctx.Bag[BagKeys.PdfPath] = destination;
    }
}
