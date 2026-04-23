using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public TaskService(AppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<TaskItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TaskItems.AsNoTracking();

        if (!_currentUserService.IsInRole("Admin"))
        {
            var currentUserId = _currentUserService.UserId ?? string.Empty;
            query = query.Where(t => t.CreatedByUserId == currentUserId);
        }

        var tasks = await query
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return tasks.Select(MapToDto).ToList();
    }

    public async Task<TaskItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TaskItems.AsNoTracking().Where(t => t.Id == id);

        if (!_currentUserService.IsInRole("Admin"))
        {
            var currentUserId = _currentUserService.UserId ?? string.Empty;
            query = query.Where(t => t.CreatedByUserId == currentUserId);
        }

        var task = await query.FirstOrDefaultAsync(cancellationToken);

        return task is null ? null : MapToDto(task);
    }

    public async Task<TaskItemDto> CreateAsync(CreateTaskDto request, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = TaskFlow.Domain.Enums.TaskStatus.Todo,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = _currentUserService.UserId ?? "system",
            DueDateUtc = request.DueDateUtc,
            UpdatedAtUtc = null
        };

        _dbContext.TaskItems.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(task);
    }

    public async Task<TaskItemDto?> UpdateAsync(Guid id, UpdateTaskDto request, CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        if (!_currentUserService.IsInRole("Admin"))
        {
            var currentUserId = _currentUserService.UserId ?? string.Empty;
            if (task.CreatedByUserId != currentUserId)
            {
                return null;
            }
        }

        if (request.Title is not null)
        {
            task.Title = request.Title;
        }

        if (request.Description is not null)
        {
            task.Description = request.Description;
        }

        if (request.Status.HasValue)
        {
            task.Status = request.Status.Value;
        }

        if (request.Priority.HasValue)
        {
            task.Priority = request.Priority.Value;
        }

        if (request.DueDateUtc.HasValue)
        {
            task.DueDateUtc = request.DueDateUtc.Value;
        }

        task.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(task);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            return false;
        }

        var task = await _dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task is null)
        {
            return false;
        }

        _dbContext.TaskItems.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static TaskItemDto MapToDto(TaskItem task)
    {
        return new TaskItemDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            CreatedAtUtc = task.CreatedAtUtc,
            CreatedByUserId = task.CreatedByUserId,
            DueDateUtc = task.DueDateUtc,
            UpdatedAtUtc = task.UpdatedAtUtc
        };
    }
}
