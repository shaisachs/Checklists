using System;
using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using Checklists.Repositories;
using Checklists;
using Checklists.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace Checklists.Tests
{
    public class Test1
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private readonly ChecklistsContext _context;
        public Test1()
        {

            // Arrange
            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.ConfigureServices(s => s.AddSingleton<IStartupConfigurationService, TestStartupConfigurationService<ChecklistsContext>>());

            _server = new TestServer(webHostBuilder.UseStartup<Startup>());
            _context = _server.Host.Services.GetService(typeof(ChecklistsContext)) as ChecklistsContext;
            _client = _server.CreateClient();

            var item = new Checklist() { Name = "chores", Id = 12 };
            var repo = new ChecklistRepository(_context);
            repo.CreateItem(item, "alice");
        }

        [Fact]
        public async Task ReturnHelloWorld()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/checklists");
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsAsync<BaseModelCollection<Checklist>>();
            var items = data.Items;

            // Assert
            Assert.True(items != null);
            Assert.True(items.Any(c => c.Name.Equals("chores")));
            Assert.False(items.Any(c => c.Name.Equals("blah")));
        }
    }
}
