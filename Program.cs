using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PaymentBE.DataAccess;
using PaymentBE.Middleware;
using PaymentBE.Services;

namespace PaymentBE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped(typeof(EmailService));
            builder.Services.AddScoped(typeof(OTPService));

            // Add services to the container.
            builder.Services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
            });
            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthentication()
                .AddCookie("CookieScheme", options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.ExpireTimeSpan = TimeSpan.FromDays(2);
                    options.Cookie.Name = "MyAppCookie";
                    options.LogoutPath = "/Auth/Logout";

                });
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseOtp();
            


            app.MapControllerRoute(
                "default", "{controller}/{action}/{id?}"
                );

            app.Run();
        }
    }
}
