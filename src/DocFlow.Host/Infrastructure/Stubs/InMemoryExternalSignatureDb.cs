using System.Collections.Concurrent;

namespace DocFlow.Host;

public sealed class InMemoryExternalSignatureDb : IExternalSignatureDb
{
    private readonly ConcurrentDictionary<string, byte[]> _items = new();
    public Task<bool> ExistsAsync(string documentId, CancellationToken ct) => Task.FromResult(_items.ContainsKey(documentId));
    public Task SaveAsync(string documentId, byte[] signature, CancellationToken ct)
    {
        _items[documentId] = signature;
        return Task.CompletedTask;
    }
}
