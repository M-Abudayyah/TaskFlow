using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs;

public class TaskItemDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskFlow.Domain.Enums.TaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTime? DueDateUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
