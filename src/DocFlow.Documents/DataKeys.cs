namespace DocFlow.Documents;

/// <summary>
/// Имена полей входных данных документа (колонки вьюхи → входные данные процесса → StepContext.Data).
/// ПРИМЕР: замените на реальные поля вашей вьюхи.
/// </summary>
public static class DataKeys
{
    public const string FullName = "FullName";
    public const string AgreementNumber = "AgreementNumber";
    public const string AgreementDate = "AgreementDate";   // ISO 8601: yyyy-MM-dd
}
