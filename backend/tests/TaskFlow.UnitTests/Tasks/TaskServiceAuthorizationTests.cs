using Microsoft.EntityFrameworkCore;
using Moq;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.UnitTests.Tasks;

public class TaskServiceAuthorizationTests
{
    [Fact]
    public async Task Admin_GetsAllTasks()
    {
        await using var dbContext = CreateDbContext();
        SeedTasks(dbContext);
        var service = CreateService(dbContext, userId: "admin-id", isAdmin: true);

        var result = await service.GetAllAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task User_GetsOnlyOwnTasks()
    {
        await using var dbContext = CreateDbContext();
        SeedTasks(dbContext);
        var service = CreateService(dbContext, userId: "user-1", isAdmin: false);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal("user-1", t.CreatedByUserId));
    }

    [Fact]
    public async Task User_CannotAccessTask_ThatDoesNotBelongToThem()
    {
        await using var dbContext = CreateDbContext();
        SeedTasks(dbContext);
        var targetTaskId = dbContext.TaskItems.Single(t => t.CreatedByUserId == "user-2").Id;
        var service = CreateService(dbContext, userId: "user-1", isAdmin: false);

        var result = await service.GetByIdAsync(targetTaskId);

        Assert.Null(result);
    }

    [Fact]
    public async Task User_CannotUpdateTask_ThatDoesNotBelongToThem()
    {
        await using var dbContext = CreateDbContext();
        SeedTasks(dbContext);
        var target = dbContext.TaskItems.Single(t => t.CreatedByUserId == "user-2");
        var service = CreateService(dbContext, userId: "user-1", isAdmin: false);

        var result = await service.UpdateAsync(target.Id, new UpdateTaskDto { Title = "Hacked title" });

        Assert.Null(result);

        var unchanged = await dbContext.TaskItems.SingleAsync(t => t.Id == target.Id);
        Assert.Equal("Task 3", unchanged.Title);
    }

    [Fact]
    public async Task Admin_CanUpdateAnyTask()
    {
        await using var dbContext = CreateDbContext();
        SeedTasks(dbContext);
        var target = dbContext.TaskItems.Single(t => t.CreatedByUserId == "user-2");
        var service = CreateService(dbContext, userId: "admin-id", isAdmin: true);

        var result = await service.UpdateAsync(target.Id, new UpdateTaskDto { Title = "Updated by admin" });

        Assert.NotNull(result);
        Assert.Equal("Updated by admin", result!.Title);
    }

    [Fact]
    public async Task Admin_CanDeleteAnyTask()
    {
        await using var dbContext = CreateDbContext();
        SeedTasks(dbContext);
        var targetId = dbContext.TaskItems.Single(t => t.CreatedByUserId == "user-2").Id;
        var service = CreateService(dbContext, userId: "admin-id", isAdmin: true);

        var deleted = await service.DeleteAsync(targetId);

        Assert.True(deleted);
        Assert.False(await dbContext.TaskItems.AnyAsync(t => t.Id == targetId));
    }

    [Fact]
    public async Task NonAdmin_CannotDelete_WhenDeleteIsAdminOnly()
    {
        await using var dbContext = CreateDbContext();
        SeedTasks(dbContext);
        var targetId = dbContext.TaskItems.First(t => t.CreatedByUserId == "user-1").Id;
        var service = CreateService(dbContext, userId: "user-1", isAdmin: false);

        var deleted = await service.DeleteAsync(targetId);

        Assert.False(deleted);
        Assert.True(await dbContext.TaskItems.AnyAsync(t => t.Id == targetId));
    }

    [Fact]
    public async Task CreateAsync_StoresCreatedByUserId_FromCurrentUser()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext, userId: "user-42", isAdmin: false);

        var created = await service.CreateAsync(new CreateTaskDto
        {
            Title = "Created task",
            Description = "desc",
            Priority = TaskPriority.High
        });

        Assert.Equal("user-42", created.CreatedByUserId);

        var inDb = await dbContext.TaskItems.SingleAsync(t => t.Id == created.Id);
        Assert.Equal("user-42", inDb.CreatedByUserId);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static TaskService CreateService(AppDbContext dbContext, string userId, bool isAdmin)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        currentUser.Setup(x => x.IsInRole("Admin")).Returns(isAdmin);

        return new TaskService(dbContext, currentUser.Object);
    }

    private static void SeedTasks(AppDbContext dbContext)
    {
        dbContext.TaskItems.AddRange(
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Task 1",
                Status = TaskFlow.Domain.Enums.TaskStatus.Todo,
                Priority = TaskPriority.Medium,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-30),
                CreatedByUserId = "user-1"
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Task 2",
                Status = TaskFlow.Domain.Enums.TaskStatus.InProgress,
                Priority = TaskPriority.Low,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-20),
                CreatedByUserId = "user-1"
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Task 3",
                Status = TaskFlow.Domain.Enums.TaskStatus.Done,
                Priority = TaskPriority.High,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10),
                CreatedByUserId = "user-2"
            });

        dbContext.SaveChanges();
    }
}
