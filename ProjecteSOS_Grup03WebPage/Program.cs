using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

await Task.Delay(3000);

// Add services to the container.
builder.Services.AddRazorPages();

string apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? throw new InvalidOperationException("API not found");

builder.Services.AddHttpClient("GameApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login";
                options.AccessDeniedPath = "/AccessDenied";
            });
builder.Services.AddSession();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
