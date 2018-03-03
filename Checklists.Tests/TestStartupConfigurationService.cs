using System;
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
    public class TestStartupConfigurationService<TDbContext> : IStartupConfigurationService  
        where TDbContext : DbContext
    {
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            SetupDatabase(app);
        }

        public virtual void ConfigureEnvironment(IHostingEnvironment env)
        {
            env.EnvironmentName = "Test";
        }

        public virtual void ConfigureService(IServiceCollection services, IConfigurationRoot configuration)
        {
            ConfigureDatabase(services);
        }

        protected virtual void SetupDatabase(IApplicationBuilder app)
        {
        }

        protected virtual void ConfigureDatabase(IServiceCollection services)
        {
            services.AddDbContext<TDbContext>(options => options.UseInMemoryDatabase("Checklists"));
        }
    }
}
