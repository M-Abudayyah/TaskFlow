using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
}
