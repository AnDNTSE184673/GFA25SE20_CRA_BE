
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;

namespace CRA_Self_drive_Rental.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "YuuZone",
                    Version = "v1",
                    Description = "YuuZone Backend API",
                    Contact = new OpenApiContact
                    {
                        Name = "Vinh",
                        Email = "vinhtrannguyenquang912@gmail.com"
                    }
                });

                //Enable XML comments
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                //Enable annotation support
                c.EnableAnnotations();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token from a successful /login call"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            builder.Services.AddAuthentication(options => {
                // A Google focused approach
                //options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;   // used when call Challenge()

                // A JWT/Google hybrid approach
                // API protection
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                // Let Google write the external‑login cookie here
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),
                        NameClaimType = JwtRegisteredClaimNames.Name  // maps "name" → User.Identity.Name
                    };
                })
                .AddGoogle(google =>
                {
                    google.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                    google.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

                    google.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // explicit
                    google.SaveTokens = true;  // if you need access / refresh tokens later

                    google.Events.OnRemoteFailure = ctx =>
                    {
                        var log = ctx.HttpContext.RequestServices
                                   .GetRequiredService<ILogger<Program>>();
                        log.LogError("Google auth failed: {Error}", ctx.Failure?.Message);
                        return Task.CompletedTask;
                    };

                    //google.CallbackPath = "/Authen/signin-google";
                })
                .AddCookie();

            builder.Services.AddAuthorization();

            // Add CORS policy to allow specific origins (e.g., localhost for development)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services
                .AddServices(builder.Configuration)
                .AddRepositories(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            //navigate to this path to check environment
            app.MapGet("/env", (IWebHostEnvironment env) => env.EnvironmentName);

            Log.Information("Starting logger...");

            app.Run();
        }
    }
}
