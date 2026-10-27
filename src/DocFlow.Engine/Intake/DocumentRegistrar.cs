namespace DocFlow.Engine;

/// <summary>Создаёт процессы по найденным документам. Правила дедупликации и повторной обработки — здесь.</summary>
public sealed class DocumentRegistrar(
    IProcessRepository repo,
    RouteRegistry routes,
    IProcessAudit audit,
    TimeProvider clock,
    ILogger<DocumentRegistrar> log)
{
    private readonly object _gate = new();
    private readonly HashSet<string> _reportedInvalid = new();   // чтобы предупреждение не повторялось на каждом опросе

    /// <param name="pollStartedAtUtc">Момент начала опроса: строки источника — снимок на это время.</param>
    public async Task<IReadOnlyList<Guid>> RegisterAsync(
        IEnumerable<DocumentRegistration> batch, DateTime pollStartedAtUtc, CancellationToken ct)
    {
        var created = new List<Guid>();
        foreach (var d in batch)
        {
            if (!routes.TryGet(d.DocumentType, out var route))
            {
                log.LogWarning("Нет маршрута для типа '{Type}' (документ {Id}), пропускаем", d.DocumentType, d.ExternalId);
                continue;
            }

            var last = await repo.FindLatestAsync(d.SourceId, d.ExternalId, ct);
            int run;
            if (last is null) run = 1;
            else if (!last.IsTerminal) continue;                       // уже в работе (или ждёт оператора)
            else if (last.Status == ProcessStatus.Completed
                     && last.CompletedAt < pollStartedAtUtc) run = last.Run + 1;   // запись вернулась после завершения = переобработка
            else continue;                                             // Cancelled: только явный Restart; либо устаревший снимок

            // Неполные входные данные: процесс не создаём. Запись остаётся в источнике и подхватится, когда данные появятся.
            var key = $"{d.SourceId}/{d.ExternalId}";
            var missing = route.RequiredData
                .Where(k => d.Data is null || !d.Data.TryGetValue(k, out var v) || string.IsNullOrWhiteSpace(v))
                .ToList();
            if (missing.Count > 0)
            {
                bool first;
                lock (_gate) first = _reportedInvalid.Add(key);
                if (first)
                    log.LogWarning("Документ {Doc} не зарегистрирован: во входных данных нет полей: {Fields}", key, string.Join(", ", missing));
                continue;
            }
            lock (_gate) _reportedInvalid.Remove(key);

            var p = NewProcess(d, route, run);
            if (await repo.TryAddAsync(p, ct))
            {
                audit.Record(p.Id, p.CurrentNodeId, "Registered", $"{d.SourceId}/{d.ExternalId} run {run}");
                created.Add(p.Id);
            }
        }
        return created;
    }

    /// <summary>Оператор перезапускает отменённый процесс: новый Run с теми же исходными данными.</summary>
    public async Task<Guid?> RestartAsync(Guid cancelledProcessId, CancellationToken ct)
    {
        var old = await repo.GetAsync(cancelledProcessId, ct);
        if (old is null || old.Status != ProcessStatus.Cancelled || !routes.TryGet(old.RouteKey, out var route))
            return null;

        var p = NewProcess(new DocumentRegistration(old.SourceId, old.ExternalId, old.RouteKey, old.ContentPath, old.Data), route, old.Run + 1);
        if (!await repo.TryAddAsync(p, ct)) return null;
        audit.Record(p.Id, p.CurrentNodeId, "Registered", $"restart of {old.Id}");
        return p.Id;
    }

    private DocumentProcess NewProcess(DocumentRegistration d, CompiledRoute route, int run)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        return new DocumentProcess
        {
            SourceId = d.SourceId,
            ExternalId = d.ExternalId,
            Run = run,
            RouteKey = route.Key,
            ContentPath = d.ContentPath,
            Data = d.Data is null ? new Dictionary<string, string>() : new Dictionary<string, string>(d.Data),
            RouteVersion = route.Version,
            CurrentNodeId = route.First.Id,
            Status = ProcessStatus.Running,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
