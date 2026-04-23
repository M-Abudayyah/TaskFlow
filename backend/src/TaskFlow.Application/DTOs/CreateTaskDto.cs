using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDateUtc { get; set; }
}
