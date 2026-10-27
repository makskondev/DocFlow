namespace DocFlow.Documents;

/// <summary>
/// Запись подписи во внешнюю БД = «коммит»: после неё запись исчезает из вьюхи. Поэтому шаг последний:
/// к этому моменту файлы уже лежат в результате. Идемпотентно: если подпись уже записана — ничего не делаем.
/// </summary>
public sealed class PersistSignatureStep(IFileStorage fs, IExternalSignatureDb db) : IStep
{
    public async Task ExecuteAsync(StepContext ctx, CancellationToken ct)
    {
        if (await db.ExistsAsync(ctx.ExternalId, ct)) return;
        await db.SaveAsync(ctx.ExternalId, await fs.ReadAsync(ctx.Bag[BagKeys.SigPath], ct), ct);
    }
}
