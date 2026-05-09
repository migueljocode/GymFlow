var builder = WebApplication.CreateBuilder(args);

// Register Controllers
builder.Services.AddControllers();

// Register GymFlow DAL (DbContext + Repositories + Seed Configuration)
builder.Services.AddGymFlowDal(builder.Configuration, builder.Environment);

// Register OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Development Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await app.Services.EnsureDatabaseSeededAsync();
}

// Production Pipeline
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();