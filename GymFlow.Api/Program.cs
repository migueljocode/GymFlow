var builder = WebApplication.CreateBuilder(args);

// Register Controllers
builder.Services.AddControllers();

// Register GymFlow DAL (DbContext + Repositories + Seed Configuration)
builder.Services.AddGymFlowDal(builder.Configuration, builder.Environment);
builder.Services.AddGymFlowServices(); 

// Register OpenAPI
builder.Services.AddOpenApi();

// Register services via extension methods
builder.Services.AddAuthService();
builder.Services.AddBasicSeeder();

var app = builder.Build();

// Ensure basic users (coach & member) exist in all environments
await app.EnsureBasicUsersAsync();

// Development-specific seeding (rich test data)
await app.SeedDevelopmentDataAsync();

// Development pipeline (OpenAPI, Scalar)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Custom Authentication Middleware
app.UseMiddleware<BasicAuthMiddleware>();

app.UseAuthorization();
app.MapControllers();

Console.WriteLine("Api Server is Listening...");
app.Run();