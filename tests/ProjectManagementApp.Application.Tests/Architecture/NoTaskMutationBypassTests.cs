using FluentAssertions;

namespace ProjectManagementApp.Application.Tests.Architecture;

// The belt-and-braces guarantee (spec 003 T.2): every write handler under Features/Tasks/ must call
// CanMutateAsync — structural narrowness (narrow request DTOs) is only half the graduated model;
// this is the behavioural half, and it must not be droppable by a future slice. Source-scan pattern
// mirrors Api.Tests' NoInlineRoleChecksTests exactly (same repo-root resolution, same rationale for
// why a text scan beats reflection here: a missing call is invisible to a type check).
public class NoTaskMutationBypassTests
{
    [Fact]
    public void EveryTaskCommandHandler_CallsCanMutateAsync()
    {
        var repoRoot = FindRepoRoot();
        var tasksFeaturesDir = Path.Combine(repoRoot, "src", "ProjectManagementApp.Application", "Features", "Tasks");
        Directory.Exists(tasksFeaturesDir).Should().BeTrue($"expected {tasksFeaturesDir} to exist");

        var handlerFiles = Directory.EnumerateFiles(tasksFeaturesDir, "*CommandHandler.cs", SearchOption.AllDirectories).ToList();

        // Sanity floor — if this drops, the glob or the directory layout broke, not the invariant.
        handlerFiles.Should().HaveCountGreaterThanOrEqualTo(5,
            "CreateTask, UpdateTask, UpdateTaskStatus, DeleteTask, and ReassignTask handlers must all exist under Features/Tasks/");

        var offending = handlerFiles
            .Where(file => !File.ReadAllText(file).Contains("CanMutateAsync"))
            .ToList();

        offending.Should().BeEmpty(
            "every *CommandHandler.cs under Features/Tasks/ must call ITaskAccessPolicy.CanMutateAsync — " +
            "a handler that skips it would bypass the graduated authorization model entirely, not just narrow it");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ProjectManagementApp.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Could not locate repo root (ProjectManagementApp.slnx not found).");
    }
}
