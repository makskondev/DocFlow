namespace DocFlow.Documents;

/// <summary>Общий хвост всех маршрутов: проверка подписи, перенос в результат, запись во внешнюю БД (последний шаг = «коммит»).</summary>
public static class RouteFragments
{
    public static void Finish(IRouteBuilder r) => r
        .Step<VerifySignatureStep>()
        .Step<MoveToResultStep>()
        .Step<PersistSignatureStep>();
}
