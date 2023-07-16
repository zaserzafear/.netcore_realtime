using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            var corsSettings = builder.Configuration.GetSection(nameof(CorsSettings)).Get<CorsSettings>();
            var allowedOrigins = corsSettings?.AllowedOrigins ?? Array.Empty<string>();
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var jwtSetting = builder.Configuration.GetSection(nameof(JwtSetting)).Get<JwtSetting>();
            builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection(nameof(JwtSetting)));

            builder.Services.AddSignalR();
            builder.Services.AddSingleton<ChatConnectionManager>();

            builder.Services.AddSingleton<JwtToken>();
            builder.Services.AddAuthentication(options =>
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
                    ValidIssuer = jwtSetting!.Issuer,
                    ValidAudience = jwtSetting!.Audience,
                    IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(token);

                        var keyBytes = Encoding.UTF8.GetBytes(jwtSetting!.SigningKey);
                        var key = new byte[16]; // 128 bits key size

                        if (keyBytes.Length >= 16)
                        {
                            Array.Copy(keyBytes, key, 16);
                        }
                        else
                        {
                            throw new InvalidOperationException("The key size is too small for the HS256 encryption algorithm.");
                        }

                        var symmetricSecurityKey = new SymmetricSecurityKey(key);

                        return new[] { symmetricSecurityKey };
                    }
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<ChatHub>("~/ChatHub");

            app.Run();
        }
    }
}