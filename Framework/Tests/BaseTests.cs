using System;
using Xunit;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Framework.Auth;
using Framework.Models;
using Framework.Dtos;
using Framework.Validators;
using Framework.Repositories;
using Newtonsoft.Json;
using Framework.Startup;
using Microsoft.EntityFrameworkCore;

namespace Framework.Tests
{
    public abstract class BaseTest<TStartup, TDbContext, TModel, TDto>
        where TStartup : class
        where TDbContext : DbContext
        where TModel : BaseModel
        where TDto : BaseDto<TModel>
    {
        protected readonly TestServer _server;
        protected readonly HttpClient _client;

        protected const string DefaultUsername = "alice";
        private const int IdThatIsOutOfRange = -1;
        private const int IdThatDoesNotExist = 200;
        private const int IdThatWasDeleted = 30;
        private const int IdOwnedBySomeoneElse = 100;
        protected const int IdThatIsValid = 10;
        private TModel ItemForCheckingGet;

        protected const int IdForFailedPut = 40;
        protected const int IdForSuccessfulPut = 50;
        protected TModel ItemForCheckingPut;
        protected const int IdToDelete = 60;

        private string BaseRoute;
        private TDbContext _context;

        private BaseRepository<TModel> Repository;

        public BaseTest(string baseRoute)
        {
            BaseRoute = baseRoute;
            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.ConfigureServices(s => s.AddSingleton<IStartupConfigurationService, TestStartupConfigurationService<TDbContext>>());

            _server = new TestServer(webHostBuilder.UseStartup<TStartup>());
            _context = _server.Host.Services.GetService(typeof(TDbContext)) as TDbContext;

            Repository = SetupRepository(_context);
            _client = SetupClient();

            SetupTestData();
        }

        protected abstract BaseRepository<TModel> SetupRepository(TDbContext context);

        private HttpClient SetupClient()
        {
            var client = _server.CreateClient();
            client.DefaultRequestHeaders.Add(RapidApiAuthenticationHandler.RapidApiSecretHeaderName, "TestingAuthSecret");
            client.DefaultRequestHeaders.Add(RapidApiAuthenticationHandler.RapidApiUsernameHeaderName, DefaultUsername);
            return client;
        }


        protected void SetupTestData()
        {
            ItemForCheckingGet = CreateValidModelWithId(IdThatIsValid);
            Repository.CreateItem(ItemForCheckingGet, DefaultUsername);

            Repository.CreateItem(CreateValidModelWithId(2), DefaultUsername);
            Repository.CreateItem(CreateValidModelWithId(IdOwnedBySomeoneElse), "bob");

            var itemToDelete = CreateValidModelWithId(IdThatWasDeleted);
            Repository.CreateItem(itemToDelete, DefaultUsername);
            Repository.DeleteItem(itemToDelete);

            Repository.CreateItem(CreateValidModelWithId(IdForFailedPut), DefaultUsername);

            ItemForCheckingPut = CreateValidModelWithId(IdForSuccessfulPut);
            Repository.CreateItem(ItemForCheckingPut, DefaultUsername);
            Repository.CreateItem(CreateValidModelWithId(IdToDelete), DefaultUsername);
        }

        protected abstract TModel CreateValidModelWithId(long id);

        // todo: inject translator so we don't have to kick out a dto
        protected abstract Tuple<TModel, TDto> CreateValidModelAndDtoWithoutId();
        protected abstract Tuple<TModel, TDto> ChangeModelAndDtoToValidState(TModel oldModel);

        protected abstract bool IsEqual(TModel expected, TDto actual);

#region "Get plural"

        [Fact]
        public async void Get_plural_includes_only_valid_ids()
        {
            var response = await _client.GetAsync(BaseRoute);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsAsync<BaseDtoCollection<TModel, TDto>>();
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
            var response = await _client.GetAsync(BaseRoute + IdThatIsValid);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsAsync<TDto>();
            Assert.Equal(IdThatIsValid, data.Id);
            Assert.True(IsEqual(ItemForCheckingGet, data));
        }


        [Theory]
        [InlineData(IdThatIsOutOfRange)]
        [InlineData(IdThatWasDeleted)]
        [InlineData(IdThatDoesNotExist)]
        [InlineData(IdOwnedBySomeoneElse)]
        public async void Get_singular_fails_on_invalid_id(int invalidId)
        {
            var response = await _client.GetAsync(BaseRoute + invalidId);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }
#endregion

#region "Post"
        [Theory]
        [InlineData("")]
        // todo: abstract this malformed-but-otherwise-valid-dto logic away
        [InlineData("name=blah")]
        public async void Post_fails_on_malformed_body(string body)
        {
            var response = await _client.PostAsync(BaseRoute,
                new StringContent(body, Encoding.UTF8, "application/json"));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.BadRequest, response.StatusCode);
            
            var data = await response.Content.ReadAsAsync<ValidationErrorDtoCollection>();
            Assert.True(data.Items.Any(i => !string.IsNullOrEmpty(i.ErrorCode) && 
                i.ErrorCode.Equals(ValidationErrorDto.NullCreateInput.ErrorCode)));
        }

