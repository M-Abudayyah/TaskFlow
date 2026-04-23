using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs;

public class UpdateTaskDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public TaskFlow.Domain.Enums.TaskStatus? Status { get; set; }

    public TaskPriority? Priority { get; set; }

    public DateTime? DueDateUtc { get; set; }
}
