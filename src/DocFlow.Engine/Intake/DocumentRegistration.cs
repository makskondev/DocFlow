namespace DocFlow.Engine;

/// <summary>Документ, обнаруженный источником. DocumentType = ключ маршрута.</summary>
/// <param name="Data">Дополнительные данные для обработки (например, для штампа). Значения — строки; даты — ISO 8601, числа — инвариантная культура.</param>
public sealed record DocumentRegistration(
    string SourceId, string ExternalId, string DocumentType, string ContentPath,
    IReadOnlyDictionary<string, string>? Data = null);
