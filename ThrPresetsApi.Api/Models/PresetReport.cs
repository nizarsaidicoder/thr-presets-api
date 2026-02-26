namespace ThrPresetsApi.Api.Models;

public enum ReportReason { BrokenFile, IncorrectInfo, Duplicate, Inappropriate, Other }

public class PresetReport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ReportReason Reason { get; set; }
    public string? Details { get; set; }
    public bool Resolved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? ReporterId { get; set; }
    public User? Reporter { get; set; }

    public string PresetId { get; set; } = null!;
    public Preset Preset { get; set; } = null!;
}