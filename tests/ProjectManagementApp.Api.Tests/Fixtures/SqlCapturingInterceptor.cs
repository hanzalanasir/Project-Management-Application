using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ProjectManagementApp.Api.Tests.Fixtures;

// Captures executed command text so a test can inspect the real generated SQL (T027) — the only
// way to prove, at the HTTP level, that scope is composed as `WHERE project_id IN (<subquery>)`
// rather than fetched then filtered in memory. Sibling to CommandCounterInterceptor; same
// sync+async override requirement applies (every request here uses async EF calls).
public class SqlCapturingInterceptor : DbCommandInterceptor
{
    private readonly ConcurrentQueue<string> _commandTexts = new();

    public IReadOnlyCollection<string> CommandTexts => _commandTexts.ToArray();

    public void Reset() => _commandTexts.Clear();

    public override InterceptionResult<System.Data.Common.DbDataReader> ReaderExecuting(
        System.Data.Common.DbCommand command, CommandEventData eventData, InterceptionResult<System.Data.Common.DbDataReader> result)
    {
        _commandTexts.Enqueue(command.CommandText);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<System.Data.Common.DbDataReader>> ReaderExecutingAsync(
        System.Data.Common.DbCommand command, CommandEventData eventData, InterceptionResult<System.Data.Common.DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        _commandTexts.Enqueue(command.CommandText);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}
