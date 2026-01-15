using FileConverter.BLL.Services;
using FileConverter.BLL.Services.Interfaces;
using FileConverter.Data;
using FileConverter.Data.Repositories;
using FileConverter.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ DbContext cu connection string din appsettings.json
builder.Services.AddDbContext<FileConverterDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ✅ Repositories
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// dacă le ai în proiect, adaugă și astea (altfel nu):
// builder.Services.AddScoped<IUserPlanRepository, UserPlanRepository>();
// builder.Services.AddScoped<IConversionRepository, ConversionRepository>();
// builder.Services.AddScoped<IMonthlyUsageRepository, MonthlyUsageRepository>();

// ✅ Services (o singură dată)
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// ✅ Cookie Auth pentru Admin Panel
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Account/Login";
		options.AccessDeniedPath = "/Account/Login";
		options.ExpireTimeSpan = TimeSpan.FromHours(8);
		options.SlidingExpiration = true;
	});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
