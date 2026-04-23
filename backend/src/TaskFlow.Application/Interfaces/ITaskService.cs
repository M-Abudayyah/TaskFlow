using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyList<TaskItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TaskItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TaskItemDto> CreateAsync(CreateTaskDto request, CancellationToken cancellationToken = default);

    Task<TaskItemDto?> UpdateAsync(Guid id, UpdateTaskDto request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
