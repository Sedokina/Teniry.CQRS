using Microsoft.EntityFrameworkCore;
using Teniry.Cqrs.SampleApi.Domain;

namespace Teniry.Cqrs.SampleApi;

public class TodoDb(DbContextOptions<TodoDb> options) : DbContext(options) {
    public DbSet<Todo> Todos { get; set; }
}