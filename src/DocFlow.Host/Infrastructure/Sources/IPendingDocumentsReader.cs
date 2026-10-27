namespace DocFlow.Host;

/// <param name="Data">
/// Дополнительные колонки вьюхи, нужные для обработки (например, ФИО, даты, номер соглашения для штампа).
/// Ключи — имена из <see cref="DataKeys"/>; значения — строки (даты — ISO 8601, числа — InvariantCulture).
/// </param>
public sealed record PendingDocument(string Id, string DocumentType, string FilePath, IReadOnlyDictionary<string, string>? Data = null);

/// <summary>Читает вьюху с документами к обработке (SELECT ... FROM вьюха). Реализуйте под вашу СУБД.</summary>
public interface IPendingDocumentsReader
{
    Task<IReadOnlyList<PendingDocument>> ReadAsync(CancellationToken ct);
}