        protected async void Post_fails_on_invalid_dtos_base(object dto)
        {
            var response = await _client.PostAsync(BaseRoute, SerializeBodyAsJson(dto));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.BadRequest, response.StatusCode);
            
            var data = await response.Content.ReadAsAsync<ValidationErrorDtoCollection>();
            Assert.True(data.Items.Any(i => !string.IsNullOrEmpty(i.ErrorCode) && 
                i.ErrorCode.Equals(ValidationError.PropertyInvalidErrorCode)));
        }

        [Fact]
        public async void Post_successfully_creates_item()
        {
            var tuple = CreateValidModelAndDtoWithoutId();
            var model = tuple.Item1;
            var dto = tuple.Item2;

            var postResponse = await _client.PostAsync(BaseRoute, SerializeBodyAsJson(dto));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.Created, postResponse.StatusCode);

            var data = await postResponse.Content.ReadAsAsync<TDto>();
            Assert.True(IsEqual(model, data));
            Assert.True(data.Id > 0);

            var createdId = data.Id;

            var getSingularResponse = await _client.GetAsync(BaseRoute + createdId);
            getSingularResponse.EnsureSuccessStatusCode();

            var getSingularData = await getSingularResponse.Content.ReadAsAsync<TDto>();
            Assert.Equal(createdId, getSingularData.Id);
            Assert.True(IsEqual(model, getSingularData));

            var getPluralResponse = await _client.GetAsync(BaseRoute);
            getPluralResponse.EnsureSuccessStatusCode();

            var getPluralData = await getPluralResponse.Content.ReadAsAsync<BaseDtoCollection<TModel, TDto>>();
            Assert.True(getPluralData.Items.Any(i => i.Id == createdId && IsEqual(model, i)));
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
            var response = await _client.PutAsync(BaseRoute + invalidId, SerializeBodyAsJson(new { Id = invalidId }));
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("{}")]
        [InlineData("{\"foo\": \"bar\"}")]
        [InlineData("id=4")]
        public async void Put_fails_on_malformed_body(string body)
        {
            var response = await _client.PutAsync(BaseRoute + IdForFailedPut, 
                new StringContent(body, Encoding.UTF8, "application/json"));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.BadRequest, response.StatusCode);
            
            var data = await response.Content.ReadAsAsync<ValidationErrorDtoCollection>();
            Assert.True(data.Items.Any(i => !string.IsNullOrEmpty(i.ErrorCode) && 
                i.ErrorCode.Equals(ValidationErrorDto.NullUpdateInput.ErrorCode)));
        }

        protected async void Put_fails_on_invalid_dtos_base(object dto)
        {
            var response = await _client.PutAsync(BaseRoute + IdForFailedPut, SerializeBodyAsJson(dto));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.BadRequest, response.StatusCode);
            
            var data = await response.Content.ReadAsAsync<ValidationErrorDtoCollection>();
            Assert.True(data.Items.Any(i => !string.IsNullOrEmpty(i.ErrorCode) && 
                i.ErrorCode.Equals(ValidationError.PropertyInvalidErrorCode)));
        }

        [Fact]
        public async void Put_successfully_updates_item()
        {
            var tuple = ChangeModelAndDtoToValidState(ItemForCheckingPut);
            var updatedItem = tuple.Item1;
            var updatedDto = tuple.Item2;

            var putResponse = await _client.PutAsync(BaseRoute + IdForSuccessfulPut, SerializeBodyAsJson(updatedDto));

            Assert.Equal<HttpStatusCode>(HttpStatusCode.NoContent, putResponse.StatusCode);
            
            var getSingularResponse = await _client.GetAsync(BaseRoute + IdForSuccessfulPut);
            getSingularResponse.EnsureSuccessStatusCode();

            var getSingularData = await getSingularResponse.Content.ReadAsAsync<TDto>();
            Assert.Equal(IdForSuccessfulPut, getSingularData.Id);
            Assert.True(IsEqual(updatedItem, getSingularData));

            var getPluralResponse = await _client.GetAsync(BaseRoute);
            getPluralResponse.EnsureSuccessStatusCode();

            var getPluralData = await getPluralResponse.Content.ReadAsAsync<BaseDtoCollection<TModel, TDto>>();
            Assert.True(getPluralData.Items.Any(i => i.Id == IdForSuccessfulPut && IsEqual(updatedItem, i)));
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
            var response = await _client.DeleteAsync(BaseRoute + invalidId);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void Delete_successfully_deletes_item()
        {
            var deleteResponse = await _client.DeleteAsync(BaseRoute + IdToDelete);

            Assert.Equal<HttpStatusCode>(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            
            var getSingularResponse = await _client.GetAsync(BaseRoute + IdToDelete);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, getSingularResponse.StatusCode);
            
            var getPluralResponse = await _client.GetAsync(BaseRoute);
            getPluralResponse.EnsureSuccessStatusCode();

            var getPluralData = await getPluralResponse.Content.ReadAsAsync<BaseDtoCollection<TModel, TDto>>();
            Assert.False(getPluralData.Items.Any(i => i.Id == IdToDelete));
        }
#endregion

        protected StringContent SerializeBodyAsJson(object body)
        {
            return new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        }

    }
}
