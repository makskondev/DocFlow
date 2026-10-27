namespace DocFlow.Documents;

/// <summary>ЗОП: штамп, отправка на подпись пользователю, ожидание подписи.</summary>
public sealed class ZopRoute : IRoute
{
    public string Key => DocumentTypes.Zop;
    public int Version => 1;

    // Данные для штампа должны прийти из источника (вьюхи); без них процесс не создаётся
    public IReadOnlyCollection<string> RequiredData => new[] { DataKeys.FullName, DataKeys.AgreementNumber, DataKeys.AgreementDate };

    public void Build(IRouteBuilder r) => r
        .Step<IntakeStep>()
        .Step<ApplyStampStep>()
        .Step<SendToSignatureStep>()
        .WaitFor(Signals.UserSignature, TimeSpan.FromDays(7))   // таймаут -> эскалация (письмо + оператор)
        .Include(RouteFragments.Finish);
}
