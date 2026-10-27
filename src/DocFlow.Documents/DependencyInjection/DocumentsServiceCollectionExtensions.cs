namespace DocFlow.Documents;

public static class DocumentsServiceCollectionExtensions
{
    /// <summary>
    /// Подключает бизнес-логику документов: настройки каталогов (секция Storage) и все маршруты, шаги и сканеры этой сборки.
    /// </summary>
    public static IServiceCollection AddDocFlowDocuments(
        this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.Configure<StorageOptions>(configuration.GetSection("Storage"));
        return services.AddDocFlowFlows(typeof(DocumentTypes).Assembly);
    }
}
