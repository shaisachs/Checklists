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
    public abstract class BaseChildTest<TStartup, TDbContext, TChildModel, TChildDto, TParentModel> : 
        BaseTest<TStartup, TDbContext, TChildModel, TChildDto>
        where TStartup : class
        where TDbContext : DbContext
        where TChildModel : BaseChildModel<TParentModel>
        where TChildDto : BaseDto<TChildModel>
        where TParentModel : BaseModel
    {
        public BaseChildTest(string baseRoute) : base(baseRoute) { }

        protected override void BeforeSetupRepository(TDbContext context)
        {
            ParentRepo = SetupParentRepository(context);
        }

        protected override void BeforeSetupTestData()
        {
            SetupParentTestData();
        }

        protected const long ParentIdThatIsOutOfRange = -15;
        protected const long ParentIdThatWasDeleted = 15;
        protected const long ParentIdOwnedBySomeoneElse = 25;
        protected const long ParentIdThatIsValidButDoesNotOwnThisItem = 35;
        protected const long ParentIdThatDoesNotExist = 215;
        protected const long ParentIdThatIsValid = 75;

        protected BaseRepository<TParentModel> ParentRepo { get; set; }

        protected abstract BaseRepository<TParentModel> SetupParentRepository(TDbContext context);
        
        protected abstract TParentModel CreateValidParentModelWithId(long id);

        protected abstract string CreateBasePath(long parentId);

        protected void SetupParentTestData()
        {
            var itemToDelete = CreateValidParentModelWithId(ParentIdThatWasDeleted);
            ParentRepo.CreateItem(itemToDelete, DefaultUsername);
            ParentRepo.DeleteItem(itemToDelete);

            ParentRepo.CreateItem(CreateValidParentModelWithId(ParentIdOwnedBySomeoneElse), "bob");

            ParentRepo.CreateItem(CreateValidParentModelWithId(ParentIdThatIsValidButDoesNotOwnThisItem), DefaultUsername);
            ParentRepo.CreateItem(CreateValidParentModelWithId(ParentIdThatIsValid), DefaultUsername);
        }

#region "NotFounds for bad parent id"

        [Theory]
        [InlineData(ParentIdThatIsOutOfRange)]
        [InlineData(ParentIdThatWasDeleted)]
        [InlineData(ParentIdThatDoesNotExist)]
        [InlineData(ParentIdThatIsValidButDoesNotOwnThisItem)]
        [InlineData(ParentIdOwnedBySomeoneElse)]
        public async void Get_singular_fails_on_invalid_parentid(long invalidParentId)
        {
            if (!HasGetSingularRoute) { return ; }
            var response = await _client.GetAsync(CreateBasePath(invalidParentId) + IdThatIsValid);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);

        }

        [Theory]
        [InlineData(ParentIdThatIsOutOfRange)]
        [InlineData(ParentIdThatWasDeleted)]
        [InlineData(ParentIdThatDoesNotExist)]
        [InlineData(ParentIdOwnedBySomeoneElse)]
        public async void Get_plural_fails_on_invalid_parentid(long invalidParentId)
        {
            if (!HasGetPluralRoute) { return ; }
            var response = await _client.GetAsync(CreateBasePath(invalidParentId));
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);

        }

        [Theory]
        [InlineData(ParentIdThatIsOutOfRange)]
        [InlineData(ParentIdThatWasDeleted)]
        [InlineData(ParentIdThatDoesNotExist)]
        [InlineData(ParentIdOwnedBySomeoneElse)]
        public async void Post_fails_on_invalid_parentid(long invalidParentId)
        {
            if (!HasPostPluralRoute) { return ; }

            var tuple = CreateValidModelAndDtoWithoutId();
            var model = tuple.Item1;
            var dto = tuple.Item2;

            var response = await _client.PostAsync(CreateBasePath(invalidParentId), SerializeBodyAsJson(dto));
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(ParentIdThatIsOutOfRange)]
        [InlineData(ParentIdThatWasDeleted)]
        [InlineData(ParentIdThatDoesNotExist)]
        [InlineData(ParentIdThatIsValidButDoesNotOwnThisItem)]
        [InlineData(ParentIdOwnedBySomeoneElse)]
        public async void Put_fails_on_invalid_parentid(long invalidParentId)
        {
            if (!HasPutSingularRoute) { return ; }

            var tuple = ChangeModelAndDtoToValidState(ItemForCheckingPut);
            var updatedItem = tuple.Item1;
            var updatedDto = tuple.Item2;

            var response = await _client.PutAsync(CreateBasePath(invalidParentId) + IdForSuccessfulPut, SerializeBodyAsJson(updatedDto));
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(ParentIdThatIsOutOfRange)]
        [InlineData(ParentIdThatWasDeleted)]
        [InlineData(ParentIdThatDoesNotExist)]
        [InlineData(ParentIdThatIsValidButDoesNotOwnThisItem)]
        [InlineData(ParentIdOwnedBySomeoneElse)]
        public async void Delete_fails_on_invalid_parentid(long invalidParentId)
        {
            if (!HasDeleteSingularRoute) { return ; }
            
            var response = await _client.DeleteAsync(CreateBasePath(invalidParentId) + IdToDelete);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }

#endregion

    }
}
