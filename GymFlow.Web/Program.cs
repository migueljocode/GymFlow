var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();

// Register HttpClient as singleton
builder.Services.AddHttpClient();

// Register ApiClient as scoped (with HttpClient factory injection)
builder.Services.AddScoped<ApiClient>();

// اضافه کردن Session برای ذخیره نقش کاربر
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}


// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapRazorPages();

Console.WriteLine("WebApp is Listening...");
app.Run();