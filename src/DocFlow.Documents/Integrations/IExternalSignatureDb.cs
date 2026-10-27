namespace DocFlow.Documents;

/// <summary>Сторонняя БД: запись подписи = «коммит»; после неё документ исчезает из вьюхи.</summary>
public interface IExternalSignatureDb
{
    Task<bool> ExistsAsync(string documentId, CancellationToken ct);
    Task SaveAsync(string documentId, byte[] signature, CancellationToken ct);
}
