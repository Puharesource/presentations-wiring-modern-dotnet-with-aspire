using Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.DbContexts;

public class DemoDbContext(DbContextOptions<DemoDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
}
