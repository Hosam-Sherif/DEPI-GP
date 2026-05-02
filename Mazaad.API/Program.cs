using Microsoft.EntityFrameworkCore;
using Mazaad.Infrastructure.Persistence;
using Mazaad.Application.Interfaces;
using Mazaad.Application.Services;
using Mazaad.API.Hubs;

namespace Mazaad.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ─── MVC & Swagger ────────────────────────────────────────────────────
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Mazaad Institutional Marketplace API",
                    Version = "v1",
                    Description = "REST + SignalR API for real-time B2B industrial asset auctions."
                });
            });

            // ─── CORS (allow all origins for development) ─────────────────────────
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });

            // ─── SignalR (real-time chat + bidding) ───────────────────────────────
            builder.Services.AddSignalR();

            // ─── Database ─────────────────────────────────────────────────────────
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // ─── Application Services (DI) ────────────────────────────────────────

            // Core marketplace services
            builder.Services.AddScoped<IListingService, ListingService>();
            builder.Services.AddScoped<IMaterialCategoryService, MaterialCategoryService>();

            // Notification must be registered before BiddingService (dependency)
            builder.Services.AddScoped<INotificationService, NotificationService>();

            // Bidding service (depends on INotificationService)
            builder.Services.AddScoped<IBiddingService, BiddingService>();

            // Order service (depends on INotificationService)
            builder.Services.AddScoped<IOrderService, OrderService>();

            // Supporting services
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<ICompanyService, CompanyService>();
            builder.Services.AddScoped<IIndustryService, IndustryService>();

            var app = builder.Build();

            // ─── HTTP Pipeline ────────────────────────────────────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mazaad API v1");
                    options.RoutePrefix = string.Empty; // Swagger at root /
                });
            }

            app.UseCors();
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();

            // ─── SignalR Hubs ─────────────────────────────────────────────────────
            app.MapHub<ChatHub>("/hubs/chat");
            app.MapHub<BiddingHub>("/hubs/bidding");

            app.Run();
        }
    }
}
