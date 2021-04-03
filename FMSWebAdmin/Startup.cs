
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelsFeedbackSystem.Models;
using Microsoft.EntityFrameworkCore;
using ModelsFeedbackSystem.Repository;
using Quartz.Impl;
using Quartz;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace FMSWebAdmin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string securityKey = Configuration.GetValue<string>("Jwt:SecretKey");
            //symmetric security key 
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            string issuer = Configuration.GetValue<string>("ValidIssuer");
            string audience = Configuration.GetValue<string>("ValidAudience");
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    // what to validate
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    // set up validate data 
                    ValidIssuer = issuer, //The audience "aud" claim in a JWT is meant to refer to the Resource Servers that should accept the token.
                    ValidAudience = audience, //typically, the base address of the resource being accessed, such as https://contoso.com.
                    IssuerSigningKey = symmetricSecurityKey
                };
            });
            services.AddControllersWithViews();

            services.AddTransient<DbContext, FeedbackSystemDBContext>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            services.AddScoped<IEquipmentGroup, EquipmentGroupRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            services.AddScoped<IThanksRepository, ThanksRepository>();
            services.AddScoped<IGiftRepository, GiftRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IResponseRepository, ResponseRepository>();

             services.AddMvc().AddSessionStateTempDataProvider();
            services.AddDistributedMemoryCache();
            /*services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "localhost:6379";
                options.InstanceName = "redis";
            });*/
            services.AddSession();
            //Add distributed cache service backed by Redis cache
            /*services.AddDistributedRedisCache(o =>
            {
                o.Configuration = Configuration.GetConnectionString("Redis");
            });*/

            services.AddSingleton(provider => GetScheduler().Result);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            //Enable sessions
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });

        }

        private async Task<IScheduler> GetScheduler()
        {
            var properties = new NameValueCollection
            {
                { "quartz.scheduler.instanceName", "FMSWebAdmin" },
                { "quartz.scheduler.instanceId", "FMSWebAdmin" },
                { "quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz" },
                { "quartz.jobStore.useProperties", "true" },
                { "quartz.jobStore.dataSource", "default" },
                { "quartz.jobStore.tablePrefix", "QRTZ_" },
                {
                    "quartz.dataSource.default.connectionString",
                    "Server=tcp:feedbacksystem.database.windows.net,1433;Initial Catalog=FeedbackSystemDB;Persist Security Info=False;User ID=hiepfms;Password=Hiep123456789;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
                },
                { "quartz.dataSource.default.provider", "SqlServer" },
                { "quartz.threadPool.threadCount", "1" },
                { "quartz.serializer.type", "json" },
            };
            var schedulerFactory = new StdSchedulerFactory(properties);
            var scheduler = await schedulerFactory.GetScheduler();
            return scheduler;
        }
    }
}
