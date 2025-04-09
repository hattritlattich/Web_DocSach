using DocumentWebsite.Data;
using DocumentWebsite.Models.Mail;
using DocumentWebsite.Models;
using DocumentWebsite.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// XỬ LÝ CONFILCT
// Add services to the container.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// DB----------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ID----------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddDefaultTokenProviders().AddDefaultUI().AddEntityFrameworkStores<ApplicationDbContext>();

Console.WriteLine("Hello from branch1!");

// Google Authentication ------------------
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
//    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
//})
//.AddCookie()
//.AddGoogle(options =>
//{
//    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//});
// Xử Lý Conflicts khi đăng nhập bằng Google 11111
// MAIL SERVER ------------------
//// Cấu hình dịch vụ gửi mail, giá trị Inject từ appsettings.json
//builder.Services.AddOptions();                                              // Kích hoạt Options
//var mailSettingsSection = builder.Configuration.GetSection("MailSettings"); // đọc config
//builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));    // đăng ký để Inject
//builder.Services.AddTransient<IEmailSender, SendMailService>();             // Đăng ký dịch vụ Mail

// ------------------------------
builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();

// REPO------------------------------
builder.Services.AddScoped<IDocumentRepo, EFDocumentRepo>();
builder.Services.AddScoped<ICategoryRepo, EFCategoryRepo>();
builder.Services.AddScoped<IFavoriteRepo, EFFavoriteRepo>();
builder.Services.AddScoped<ICommentRepo, EFCommentRepo>();
builder.Services.AddScoped<IRatingRepo, EFRatingRepo>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

// Use session before routing ---------------
app.UseSession();

app.UseRouting();
// Routing and Authentication --------------
app.UseAuthentication();

app.UseAuthorization();

// -----------------
app.MapRazorPages();

// area---------------
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
