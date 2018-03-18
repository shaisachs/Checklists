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

namespace Checklists.Tests
{
    public class ChecklistsTest
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private readonly ChecklistsContext _context;

        private const int IdThatIsOutOfRange = -1;
        private const int IdThatDoesNotExist = 200;
        private const int IdThatWasDeleted = 30;
        private const int IdOwnedBySomeoneElse = 100;
        private const int IdThatIsValid = 10;
        private const string NameForValidId = "list 1";
        private const int IdForFailedPut = 40;
        private const int IdForSuccessfulPut = 50;
        private const int IdToDelete = 60;

        public ChecklistsTest()
        {

            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.ConfigureServices(s => s.AddSingleton<IStartupConfigurationService, TestStartupConfigurationService<ChecklistsContext>>());

            _server = new TestServer(webHostBuilder.UseStartup<Startup>());
            _context = _server.Host.Services.GetService(typeof(ChecklistsContext)) as ChecklistsContext;
            _client = _server.CreateClient();

            var repo = new ChecklistRepository(_context);
            repo.CreateItem(new Checklist() { Name = NameForValidId, Id = IdThatIsValid }, "alice");
            repo.CreateItem(new Checklist() { Name = "list 2", Id = 2 }, "alice");
            repo.CreateItem(new Checklist() { Name = "list 10", Id = IdOwnedBySomeoneElse }, "bob");

            var itemToDelete = new Checklist{ Name = "list 3", Id = IdThatWasDeleted };
            repo.CreateItem(itemToDelete, "alice");
            repo.DeleteItem(itemToDelete);

            repo.CreateItem(new Checklist { Name = "list 4", Id = IdForFailedPut }, "alice");
            repo.CreateItem(new Checklist { Name = "list 5", Id = IdForSuccessfulPut }, "alice");
            repo.CreateItem(new Checklist { Name = "list 6", Id = IdToDelete }, "alice");

        }

#region "Get plural"
        [Fact]
        public async void Get_plural_includes_only_valid_ids()
        {
            var response = await _client.GetAsync("/api/v1/checklists/");
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsAsync<BaseDtoCollection<Checklist, ChecklistDto>>();
            var items = data.Items;

            var badIds = new[] { IdThatIsOutOfRange, IdThatWasDeleted, IdThatDoesNotExist, IdOwnedBySomeoneElse };
            foreach (var badId in badIds)
            {
                Assert.False(items.Any(i => i.Id == badId));
            }

            Assert.True(items.Any(i => i.Id == IdThatIsValid));
        }
#endregion

#region "Get singular"
        [Fact]
        public async void Get_singular_succeeds_on_valid_id()
        {
            var response = await _client.GetAsync("/api/v1/checklists/" + IdThatIsValid);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsAsync<ChecklistDto>();
            Assert.Equal(IdThatIsValid, data.Id);
            Assert.Equal(NameForValidId, data.Name);
        }


        [Theory]
        [InlineData(IdThatIsOutOfRange)]
        [InlineData(IdThatWasDeleted)]
        [InlineData(IdThatDoesNotExist)]
        [InlineData(IdOwnedBySomeoneElse)]
        public async void Get_singular_fails_on_invalid_id(int invalidId)
        {
            var response = await _client.GetAsync("/api/v1/checklists/" + invalidId);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }
#endregion

#region "Post"
        [Theory]
        [InlineData("")]
        [InlineData("name=blah")]
        public async void Post_fails_on_malformed_body(string body)
        {
            var response = await _client.PostAsync("/api/v1/checklists/",
                new StringContent(body, Encoding.UTF8, "application/json"));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.BadRequest, response.StatusCode);
            
            var data = await response.Content.ReadAsAsync<ValidationErrorDtoCollection>();
            Assert.True(data.Items.Any(i => !string.IsNullOrEmpty(i.ErrorCode) && 
                i.ErrorCode.Equals(ValidationErrorDto.NullCreateInput.ErrorCode)));
        }

        [Fact]
        public async void Post_fails_on_invalid_body_due_to_long_name()
        {
            var invalidName = new String('a', 51);
            var response = await _client.PostAsync("/api/v1/checklists/",
                new StringContent("{\"name\": \"" + invalidName + "\"}", Encoding.UTF8, "application/json"));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.BadRequest, response.StatusCode);
            
            var data = await response.Content.ReadAsAsync<ValidationErrorDtoCollection>();
            Assert.True(data.Items.Any(i => !string.IsNullOrEmpty(i.ErrorCode) && 
                i.ErrorCode.Equals(ValidationError.PropertyInvalidErrorCode)));
        }

        [Fact]
        public async void Post_fails_on_invalid_body_due_to_missing_name()
        {
            var invalidName = new String('a', 51);
            var response = await _client.PostAsync("/api/v1/checklists/",
                new StringContent("{}", Encoding.UTF8, "application/json"));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.BadRequest, response.StatusCode);
            
            var data = await response.Content.ReadAsAsync<ValidationErrorDtoCollection>();
            Assert.True(data.Items.Any(i => !string.IsNullOrEmpty(i.ErrorCode) && 
                i.ErrorCode.Equals(ValidationError.PropertyInvalidErrorCode)));
        }

        [Fact]
        public async void Post_successfully_creates_item()
        {
            var newName = "new name";
            var postResponse = await _client.PostAsync("/api/v1/checklists/", 
                new StringContent("{\"name\": \"" + newName + "\"}", Encoding.UTF8, "application/json"));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.Created, postResponse.StatusCode);

            var data = await postResponse.Content.ReadAsAsync<ChecklistDto>();
            Assert.Equal(newName, data.Name);
            Assert.True(data.Id > 0);

            var createdId = data.Id;

            var getSingularResponse = await _client.GetAsync("/api/v1/checklists/" + createdId);
            getSingularResponse.EnsureSuccessStatusCode();

            var getSingularData = await getSingularResponse.Content.ReadAsAsync<ChecklistDto>();
            Assert.Equal(createdId, getSingularData.Id);
            Assert.Equal(newName, getSingularData.Name);

            var getPluralResponse = await _client.GetAsync("/api/v1/checklists/");
            getPluralResponse.EnsureSuccessStatusCode();

            var getPluralData = await getPluralResponse.Content.ReadAsAsync<BaseDtoCollection<Checklist, ChecklistDto>>();
            Assert.True(getPluralData.Items.Any(i => i.Id == createdId && !string.IsNullOrEmpty(i.Name) && i.Name.Equals(newName)));
        }
