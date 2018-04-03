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

namespace Framework.Startup
{
    public class StartupConfigurationService<TDbContext> : IStartupConfigurationService  
        where TDbContext : DbContext
    {
        public virtual void ConfigureEnvironment(IHostingEnvironment env) { }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) { }

        public virtual void ConfigureService(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddDbContext<TDbContext>(options => options.UseInMemoryDatabase("Checklists"));
//            var connection = configuration.GetConnectionString("defaultConnection");
  //          services.AddDbContext<ChecklistsContext>(options => options.UseSqlServer(connection));
        }
    }
}
