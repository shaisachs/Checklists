﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;
using AutoMapper;
using Checklists.Models;
using Checklists.Validators;
using Checklists.Translators;
using Checklists.Dtos;
using Checklists.Repositories;

namespace Checklists
{
    public class Startup
    {
        private IStartupConfigurationService _externalStartupConfiguration;

        public Startup(IConfiguration configuration, IHostingEnvironment env,
            IStartupConfigurationService externalStartupConfiguration)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            HostingEnvironment = env;
            _externalStartupConfiguration = externalStartupConfiguration;
            _externalStartupConfiguration.ConfigureEnvironment(env);
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }    

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication().AddRapidApiAuthentication();
            services.AddMvc();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddAutoMapper(x=> x.AddProfile(new MappingsProfile()));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Checklists API", Version = "v1" });
            });

            services.AddScoped<ValidationErrorTranslator, ValidationErrorTranslator>();
            // TODO: automate this piece
            services.AddScoped<BaseValidator<Checklist>, ChecklistValidator>();
            services.AddScoped<BaseTranslator<Checklist, ChecklistDto>, ChecklistTranslator>();
            services.AddScoped<ChecklistRepository, ChecklistRepository>();

            _externalStartupConfiguration.ConfigureService(services, null);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Checklists API V1");
            });

            _externalStartupConfiguration.Configure(app, env, loggerFactory);

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetService<ChecklistsContext>().Database.EnsureCreated();
            }

            app.UseMvc();
        }

    }
}
