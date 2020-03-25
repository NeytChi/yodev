using NMiddleware;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Common
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }        
        public IConfiguration Configuration { get; }
        public readonly string allowSpecificOrigins = "cors-politics";


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var serverConfig = Program.serverConfiguration();
            services.AddCors(options =>
            {
                var origins = serverConfig.GetSection("allow_origin")
                .GetChildren()
                .Select(x => x.Value)
                .ToArray();
                options.AddPolicy(allowSpecificOrigins,
                builder =>
                {
                    builder.WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });
            AuthOptions authOptions = new AuthOptions();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true,
                    };
                });

            services.AddDbContext<Context>(options 
                => options.UseMySql(Context.databaseConnection()));

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory(allowSpecificOrigins));
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            if (Program.requestView)
            {
                app.Use(next => context => { context.Request.EnableRewind(); return next(context); });
                app.UseMiddleware<RequestWatchingSteamMiddleware>();
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCors(allowSpecificOrigins);
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
