using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskFlow.Domain.Enums.TaskStatus Status { get; set; } = TaskFlow.Domain.Enums.TaskStatus.Todo;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime CreatedAtUtc { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTime? DueDateUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
