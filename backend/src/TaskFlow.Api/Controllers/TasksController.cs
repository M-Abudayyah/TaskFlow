using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ICurrentUserService _currentUserService;

    public TasksController(ITaskService taskService, ICurrentUserService currentUserService)
    {
        _taskService = taskService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var tasks = await _taskService.GetAllAsync(cancellationToken);

        if (_currentUserService.IsInRole("Admin"))
        {
            return Ok(tasks);
        }

        var currentUserId = _currentUserService.UserId;
        var ownTasks = tasks
            .Where(task => task.CreatedByUserId == currentUserId)
            .ToList();

        return Ok(ownTasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskItemDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        if (_currentUserService.IsInRole("Admin") || task.CreatedByUserId == _currentUserService.UserId)
        {
            return Ok(task);
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> Create([FromBody] CreateTaskDto request, CancellationToken cancellationToken)
    {
        var createdTask = await _taskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskItemDto>> Update(Guid id, [FromBody] UpdateTaskDto request, CancellationToken cancellationToken)
    {
        var existingTask = await _taskService.GetByIdAsync(id, cancellationToken);
        if (existingTask is null)
        {
            return NotFound();
        }

        if (!_currentUserService.IsInRole("Admin") && existingTask.CreatedByUserId != _currentUserService.UserId)
        {
            return NotFound();
        }

        var updatedTask = await _taskService.UpdateAsync(id, request, cancellationToken);
        if (updatedTask is null)
        {
            return NotFound();
        }

        return Ok(updatedTask);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _taskService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
