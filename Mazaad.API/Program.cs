// Mazaad.API/Program.cs
using Mazaad.Infrastructure.Services.Payment;
using Mazaad.API.Hubs;
using Mazaad.Application.Common;
using
Mazaad.Application.Interfaces;
using
Mazaad.Application.Interfaces.Repositories;
using
Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using
Mazaad.Domain.Models;
using Mazaad.Infrastructure.Hubs;
using
Mazaad.Infrastructure.Persistence;
using
Mazaad.Infrastructure.Persistence.Repositories;
using
Mazaad.Infrastructure.Services;
using
Mazaad.Infrastructure.Services.Auth;
using
Mazaad.Infrastructure.Services.Inventory;
using
Mazaad.Infrastructure.Services.SalesOperations;
using
Microsoft.AspNetCore.Authentication.JwtBearer;
using
Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Mazaad.API
{
    public class Program
    {
        public static async Task
Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ─── MVC & Swagger ────────────────────────────────────────────────
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(
                        new System.Text.Json.Serialization.JsonStringEnumConverter());
                });
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
            builder.Services.AddScoped<IImageService, CloudinaryImageService>();
            builder.Services.AddScoped<IProfileService, ProfileService>();
            // Email settings binding
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            // Services
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IContactService, ContactService>();
            // بعد باقي الـ services
            builder.Services.AddHostedService<AuctionStatusUpdaterService>();

            builder.Services.AddScoped<IEmployeeService, EmployeeService>(); ;
            //-----payment
            builder.Services.AddScoped<IOrderService, OrderService>();

            builder.Services.Configure<PaymobOptions>(builder.Configuration.GetSection("Paymob"));
            builder.Services.AddHttpClient<PaymobClient>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            // في Program.cs أو ServiceCollectionExtensions — زود السطر ده مع باقي الـ services
            builder.Services.AddScoped<ICommissionPolicyService, CommissionPolicyService>();

            // ─── Build ────────────────────────────────────────────────────────
            var app = builder.Build();

            // ─── Seed Roles (لازم تتنفذ قبل أي Seed تاني بيستخدم AddToRoleAsync) ──
            await SeedRolesAsync(app);

            // ─── Seed SuperAdmin ──────────────────────────────────────────────
            await SeedSuperAdminAsync(app);

            // ─── Seed Demo Data (dev only) ────────────────────────────────────
            await SeedDemoDataAsync(app);

            // ─── Pipeline ─────────────────────────────────────────────────────
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "Mazaad API v1");
                o.RoutePrefix = "swagger";
            });
            //}

            app.UseStaticFiles();
            app.UseCors();
            app.UseAuthentication(); // ← لازم قبل UseAuthorization
            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<ChatHub>("/hubs/chat");
            app.MapHub<AuctionHub>("/hubs/auction");

            app.Run();
        }

        // ── Seed Roles ────────────────────────────────────────────────────────
        /// <summary>
        /// بيتأكد إن كل الـ Roles الأساسية موجودة في AspNetRoles.
        /// Idempotent: بيتشيك RoleExistsAsync قبل ما يعمل Create.
        /// SuperAdmin / CompanyAdmin → موظفين الشركة من الداخل (التسلسل الإداري).
        /// CompanyUser → موظف تابع لشركة (يضيفه CompanyAdmin من CompanyUsersController).
        /// Bidder → مزايد عادي مسجل بنفسه من صفحة Register العامة (مالوش شركة).
        /// </summary>
        private static async Task SeedRolesAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<ApplicationRole>>();

            string[] roles = { "SuperAdmin", "CompanyAdmin", "CompanyUser", "Bidder" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                }
            }
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

        // ── Seed Demo Data ────────────────────────────────────────────────────
        /// <summary>
        /// بيضيف orders اختبار لـ company 4 الموجودة في الـ DB عشان الـ Sales Statistics APIs مش تبقى فاضية.
        /// Idempotent: بيشتغل بس لو company 4 معهاش orders.
        /// </summary>
        private static async Task SeedDemoDataAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // ── اجيب الـ seller company (id=4 اللي بيستخدمه اليوزر) ──────────
            var sellerCompany = await db.Companies.FindAsync(4);
            if (sellerCompany == null)
            {
                // لو 4 مش موجود، جيب أي company موجودة
                sellerCompany = await db.Companies.FirstOrDefaultAsync();
                if (sellerCompany == null) return; // مفيش companies خالص
            }

            // لو الـ company دي عندها orders بالفعل → skip
            if (await db.Orders.AnyAsync(o => o.SellerCompanyId == sellerCompany.Id))
                return;

            // ── جيب أو اعمل buyer companies ──────────────────────────────────
            // نجيب أي 2 companies تانية كـ buyers
            var otherCompanies = await db.Companies
                .Where(c => c.Id != sellerCompany.Id)
                .Take(2)
                .ToListAsync();

            // لو مفيش companies تانية كفاية، نعملهم
            var industryId = sellerCompany.IndustryId;
            while (otherCompanies.Count < 2)
            {
                var newBuyer = new Companies
                {
                    IndustryId = industryId,
                    CompanyName = otherCompanies.Count == 0 ? "Beta Construction Ltd." : "Gamma Trading LLC",
                    CommercialRegNum = otherCompanies.Count == 0 ? "CR-B01" : "CR-G01",
                    TaxRegistrationNum = otherCompanies.Count == 0 ? "TR-B01" : "TR-G01",
                    City = otherCompanies.Count == 0 ? "Alexandria" : "Giza",
                    AddressDetails = "Test Address",
                    VerificationStatus = CompanyVerificationStatus.Verified,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                db.Companies.Add(newBuyer);
                await db.SaveChangesAsync();
                otherCompanies.Add(newBuyer);
            }

            var buyerCompany1 = otherCompanies[0];
            var buyerCompany2 = otherCompanies[1];

            // ── جيب أو اعمل Material Category ───────────────────────────────
            var category = await db.MaterialCategories.FirstOrDefaultAsync()
                           ?? new Material_Categories
                           {
                               CategoryName = "Steel",
                               Description = "Various steel products",
                               UnitOfMeasure = "Ton",
                               CreatedAt = DateTime.UtcNow
                           };
            if (category.Id == 0)
            {
                db.MaterialCategories.Add(category);
                await db.SaveChangesAsync();
            }

            // ── جيب أو اعمل Commission Policy ───────────────────────────────
            var policy = await db.CommissionPolicies.FirstOrDefaultAsync(p => p.Active)
                         ?? new Commission_Policies
                         {
                             PolicyName = "Standard 2%",
                             CommissionRate = 0.02m,
                             MinAmount = 0m,
                             MaxAmount = 9999999m,
                             EffectiveFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                             EffectiveTo = new DateTime(2030, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                             Active = true
                         };
            if (policy.Id == 0)
            {
                db.CommissionPolicies.Add(policy);
                await db.SaveChangesAsync();
            }

            // ── جيب أو اعمل Listing ──────────────────────────────────────────
            var listing = await db.Listings.FirstOrDefaultAsync(l => l.CompanyId == sellerCompany.Id)
                          ?? new Listings
                          {
                              CompanyId = sellerCompany.Id,
                              CategoryId = category.Id,
                              Title = "Demo Steel Batch",
                              Description = "Demo listing for statistics",
                              TechnicalSpecs = "Grade A",
                              MinOrderQuantity = 5m,
                              AvailableQuantity = 500m,
                              PurityPercentage = 99m,
                              BaseCurrency = "USD",
                              CurrentHighestBid = 850m,
                              BidCount = 3,
                              Status = ListingStatus.Ended,
                              Condition = ListingCondition.New,
                              Location = "Warehouse",
                              StartDate = DateTime.UtcNow.AddMonths(-6),
                              EndDate = DateTime.UtcNow.AddMonths(-5),
                              CreatedAt = DateTime.UtcNow.AddMonths(-6),
                              UpdatedAt = DateTime.UtcNow
                          };
            if (listing.Id == 0)
            {
                db.Listings.Add(listing);
                await db.SaveChangesAsync();
            }

            // ── جيب الـ SuperAdmin user لوضعه في الـ bids ───────────────────
            var superAdminUser = await userManager.FindByEmailAsync("superadmin@mazaad.com");
            // جيب أي user موجود في الـ DB لو مفيش superadmin
            var anyUserId = superAdminUser?.Id
                            ?? (await db.Users.Select(u => u.Id).FirstOrDefaultAsync());
            if (anyUserId == 0) return; // مفيش users خالص

            // ── اعمل Bids ────────────────────────────────────────────────────
            var bid = new Bids
            {
                ListingId = listing.Id,
                PlacedByUserId = anyUserId,
                BuyerCompanyId = buyerCompany1.Id,
                BidAmountPerUnit = 850m,
                TotalBidAmount = 850m * 200m,
                Quantity = 200m,
                IsAnonymous = false,
                WinningBid = true,
                Status = BidStatus.Won,
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            };
            db.Bids.Add(bid);
            await db.SaveChangesAsync();

            // ── اعمل Orders موزعة على شهور ─────────────────────────────────
            var now = DateTime.UtcNow;
            var orders = new[]
            {
                new Orders { SellerCompanyId = sellerCompany.Id, BuyerCompanyId = buyerCompany1.Id, BidId = bid.Id, AppliedPolicyId = policy.Id, AgreedQuantity = 100m, AgreedUnitPrice = 850m, PlatformFee = 1700m,  TotalAmount = 86700m,   Status = OrderStatus.Completed, OrderDate = now.AddMonths(-5),             UpdatedAt = now },
                new Orders { SellerCompanyId = sellerCompany.Id, BuyerCompanyId = buyerCompany2.Id, BidId = bid.Id, AppliedPolicyId = policy.Id, AgreedQuantity = 150m, AgreedUnitPrice = 820m, PlatformFee = 2460m,  TotalAmount = 125460m,  Status = OrderStatus.Completed, OrderDate = now.AddMonths(-4),             UpdatedAt = now },
                new Orders { SellerCompanyId = sellerCompany.Id, BuyerCompanyId = buyerCompany1.Id, BidId = bid.Id, AppliedPolicyId = policy.Id, AgreedQuantity = 200m, AgreedUnitPrice = 870m, PlatformFee = 3480m,  TotalAmount = 177480m,  Status = OrderStatus.Completed, OrderDate = now.AddMonths(-3),             UpdatedAt = now },
                new Orders { SellerCompanyId = sellerCompany.Id, BuyerCompanyId = buyerCompany2.Id, BidId = bid.Id, AppliedPolicyId = policy.Id, AgreedQuantity = 80m,  AgreedUnitPrice = 860m, PlatformFee = 1376m,  TotalAmount = 70176m,   Status = OrderStatus.Completed, OrderDate = now.AddMonths(-3).AddDays(5), UpdatedAt = now },
                new Orders { SellerCompanyId = sellerCompany.Id, BuyerCompanyId = buyerCompany1.Id, BidId = bid.Id, AppliedPolicyId = policy.Id, AgreedQuantity = 120m, AgreedUnitPrice = 900m, PlatformFee = 2160m,  TotalAmount = 110160m,  Status = OrderStatus.Completed, OrderDate = now.AddMonths(-1),             UpdatedAt = now },
                new Orders { SellerCompanyId = sellerCompany.Id, BuyerCompanyId = buyerCompany2.Id, BidId = bid.Id, AppliedPolicyId = policy.Id, AgreedQuantity = 90m,  AgreedUnitPrice = 920m, PlatformFee = 1656m,  TotalAmount = 84456m,   Status = OrderStatus.Pending,   OrderDate = now.AddDays(-3),              UpdatedAt = now },
            };
            db.Orders.AddRange(orders);
            await db.SaveChangesAsync();

            // ── اعمل Inventory Items ─────────────────────────────────────────
            if (!await db.InventoryItems.AnyAsync(i => i.company_id == sellerCompany.Id))
            {
                db.InventoryItems.AddRange(
                    new InventoryItem { company_id = sellerCompany.Id, category_id = category.Id, name = "Steel Bar 12mm", description = "High-grade construction steel bars", quantity = 500m, unit_of_measure = "Ton", minimum_auction_price = 750m, current_market_price = 900m, status = InventoryItemStatus.Available, created_at = now.AddMonths(-6), updated_at = now },
                    new InventoryItem { company_id = sellerCompany.Id, category_id = category.Id, name = "Steel Plate 5mm", description = "Flat steel plates for fabrication", quantity = 200m, unit_of_measure = "Ton", minimum_auction_price = 800m, current_market_price = 950m, status = InventoryItemStatus.InAuction, created_at = now.AddMonths(-3), updated_at = now },
                    new InventoryItem { company_id = sellerCompany.Id, category_id = category.Id, name = "Steel Coil", description = "Cold-rolled steel coil", quantity = 300m, unit_of_measure = "Ton", minimum_auction_price = 700m, current_market_price = 850m, status = InventoryItemStatus.Sold, created_at = now.AddMonths(-5), updated_at = now }
                );
                await db.SaveChangesAsync();
            }
        }
    }

}