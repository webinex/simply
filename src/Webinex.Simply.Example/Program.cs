using Microsoft.EntityFrameworkCore;
using Webinex.Asky;
using Webinex.Simply;
using Webinex.Simply.AspNetCore;
using Webinex.Simply.Example;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<StatusPolicy>();

builder.Services.AddDbContext<ExampleDbContext>(x => x
    .UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSingleton<IAskyFieldMap<User>, UserFieldMap>();
// builder.Services.AddSimply<User>(x => x
//     .UseDbContext<ExampleDbContext>()
//     .AddController(c => c
//         .UseRoute("/api/user")
//         .MapDefaultGet<UserDto>()
//         .MapDefaultGetMany<UserDto>()
//         .MapCreate("inactive", "CreateInactiveAsync")
//         .MapUpdate("status", "UpdateStatusAsync")));

builder.Services.AddSimply<User>(x => x
    .UseDbContext<ExampleDbContext>()
    .AddController(c => c
        .UseRoute("/api/user")
        .MapUpdate("status", "UpdateStatusAsync")
        .MapCreate("inactive", "CreateInactiveAsync")
        .MapDefaultGet<UserDto>()
        .MapGetAll<UserLookupDto>("lookup", "GetLookups")));

builder.Services.AddSimply<Company>(x => x
    .UseDbContext<ExampleDbContext>()
    .AddController("/api/company"));

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