using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Teniry.Cqrs;
using Teniry.Cqrs.ApplicationEvents;
using Teniry.Cqrs.SampleApi;
using Teniry.Cqrs.SampleApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TodoDb>(options => options.UseNpgsql(connectionString));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options => {
        // Add swagger documentation from an assembly xml file
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    }
);

builder.Services.AddCqrs();
builder.Services.AddApplicationEvents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("todo", Todos.GetTodosAsync);
app.MapGet("todo/toComplete", Todos.GetTodosToCompleteAsync);
app.MapGet("todo/{id:guid}", Todos.GetTodoAsync);
app.MapPost("todo/create", Todos.CreateTodoAsync);
app.MapPut("todo/{id:guid}/complete", Todos.CompleteTodoAsync);

app.Run();