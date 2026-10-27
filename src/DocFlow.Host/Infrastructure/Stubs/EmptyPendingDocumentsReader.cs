namespace DocFlow.Host;

public sealed class EmptyPendingDocumentsReader : IPendingDocumentsReader
{
    public Task<IReadOnlyList<PendingDocument>> ReadAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<PendingDocument>>(new List<PendingDocument>());
}
