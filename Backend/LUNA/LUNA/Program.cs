
using System;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using BLL.Caching;
using BLL.Exceptions;
using BLL.Interfaces;
using BLL.Services;
using BLL.Services.Auth;
using BLL.Services.Repository;
using BLL.Settings;
using BLL.Settings.Email;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PL.Middlewares;
using StackExchange.Redis;

namespace LUNA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            ////Swagger
            //builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();

            //Registering the services
            builder.Services.AddDbContext<DBContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {
               
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
            })
            .AddEntityFrameworkStores<DBContext>()
            .AddDefaultTokenProviders();

            //builder.Services.AddIdentity<User, IdentityRole<int>>()
            //       .AddEntityFrameworkStores<DBContext>()
            //       .AddDefaultTokenProviders();

            IWebHostEnvironment env = builder.Environment;
            //JWT
            builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWT"));
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
                };
            });

            //emailSettings
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            //builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddScoped<IEmailService, SMTPEmailService>();
            builder.Services.AddScoped<ITokenService, JWTTokenService>();
            builder.Services.AddScoped<IOTPService, OTPService>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IServiceManager, ServiceManager>();
            builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            builder.Services.Configure<ApiBehaviorOptions>(config =>
            {
                config.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState.Where(m => m.Value.Errors.Any())
                                                            .Select(m => new ValidationError()
                                                            {
                                                                Field = m.Key,
                                                                Errors = m.Value.Errors.Select(errors => errors.ErrorMessage)
                                                            });

                    var response = new ValidationErrorResponse()
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(response);
                };
            });

            //CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", builder =>
                {
                    builder.WithOrigins("http://127.0.0.1:5500")
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });


            //Rate Limiting
            builder.Services.AddRateLimiter(options =>
            {
                // Return standard HTTP 429 instead of default 503
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
              

                // Define a Sliding Window policy partitioned by user IP address
                options.AddPolicy("sliding-by-ip", context =>
                {
                    string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetSlidingWindowLimiter(clientIp, _ =>
                        new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 5,                              // Max 5 requests
                            Window = TimeSpan.FromMinutes(1),             // Per 1 minute
                            SegmentsPerWindow = 6,
                            QueueLimit = 0                                // Reject immediately if limit is hit
                        });
                });

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    var errorResponse = new
                    {
                        success = false,
                        statusCode = StatusCodes.Status429TooManyRequests,
                        ErrorMessage = "Too many requests. Please try again later."
                    };

                    var json = JsonSerializer.Serialize(errorResponse);
                    await context.HttpContext.Response.WriteAsync(json, cancellationToken);
                };
            });

            //caching
            var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
                options.InstanceName = "Luna:";
            });

            builder.Services.AddScoped<ICacheRepository, CacheRepository>();
            builder.Services.AddScoped<ICacheService, RedisCacheService>();
            builder.Services.AddScoped<IIntentService, IntentService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            //app.UseSwagger();
            //app.UseSwaggerUI();

            app.UseMiddleware<GlobalErrorHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseCors("AllowFrontend");
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
