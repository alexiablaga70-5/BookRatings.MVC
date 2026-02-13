using BookRatings.gRPC;
using BookRatings.MVC.Data; // dacă așa se numește namespace-ul tău pentru DbContext
using BookRatings.MVC.Services;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ML API (dacă îl ai deja)
builder.Services.AddHttpClient<PredictionService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5191/");
});
// gRPC client (adresa = portul din launchSettings.json al BookRatings.gRPC)
builder.Services.AddGrpcClient<RatingsGrpc.RatingsGrpcClient>(o =>
{
    o.Address = new Uri("https://localhost:7281"); // <-- schimbă cu portul tău gRPC
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();