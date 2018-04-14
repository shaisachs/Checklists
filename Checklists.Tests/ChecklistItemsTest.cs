using System;
using Xunit;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using Checklists.Repositories;
using Checklists;
using Checklists.Models;
using Checklists.Dtos;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Checklists.Validators;
using Framework.Auth;
using Framework.Models;
using Framework.Dtos;
using Framework.Validators;
using Framework.Repositories;
using Newtonsoft.Json;
using Framework.Tests;
using Framework.Startup;

namespace Checklists.Tests
{
    public class ChecklistItemsTest
    {
        protected const string DefaultUsername = "alice";
        protected HttpClient client;

        protected ChecklistTemplateRepository TemplateRepo;
        protected ChecklistTemplateItemRepository TemplateItemRepo;
        protected ChecklistRepository ChecklistRepo;
        protected ChecklistItemRepository ChecklistItemRepo;

        public ChecklistItemsTest()
        {
            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.ConfigureServices(s => s.AddSingleton<IStartupConfigurationService, TestStartupConfigurationService<ChecklistsContext>>());

            var server = new TestServer(webHostBuilder.UseStartup<Startup>());
            var context = server.Host.Services.GetService(typeof(ChecklistsContext)) as ChecklistsContext;

            client = server.CreateClient();
            client.DefaultRequestHeaders.Add(RapidApiAuthenticationHandler.RapidApiSecretHeaderName, "TestingAuthSecret");
            client.DefaultRequestHeaders.Add(RapidApiAuthenticationHandler.RapidApiUsernameHeaderName, DefaultUsername);
            SetupRepositories(context);
            SetupData();
        }

        protected void SetupRepositories(ChecklistsContext context)
        {
            TemplateRepo = new ChecklistTemplateRepository(context);
            TemplateItemRepo = new ChecklistTemplateItemRepository(context);
            ChecklistRepo = new ChecklistRepository(context);
            ChecklistItemRepo = new ChecklistItemRepository(context);
        }

        protected void SetupData()
        {
            var template = new ChecklistTemplate() { Id = 900, Name = "Test" };
            TemplateRepo.CreateItem(template, DefaultUsername);

            var otherUserTemplate = new ChecklistTemplate() { Id = 901, Name = "Test" };
            TemplateRepo.CreateItem(otherUserTemplate, "bob");

            var templateItems = new[]
            {
                new ChecklistTemplateItem() { Id = 911, ParentId = template.Id, Name = "Item 1" },
                new ChecklistTemplateItem() { Id = 912, ParentId = template.Id, Name = "Item 2" },
                new ChecklistTemplateItem() { Id = 913, ParentId = template.Id, Name = "Item 3" },
            };

            foreach (var templateItem in templateItems)
            {
                TemplateItemRepo.CreateItem(templateItem, DefaultUsername);
            }

            TemplateItemRepo.CreateItem(new ChecklistTemplateItem { Id = 914, ParentId = 901, Name = "Bob Item" }, "bob");

            ChecklistIdThatIsValid = 921;
            var checklists = new[]
            {
                new Checklist() { Id = ChecklistIdThatIsValid, ChecklistTemplateId = template.Id },
                new Checklist() { Id = ChecklistIdThatWasDeleted, ChecklistTemplateId = template.Id },
                new Checklist() { Id = 923, ChecklistTemplateId = template.Id },
            };

            foreach (var checklist in checklists)
            {
                ChecklistRepo.CreateItem(checklist, DefaultUsername);
            }

            ChecklistRepo.DeleteItem(checklists[1]);

            ChecklistRepo.CreateItem(new Checklist() { Id = ChecklistIdOwnedByAnotherUser, ChecklistTemplateId = 901 }, "bob");

            ChecklistItemThatIsValid = ChecklistItemRepo.GetAllItems(DefaultUsername, i => i.ParentId == ChecklistIdThatIsValid).First();
            ChecklistItemForSuccessfulPut = ChecklistItemRepo.GetAllItems(DefaultUsername, i => i.ParentId == ChecklistIdThatIsValid).ElementAt(1);
            ChecklistItemIdThatIsValid = ChecklistItemThatIsValid.Id;
            ChecklistItemIdOwnedByAnotherChecklist = ChecklistItemRepo.GetAllItems(DefaultUsername, i => i.ParentId == 922).First().Id;
            ChecklistItemIdOwnedByAnotherUser = ChecklistItemRepo.GetAllItems("bob", i => i.ParentId == 924).First().Id;
        }

