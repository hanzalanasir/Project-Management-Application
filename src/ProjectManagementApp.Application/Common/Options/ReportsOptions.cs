namespace ProjectManagementApp.Application.Common.Options;

// Declared in Application, not Api/Configuration (tasks.md T005's literal location) — same
// relocation 002/003/004/005 already made for their own options classes, for the identical reason:
// every value here is consumed by a slice handler and Application MUST NOT reference Api
// (Constitution II.2). Api still owns the services.Configure<ReportsOptions>(...) binding call in
// Program.cs; only the class itself moved.
//
// There is deliberately NO TimeZone knob here. Every window boundary and bucket is fixed to UTC in
// ReportWindow/ReportCountingRules — a configurable timezone would let a deployment break NFR-002's
// value-parity requirement against 005's Dashboard (research R-4).
public class ReportsOptions
{
    public int DefaultWindowDays { get; set; } = 30;
    public int MaxWindowDays { get; set; } = 366;
    public ActivityOptions Activity { get; set; } = new();
    public int LargeReportRowThreshold { get; set; } = 10000;
    public string LargeReportFallback { get; set; } = "ForceNarrow";
    public string[] EnabledTypes { get; set; } = ["ProjectProgress", "TaskCompletion", "TeamPerformance", "Activity"];
    public bool AuditOnGeneration { get; set; } = true;
    public bool MaskOutOfScopeAs404 { get; set; }
    public string DownloadFilenamePattern { get; set; } = "{reportType}_{from:yyyy-MM-dd}_{to:yyyy-MM-dd}";

    public class ActivityOptions
    {
        public int DefaultPageSize { get; set; } = 20;
        public int MaxPageSize { get; set; } = 100;
    }
}
