using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CDN.Data;
using CDN.Data.Tables;
using CDN.Services;

namespace CDN
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("CDNContextConnection");
            builder.Services.AddDbContext<CDNContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddIdentity<CDNUser, IdentityRole>(options => {
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/ ";
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<CDNContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddTransient<IFtpFileService, FtpFileService>();

            // Temporary service scope to access DbContext before building the app
            using (var serviceProvider = builder.Services.BuildServiceProvider())
            {
                // Create a scope to access services
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CDNContext>();

                    // Fetch the allowed origins from the database
                    var allowedOrigins = dbContext.AllowedOrigins.Select(o => o.Origin).ToList();

                    // Configure CORS using the fetched allowed origins
                    builder.Services.AddCors(options =>
                    {
                        options.AddPolicy("CDNFilesCorsPolicy", policy =>
                        {
                            policy.WithOrigins(allowedOrigins.ToArray()) // Use the fetched origins
                                  .AllowAnyHeader()
                                  .AllowAnyMethod();
                        });
                    });
                }
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseCors("CDNFilesCorsPolicy");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");


            app.MapRazorPages();

            app.Run();
        }
    }
}