        protected long ChecklistIdThatIsValid;

        protected ChecklistItem ChecklistItemThatIsValid;
        protected long ChecklistItemIdThatIsValid;

        protected ChecklistItem ChecklistItemForSuccessfulPut;

        protected const long ChecklistIdOutOfRange = -1;
        protected const long ChecklistIdThatWasDeleted = 922;
        protected const long ChecklistIdOwnedByAnotherUser = 924;
        protected const long ChecklistIdThatDoesNotExist = 2500;

        protected const long ChecklistItemIdOutOfRange = -1;
        protected long ChecklistItemIdOwnedByAnotherChecklist;
        protected long ChecklistItemIdOwnedByAnotherUser;
        protected const long ChecklistItemIdThatDoesNotExist = 2500;

        [Fact]
        public async void Get_items_for_valid_parent_succeeds()
        {
            var response = await client.GetAsync("/api/v1/checklists/" + ChecklistIdThatIsValid + "/items/");
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsAsync<BaseDtoCollection<ChecklistItem, ChecklistItemDto>>();
            var items = data.Items;

            Assert.True(items.Any(i => i.Id == ChecklistItemIdThatIsValid));

            var badIds = new[] { ChecklistItemIdOutOfRange, ChecklistItemIdOwnedByAnotherChecklist, ChecklistItemIdOwnedByAnotherUser, ChecklistItemIdThatDoesNotExist };
            foreach (var badId in badIds)
            {
                Assert.False(items.Any(i => i.Id == badId));
            }
        }

        [Fact]
        public async void Get_valid_item_for_valid_parent_succeeds()
        {
            var response = await client.GetAsync("/api/v1/checklists/" + ChecklistIdThatIsValid + "/items/" + ChecklistItemIdThatIsValid);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsAsync<ChecklistItemDto>();

            Assert.Equal(ChecklistItemIdThatIsValid, data.Id);
        }

        [Theory]
        [InlineData(ChecklistItemIdOutOfRange)]
// todo: workaround because these aren't static and can't be known before the object is instantiated
//        [InlineData(ChecklistItemIdOwnedByAnotherChecklist)]
//        [InlineData(ChecklistItemIdOwnedByAnotherUser)]
        [InlineData(ChecklistItemIdThatDoesNotExist)]
        public async void Get_invalid_item_for_valid_parent_succeeds(int invalidItemId)
        {
            var response = await client.GetAsync("/api/v1/checklists/" + ChecklistIdThatIsValid + "/items/" + invalidItemId);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(ChecklistIdOutOfRange)]
        [InlineData(ChecklistIdThatWasDeleted)]
        [InlineData(ChecklistIdOwnedByAnotherUser)]
        [InlineData(ChecklistIdThatDoesNotExist)]
        public async void Get_items_for_invalid_parent_fails(long invalidParentId)
        {
            var response = await client.GetAsync("/api/v1/checklists/" + invalidParentId + "/items/");
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(ChecklistIdOutOfRange)]
        [InlineData(ChecklistIdThatWasDeleted)]
        [InlineData(ChecklistIdOwnedByAnotherUser)]
        [InlineData(ChecklistItemIdThatDoesNotExist)]
        public async void Get_item_for_invalid_parent_fails(long invalidParentId)
        {
            var response = await client.GetAsync("/api/v1/checklists/" + invalidParentId + "/items/" + ChecklistItemIdThatIsValid);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(ChecklistIdOutOfRange)]
        [InlineData(ChecklistIdThatWasDeleted)]
        [InlineData(ChecklistIdOwnedByAnotherUser)]
        [InlineData(ChecklistItemIdThatDoesNotExist)]
        public async void Put_item_for_invalid_parent_fails(int invalidParentId)
        {
            var dto = new ChecklistItemDto();
            dto.Id = ChecklistItemThatIsValid.Id;
            dto.ParentId = ChecklistItemThatIsValid.ParentId;
            dto.ChecklistTemplateItemId = ChecklistItemThatIsValid.ChecklistTemplateItemId;
            dto.Completed = DateTime.Now;

            var response = await client.PutAsync("/api/v1/checklists/" + invalidParentId + "/items/" + ChecklistItemIdThatIsValid, SerializeBodyAsJson(dto));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Theory]
        [InlineData(ChecklistItemIdOutOfRange)]
//        [InlineData(ChecklistItemIdOwnedByAnotherChecklist)]
//        [InlineData(ChecklistItemIdOwnedByAnotherUser)]
        [InlineData(ChecklistItemIdThatDoesNotExist)]
        public async void Put_item_for_invalid_item_fails(int invalidItemId)
        {
            var dto = new ChecklistItemDto();
            dto.Id = invalidItemId;
            dto.ParentId = ChecklistItemThatIsValid.ParentId;
            dto.ChecklistTemplateItemId = ChecklistItemThatIsValid.ChecklistTemplateItemId;
            dto.Completed = DateTime.Now;

            var response = await client.PutAsync("/api/v1/checklists/" + ChecklistIdThatIsValid + "/items/" + invalidItemId, SerializeBodyAsJson(dto));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(GetInvalidDtosForPut))]
        public async void Put_fails_on_invalid_dtos(object dto)
        {
            var putResponse = await client.PutAsync("/api/v1/checklists/" + ChecklistIdThatIsValid + "/items/" + ChecklistItemForSuccessfulPut.Id, SerializeBodyAsJson(dto));
            Assert.Equal<HttpStatusCode>(HttpStatusCode.BadRequest, putResponse.StatusCode);
        }

