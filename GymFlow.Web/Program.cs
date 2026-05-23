using GymFlow.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddCustomSession();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCustomErrorHandling();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapRazorPages();

Console.WriteLine("WebApp is Listening...");
app.Run();