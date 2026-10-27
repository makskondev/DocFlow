namespace DocFlow.Documents;

/// <summary>Обязательная проверка подписи. Невалидная подпись или подменённый PDF — повторять бессмысленно (эскалация).</summary>
public sealed class VerifySignatureStep(IFileStorage fs, IOptions<StorageOptions> storage, ISignatureVerifier verifier) : IStep
{
    public async Task ExecuteAsync(StepContext ctx, CancellationToken ct)
    {
        var s = storage.Value;
        var pdfPath = ctx.Bag.TryGetValue(BagKeys.PdfPath, out var pp) ? pp : s.Signature(ctx.Id, ".pdf");
        var sigPath = ctx.Bag.TryGetValue(BagKeys.SigPath, out var sp) ? sp : s.Signature(ctx.Id, ".sig");

        var pdf = await fs.ReadAsync(pdfPath, ct);
        var signature = await fs.ReadAsync(sigPath, ct);   // нет файла = обычная ошибка (возможен лаг SMB): повторится

        // Файл не должен измениться с момента отправки на подпись
        if (ctx.Bag.TryGetValue(BagKeys.Sha256, out var expected) && Hashing.Sha256Hex(pdf) != expected)
            throw new PermanentStepException("PDF отличается от отправленного на подпись (хеш не совпал)");

        var result = await verifier.VerifyAsync(pdf, signature, ct);
        if (!result.IsValid)
            throw new PermanentStepException($"Подпись не прошла проверку: {result.Reason}");

        ctx.Bag[BagKeys.SigPath] = sigPath;
    }
}
