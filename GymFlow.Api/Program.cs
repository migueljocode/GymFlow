using GymFlow.Dal.Context;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using GymFlow.Services.Extensions;
using GymFlow.Services.Interfaces;
using GymFlow.Services.Implementations;
using GymFlow.Api.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register Controllers
builder.Services.AddControllers();

// Register GymFlow DAL (DbContext + Repositories + Seed Configuration)
builder.Services.AddGymFlowDal(builder.Configuration, builder.Environment);

builder.Services.AddGymFlowServices(); 

// Register OpenAPI
builder.Services.AddOpenApi();

builder.Services.AddScoped<IAuthService>(provider =>
{
    var factory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    return new AuthService(factory);
});

var app = builder.Build();

// Development Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await app.Services.EnsureDatabaseSeededAsync();
}

// *** اضافه کردن Middleware سفارشی احراز هویت ***
app.UseMiddleware<BasicAuthMiddleware>();

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("Api Server is Listening...");
app.Run();