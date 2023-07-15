using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Helper;
using Web.Settings;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("Session:IdleTimeoutMinutes"));
                options.Cookie.HttpOnly = true;
            });
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient<IApiClient, ApiClient>();
            builder.Services.AddScoped<JwtTokenEvent>();

            builder.Services.Configure<ServiceUrlSetting>(builder.Configuration.GetSection(nameof(ServiceUrlSetting)));
            builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection(nameof(JwtSetting)));
            var jwtBearerOptionsSetup = new JwtTokenOptionsSetup();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.EventsType = typeof(JwtTokenEvent);
                jwtBearerOptionsSetup.Configure(options, builder.Services);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("~/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "chat",
                pattern: "Chat",
                defaults: new { controller = "Home", action = "Chat" }
                );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
                );

            app.Run();
        }
    }
}