#endregion

#region "Put"
        [Theory]
        [InlineData(IdThatIsOutOfRange)]
        [InlineData(IdThatWasDeleted)]
        [InlineData(IdThatDoesNotExist)]
        [InlineData(IdOwnedBySomeoneElse)]
        public async void Put_singular_fails_on_invalid_id(int invalidId)
        {
            var response = await _client.PutAsync("/api/v1/checklists/" + invalidId, 
                new StringContent("{\"id\":" + invalidId + "}", Encoding.UTF8, "application/json"));
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("{}")]
        [InlineData("{\"foo\": \"bar\"}")]
        [InlineData("id=4")]
        public async void Put_fails_on_malformed_body(string body)
        {
            var response = await _client.PutAsync("/api/v1/checklists/" + IdForFailedPut, 
                new StringContent(body, Encoding.UTF8, "application/json"));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.BadRequest, response.StatusCode);
            
            var data = await response.Content.ReadAsAsync<ValidationErrorDtoCollection>();
            Assert.True(data.Items.Any(i => !string.IsNullOrEmpty(i.ErrorCode) && 
                i.ErrorCode.Equals(ValidationErrorDto.NullUpdateInput.ErrorCode)));
        }

        [Fact]
        public async void Put_fails_on_invalid_body_due_to_long_name()
        {
            var invalidName = new String('a', 51);
            var response = await _client.PutAsync("/api/v1/checklists/" + IdForFailedPut, 
                new StringContent("{\"id\": " + IdForFailedPut + ", \"name\": \"" + invalidName + "\"}", Encoding.UTF8, "application/json"));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.BadRequest, response.StatusCode);
            
            var data = await response.Content.ReadAsAsync<ValidationErrorDtoCollection>();
            Assert.True(data.Items.Any(i => !string.IsNullOrEmpty(i.ErrorCode) && 
                i.ErrorCode.Equals(ValidationError.PropertyInvalidErrorCode)));
        }

        [Fact]
        public async void Put_fails_on_invalid_body_due_to_missing_name()
        {
            var response = await _client.PutAsync("/api/v1/checklists/" + IdForFailedPut, 
                new StringContent("{\"id\": 40}", Encoding.UTF8, "application/json"));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.BadRequest, response.StatusCode);
            
            var data = await response.Content.ReadAsAsync<ValidationErrorDtoCollection>();
            Assert.True(data.Items.Any(i => !string.IsNullOrEmpty(i.ErrorCode) && 
                i.ErrorCode.Equals(ValidationError.PropertyInvalidErrorCode)));
        }

        [Fact]
        public async void Put_successfully_updates_item()
        {
            var newName = "new name";
            var putResponse = await _client.PutAsync("/api/v1/checklists/" + IdForSuccessfulPut, 
                new StringContent("{\"id\":" + IdForSuccessfulPut + ", \"name\": \"" + newName + "\"}", Encoding.UTF8, "application/json"));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.NoContent, putResponse.StatusCode);
            
            var getSingularResponse = await _client.GetAsync("/api/v1/checklists/" + IdForSuccessfulPut);
            getSingularResponse.EnsureSuccessStatusCode();

            var getSingularData = await getSingularResponse.Content.ReadAsAsync<ChecklistDto>();
            Assert.Equal(IdForSuccessfulPut, getSingularData.Id);
            Assert.Equal(newName, getSingularData.Name);

            var getPluralResponse = await _client.GetAsync("/api/v1/checklists/");
            getPluralResponse.EnsureSuccessStatusCode();

            var getPluralData = await getPluralResponse.Content.ReadAsAsync<BaseDtoCollection<Checklist, ChecklistDto>>();
            Assert.True(getPluralData.Items.Any(i => i.Id == IdForSuccessfulPut && !string.IsNullOrEmpty(i.Name) && i.Name.Equals(newName)));
        }
#endregion

#region "Delete"
        [Theory]
        [InlineData(IdThatIsOutOfRange)]
        [InlineData(IdThatWasDeleted)]
        [InlineData(IdThatDoesNotExist)]
        [InlineData(IdOwnedBySomeoneElse)]
        public async void Delete_singular_fails_on_invalid_id(int invalidId)
        {
            var response = await _client.DeleteAsync("/api/v1/checklists/" + invalidId);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void Delete_successfully_updates_item()
        {
            var deleteResponse = await _client.DeleteAsync("/api/v1/checklists/" + IdToDelete);

            Assert.Equal<HttpStatusCode>(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            
            var getSingularResponse = await _client.GetAsync("/api/v1/checklists/" + IdToDelete);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, getSingularResponse.StatusCode);
            
            var getPluralResponse = await _client.GetAsync("/api/v1/checklists/");
            getPluralResponse.EnsureSuccessStatusCode();

            var getPluralData = await getPluralResponse.Content.ReadAsAsync<BaseDtoCollection<Checklist, ChecklistDto>>();
            Assert.False(getPluralData.Items.Any(i => i.Id == IdToDelete));
        }
#endregion

    }
}