        protected static IEnumerable<object[]> GetInvalidDtosForPut()
        {
            return new List<object[]>
            {
                new object[] { new { } },
                new object[] { new { Completed = DateTime.Now } },
                new object[] { new { ChecklistTemplateItemId = 912 } },
            };
        }

        [Fact]
        public async void Put_succeeds_on_valid_dtos()
        {
            var dto = new ChecklistItemDto()
            {
                Id = ChecklistItemForSuccessfulPut.Id,
                ParentId = ChecklistItemForSuccessfulPut.ParentId,
                ChecklistTemplateItemId = ChecklistItemForSuccessfulPut.ChecklistTemplateItemId,
                Completed = DateTime.Now
            };

            var putResponse = await client.PutAsync("/api/v1/checklists/" + ChecklistIdThatIsValid + "/items/" + dto.Id, SerializeBodyAsJson(dto));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.NoContent, putResponse.StatusCode);

            var getSingularResponse = await client.GetAsync("/api/v1/checklists/" + ChecklistIdThatIsValid + "/items/" + dto.Id);
            getSingularResponse.EnsureSuccessStatusCode();

            var getSingularData = await getSingularResponse.Content.ReadAsAsync<ChecklistItemDto>();

            Assert.Equal(dto.Id, getSingularData.Id);
            Assert.Equal(dto.ParentId, getSingularData.ParentId);
            Assert.Equal(dto.ChecklistTemplateItemId, getSingularData.ChecklistTemplateItemId);
            Assert.Equal(dto.Completed, getSingularData.Completed);

            var getPluralResponse = await client.GetAsync("/api/v1/checklists/" + ChecklistIdThatIsValid + "/items/");
            getPluralResponse.EnsureSuccessStatusCode();

            var getPluralData = await getPluralResponse.Content.ReadAsAsync<BaseDtoCollection<ChecklistItem, ChecklistItemDto>>();

            Assert.True(getPluralData.Items.Any((i) => {
                return i.Id == dto.Id &&
                    i.ParentId == dto.ParentId &&
                    i.ChecklistTemplateItemId == dto.ChecklistTemplateItemId &&
                    i.Completed == dto.Completed;
            }));
        }

        protected StringContent SerializeBodyAsJson(object body)
        {
            return new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        }

    }
}
