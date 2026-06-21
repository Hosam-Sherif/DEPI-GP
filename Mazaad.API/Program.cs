// Mazaad.API/Program.cs

using System.Text;
using Mazaad.API.Hubs;
using Mazaad.Application.Interfaces.Repositories;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Mazaad.Infrastructure.Persistence.Repositories;
using Mazaad.Infrastructure.Services;
using Mazaad.Infrastructure.Services.Auth;
using Mazaad.Infrastructure.Services.Inventory;
using Mazaad.Infrastructure.Services.SalesOperations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Mazaad.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ─── MVC & Swagger ────────────────────────────────────────────────
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Mazaad API",
                    Version = "v1"
                });

                // نضيف زرار Authorize في Swagger عشان نختبر JWT
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer {token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id   = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // ─── Database ─────────────────────────────────────────────────────
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // ─── ASP.NET Identity ─────────────────────────────────────────────
            builder.Services
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    // Password rules
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = false;

                    // Lockout — بعد 5 محاولات خاطئة
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    options.Lockout.AllowedForNewUsers = true;

                    // Email يكون unique
                    options.User.RequireUniqueEmail = true;

                    // 2FA
                    options.SignIn.RequireConfirmedEmail = false; // false للـ development
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // ─── JWT Authentication ───────────────────────────────────────────
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JWT:Issuer"],
                        ValidAudience = builder.Configuration["JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)),
                        // بيمنع الـ clock skew (التسامح الافتراضي 5 دقايق)
                        ClockSkew = TimeSpan.Zero
                    };

                    // SignalR محتاج JWT في الـ query string
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            // ─── Authorization Policies ───────────────────────────────────────
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("SuperAdminOnly",
                    policy => policy.RequireRole("SuperAdmin"));

                options.AddPolicy("CompanyAdminOrAbove",
                    policy => policy.RequireRole("SuperAdmin", "CompanyAdmin"));

                options.AddPolicy("AnyCompanyUser",
                    policy => policy.RequireRole("SuperAdmin", "CompanyAdmin", "CompanyUser"));

                // الشركة لازم تكون Verified عشان تعمل bid
                options.AddPolicy("VerifiedCompanyOnly", policy =>
                    policy.RequireRole("CompanyAdmin", "CompanyUser")
                          .RequireClaim("companyId"));
            });

            // ─── CORS ─────────────────────────────────────────────────────────
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.WithOrigins(
                              builder.Configuration
                                     .GetSection("AllowedOrigins")
                                     .Get<string[]>() ?? new[] { "http://localhost:3000" })
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()); // مطلوب عشان الـ cookies
            });

            // ─── SignalR ──────────────────────────────────────────────────────
            builder.Services.AddSignalR();

            // ─── Auth Module Services ─────────────────────────────────────────
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<ISecurityLogService, SecurityLogService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
            builder.Services.AddScoped<ICompanyRegistrationService, CompanyRegistrationService>();
            builder.Services.AddScoped<ICompanyUserService, CompanyUserService>();
            builder.Services.AddScoped<ICompanyDocumentService, CompanyDocumentService>();
            builder.Services.AddScoped<ICompanyService, CompanyService>();
            // ─── Existing Services ────────────────────────────────────────────
            builder.Services.AddScoped<IListingService, ListingService>();
            builder.Services.AddScoped<IMaterialCategoryService, MaterialCategoryService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IBiddingService, BiddingService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IIndustryService, IndustryService>();
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
            builder.Services.AddScoped<IBiddingRepository, BiddingRepository>();
            builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
            builder.Services.AddScoped<ISalesRepository, SalesRepository>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<ISalesOperationsService, SalesOperationsService>();
            builder.Services.AddSingleton<IAuctionPresenceService, AuctionPresenceService>();

            // ─── Build ────────────────────────────────────────────────────────
            var app = builder.Build();

            // ─── Seed SuperAdmin ──────────────────────────────────────────────
            await SeedSuperAdminAsync(app);

            // ─── Pipeline ─────────────────────────────────────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(o =>
                {
                    o.SwaggerEndpoint("/swagger/v1/swagger.json", "Mazaad API v1");
                    o.RoutePrefix = "swagger";
                });
            }

            app.UseStaticFiles();
            app.UseCors();
            app.UseAuthentication(); // ← لازم قبل UseAuthorization
            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<ChatHub>("/hubs/chat");
            app.MapHub<BiddingHub>("/hubs/bidding");
            app.MapHub<AuctionHub>("/hubs/auction");

            app.Run();
        }

        // ── Seed SuperAdmin ───────────────────────────────────────────────────

        private static async Task SeedSuperAdminAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var email = config["SuperAdmin:Email"] ?? "superadmin@mazaad.com";
            var password = config["SuperAdmin:Password"] ?? "Admin@12345";
            var name = config["SuperAdmin:FullName"] ?? "Super Admin";

            // بس لو مش موجود
            if (await userManager.FindByEmailAsync(email) != null)
                return;

            var superAdmin = new ApplicationUser
            {
                FullName = name,
                Email = email,
                UserName = email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(superAdmin, password);
            await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
        }
    }
}