using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorCommerce.Business.Services;
using NorCommerce.Business.Services.IServices;
using NorCommerce.DataAccess.Data;
using NorCommerce.DataAccess.Seed;
using NorCommerce.Infrastructure.Tripletex;
using NorCommerce.Models;
using NorCommerce.Utility;
using NorCommerceWeb.Workers;
using Stripe;
using System.Globalization;
using ProductService = NorCommerce.Business.Services.ProductService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLConnection")));

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAttachmentEmailSender, EmailSender>();

// Stripe: Scoped
builder.Services.AddScoped<Stripe.Checkout.SessionService>();
builder.Services.AddScoped<Stripe.RefundService>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Validate security stamp on every request
builder.Services.Configure<SecurityStampValidatorOptions>(o =>
{
    o.ValidationInterval = TimeSpan.Zero;
});

// Session config
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(100);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// Email config
builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Localizaton with resources
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

// Securing entire application with AutoValidateAntiforgeryTokenAttribute
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
}).AddViewLocalization();

// Tripletex configure
builder.Services.Configure<TripletexOptions>(
    builder.Configuration.GetSection("Tripletex"));
// Singelton, since we want the same to be accessed, object creation single
builder.Services.AddSingleton<TripletexSessionStore>();
builder.Services.AddHttpClient<IAccountingClient, TripletexClient>();
builder.Services.AddScoped<IAccountingService, AccountingService>();

// Background Service
builder.Services.AddHostedService<AccountingSyncWorker>();

var app = builder.Build();

// Seeding roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    foreach (var role in new[] { StaticDetails.RoleAdmin, StaticDetails.RoleCustomer, StaticDetails.RoleEmployee })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new ApplicationRole(role));
        }
    }
}

// Seeding demo data (Development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await DevDataSeeder.SeedAsync(db, userManager, logger);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Stripe Config APIKEY
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Setting app culture
var norwegian = new CultureInfo("nb-NO");
var english = new CultureInfo("en");

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture: norwegian, uiCulture: norwegian),
    SupportedCultures = new[] { norwegian },            // kr, dates stay Norwegian
    SupportedUICultures = new[] { norwegian, english }  // text: NO or EN
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Customer" })
    .WithStaticAssets();


app.Run();
