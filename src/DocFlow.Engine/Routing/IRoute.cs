namespace DocFlow.Engine;

/// <summary>Маршрут обработки документов одного типа. Один класс = один файл = один маршрут.</summary>
public interface IRoute
{
    /// <summary>Ключ маршрута = тип документа.</summary>
    string Key { get; }
    /// <summary>Увеличивайте при изменении состава узлов.</summary>
    int Version { get; }
    void Build(IRouteBuilder r);

    /// <summary>
    /// Поля входных данных документа, без которых маршрут не стартует. Проверяется при регистрации:
    /// документ с неполными данными процесс не создаёт (остаётся в источнике, в лог пишется предупреждение).
    /// </summary>
    IReadOnlyCollection<string> RequiredData => Array.Empty<string>();
}
