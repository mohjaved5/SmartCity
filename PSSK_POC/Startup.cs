using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PSSK_POC.Common;
using PSSK_POC.Contracts;
using PSSK_POC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PSSK_POC
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
            IronBarCode.License.LicenseKey = "IRONBARCODE.MONISJAVED.25033-BDA79C65C1-C5VMY2-DHXVUECGB674-VUTQ2QSDKTON-T3DBTMGLQMJI-NN4XQBTZPY3S-WHHYKAY7DKRR-XYSAJJ-TUP6XUWCDIOHEA-DEPLOYMENT.TRIAL-KPVODJ.TRIAL.EXPIRES.26.AUG.2022";
            services.AddScoped<AuthenticationService>();
            services.AddScoped<UserService>();
            services.AddScoped<DocumentService>();
            services.AddScoped<HttpClient>();
            services.AddTransient<IQRCodeService, QRCodeService>(); 
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddControllers();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "PSSK Demo API",
                    Version = "v2",
                    Description = "",
                });
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped(provider => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MapperBase());
            }).CreateMapper());

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v2/swagger.json", "PSSK Services"));
        }
    }
}
