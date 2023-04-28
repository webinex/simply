using Microsoft.EntityFrameworkCore;
using Webinex.Simply;
using Webinex.Simply.AspNetCore;
using Webinex.Simply.Example;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ExampleDbContext>(x => x
    .UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSimply<User>(x => x
    .AddController("/api/user")
    .AddDbContext<ExampleDbContext>());

builder.Services.AddSimply<Company>(x => x
    .AddController("/api/company")
    .AddDbContext<ExampleDbContext>());

builder.Services
    .AddScoped<IUniqueEmailPolicy, UniqueEmailPolicy>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();