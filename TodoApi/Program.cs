

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using TodoApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(b =>
{
    b.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Version = "v1" });
});
var connectionString = builder.Configuration.GetConnectionString("ToDoDB");

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), ServerVersion.Parse("8.0.36-mysql")),
    ServiceLifetime.Singleton);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
    builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");

app.MapGet("/items", GetAllTasks);
app.MapPost("/items", AddTask);
app.MapPut("/items/{id}", UpdateTask);
app.MapDelete("/items/{id}", DeleteTask);

app.MapMethods("/options-or-head", new[] { "OPTIONS", "HEAD" },
    () => "This is an options or head request ");

app.Run();

async Task GetAllTasks(ToDoDbContext dbContext, HttpContext context)
{
    var tasks = await dbContext.Items.ToListAsync();
    await context.Response.WriteAsJsonAsync(tasks);
}

async Task AddTask(ToDoDbContext dbContext, HttpContext context, Item item)
{
    item.IsComplete = false;
    dbContext.Add(item);
    await dbContext.SaveChangesAsync();
    context.Response.StatusCode = StatusCodes.Status201Created;
    await context.Response.WriteAsJsonAsync(item);
}

async Task DeleteTask(ToDoDbContext dbContext, HttpContext context, int id)
{
    var exist = await dbContext.Items.FindAsync(id);
    if (exist != null)
    {
        dbContext.Items.Remove(exist);
        await dbContext.SaveChangesAsync();
        context.Response.StatusCode = StatusCodes.Status200OK;
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
}

async Task UpdateTask(ToDoDbContext dbContext, HttpContext context, int id, Item item)
{
    if (item == null)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync("the data isnt valid");
        return;
    }
    var exist = await dbContext.Items.FindAsync(id);
    if (exist == null)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    if (item.Name != null)
        exist.Name = item.Name;
    exist.IsComplete = item.IsComplete;
    await dbContext.SaveChangesAsync();
    context.Response.StatusCode = StatusCodes.Status200OK;
}
