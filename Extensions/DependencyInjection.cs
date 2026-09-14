using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Yarp.ReverseProxy.Transforms;

namespace APIGateway.Msv.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Validasi & Ambil JWT Key
            var jwtKey = configuration["Jwt_Key"] ?? configuration["Jwt:Key"]
           ?? throw new InvalidOperationException("JWT Key tidak ditemukan!");


            // 2. Registrasi Autentikasi JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true
                };
            });

            // 3. Registrasi Otorisasi
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RegisteredUser", policy => policy.RequireAuthenticatedUser());
            });

            // 4. Registrasi YARP + Header Transformation
            services.AddReverseProxy()
                .LoadFromConfig(configuration.GetSection("ReverseProxy"))
                .AddTransforms(transformContext =>
                {
                    transformContext.AddRequestTransform(async requestContext =>
                    {
                        var user = requestContext.HttpContext.User;

                        if (user.Identity?.IsAuthenticated == true)
                        {
                            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                            var role = user.FindFirst(ClaimTypes.Role)?.Value;

                            // Menggunakan .Set() lebih aman daripada .Add() untuk menghindari duplikasi header
                            if (!string.IsNullOrEmpty(userId))
                            {
                                requestContext.ProxyRequest.Headers.Remove("X-User-Id");
                                requestContext.ProxyRequest.Headers.Add("X-User-Id", userId);
                            }

                            if (!string.IsNullOrEmpty(role))
                            {
                                requestContext.ProxyRequest.Headers.Remove("X-User-Role");
                                requestContext.ProxyRequest.Headers.Add("X-User-Role", role);
                            }
                        }
                        await Task.CompletedTask;
                    });
                });

            return services;
        }
    }
}
