namespace DocFlow.Host;

/// <summary>
/// Вьюха сторонней БД — это очередь: пока запись есть, документ ждёт обработки; исчезает она после записи подписи.
/// Подтверждать получение нечем (только чтение), поэтому защита от дублей — уникальность в регистраторе.
/// </summary>
public sealed class ViewDocumentSource(IPendingDocumentsReader reader) : IDocumentSource
{
    public string SourceId => "view";

    public async Task<IReadOnlyList<DocumentRegistration>> PollAsync(CancellationToken ct)
    {
        var rows = await reader.ReadAsync(ct);
        return rows.Select(r => new DocumentRegistration(SourceId, r.Id, r.DocumentType, r.FilePath, r.Data)).ToList();
    }

    public Task AcknowledgeAsync(DocumentProcess process, CancellationToken ct) => Task.CompletedTask;
}
