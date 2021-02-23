using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySafe.Api.Filters;
using MySafe.Api.Profiles;
using MySafe.Api.Services;
using MySafe.Core;
using MySafe.Data.EF;
using MySafe.Data.Entities;
using MySafe.Data.Identity;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MySafe.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddIdentity<ApplicationUser, ApplicationRole>()
                //.AddUserStore<ApplicationUserStore>()
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();

            //var certificate = new X509Certificate2("*.pfx", "password");
            //var key = new X509SecurityKey(cert);
            // получаем сертификат
            // 1. ¬идео https://www.youtube.com/watch?v=reFh982o7lU&list=PLIB8be7sunXPUwvh_kvow81lqXvU8mTu-&index=19
            // 2. —татьи https://www.calabonga.net/blog/post/self-signed-certificate-on-csharp

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Events = new JwtBearerEvents();
                    // TODO: move code challenge and tokenvalidated to services
                    options.Events.OnTokenValidated = async (context) =>
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();

                        // TODO access_token to constants
                        var jwtAccessToken = tokenHandler.WriteToken(context.SecurityToken);

                        var userManager = context.HttpContext.RequestServices
                            .GetService(typeof(UserManager<ApplicationUser>)) as UserManager<ApplicationUser>;

                        if (userManager == null)
                        {
                            context.Fail(new UnauthorizedAccessException());
                        }

                        var user = await userManager.Users
                            .Include(e => e.AccessTokens)
                            .FirstOrDefaultAsync(x => x.AccessTokens
                                .Any(t => t.JwtToken == jwtAccessToken));

                        var accessToken = user?.AccessTokens.FirstOrDefault(x => x.JwtToken == jwtAccessToken);

                        if (accessToken?.IsActive == true) return;
                        
                        context.Fail(new UnauthorizedAccessException());
                    };
                    options.Events.OnChallenge = context =>
                    {
                        context.HandleResponse();

                        var payload = new JObject
                        {
                            ["error"] = context.Error
                        };

                        if (!string.IsNullOrEmpty(context?.ErrorDescription)) payload.Add("error_description", context.ErrorDescription);
                        //payload.Add("error_uri", context.ErrorUri);

                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = 401;

                        return context.Response.WriteAsync(payload.ToString());
                    };
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // укзывает, будет ли валидироватьс€ издатель при валидации токена
                        ValidateIssuer = true,
                        // строка, представл€юща€ издател€
                        ValidIssuer = AppData.AuthOptions.ISSUER,
 
                        // будет ли валидироватьс€ потребитель токена
                        ValidateAudience = true,
                        // установка потребител€ токена
                        ValidAudience = AppData.AuthOptions.AUDIENCE,
                        // будет ли валидироватьс€ врем€ существовани€
                        ValidateLifetime = true,
 
                        // установка ключа безопасности
                        // IssuerSigningKey = new SymmetricSecurityKey(key);
                        IssuerSigningKey = AppData.AuthOptions.GetSymmetricSecurityKey(),
                        // валидаци€ ключа безопасности
                        ValidateIssuerSigningKey = true
                    };
                });



            services.AddDbContextPool<ApplicationContext>(config =>
            {
                config.UseInMemoryDatabase("FOR_TESTS");
            });
            
            services.AddMemoryCache();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // UserRequest settings.
                options.User.AllowedUserNameCharacters = null;
                options.User.RequireUniqueEmail = true;


            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "MySafe.Api",
                    Version = "v1",
                    Description = "no description"
                });
                options.ResolveConflictingActions(x => x.First());
                
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            });

            // services
            services.AddScoped<IIdentityService, IdentityService>();

            services.AddScoped<ApplicationUserManager>();
            services.AddAutoMapper(typeof(Startup));
            services.AddAuthorization();
            services.AddControllers()
                //.AddMvcOptions(options => options.Filters.Add(typeof(AuthorizeFilter)))
                .AddJsonOptions(options => options.JsonSerializerOptions.IgnoreNullValues = true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseHsts();

            app.UseSwagger();

            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MySafe.Api"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
