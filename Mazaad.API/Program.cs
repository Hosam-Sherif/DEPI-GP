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

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // SignalR for real-time chat
            builder.Services.AddSignalR();

            // Database
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Register Application Services (DI)
            builder.Services.AddScoped<IBiddingService, BiddingService>();
            builder.Services.AddScoped<IListingService, ListingService>();
            builder.Services.AddScoped<IMaterialCategoryService, MaterialCategoryService>();
            builder.Services.AddScoped<IChatService, ChatService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<ChatHub>("/hubs/chat");

            app.Run();
        }
    }
}
