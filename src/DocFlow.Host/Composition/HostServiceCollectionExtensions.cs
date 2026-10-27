namespace DocFlow.Host;

public static class HostServiceCollectionExtensions
{
    /// <summary>
    /// Подключает инфраструктуру: файловое хранилище, источники документов, внешние системы.
    /// Всё, что в папке Infrastructure/Stubs — заглушки: замените реальными реализациями (см. docs/before-production.md).
    /// </summary>
    public static IServiceCollection AddDocFlowHostInfrastructure(
        this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.Configure<IntakeOptions>(configuration.GetSection("Intake"));

        services.AddSingleton<IFileStorage, LocalFileStorage>();

        // Источники документов
        services.AddSingleton<IDocumentSource, DirectoryDocumentSource>();
        services.AddSingleton<IDocumentSource, ViewDocumentSource>();

        // Реальные реализации
        services.AddSingleton<IEscalationNotifier, LogEscalationNotifier>();   // TODO: письмо (SMTP)

        // ЗАГЛУШКИ — заменить
        services.AddSingleton<IPendingDocumentsReader, EmptyPendingDocumentsReader>();    // SELECT из вьюхи
        services.AddSingleton<IExternalSignatureDb, InMemoryExternalSignatureDb>();       // запись подписи во внешнюю БД
        services.AddSingleton<ISigningService, FakeSigningService>();                     // REST-сервис подписи
        services.AddSingleton<ISignatureVerifier, FakeSignatureVerifier>();               // REST-API проверки подписи
        return services;
    }
}
