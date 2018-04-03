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

namespace Checklists.Tests
{
    public class ChecklistTemplateItemsTest : BaseTest<Startup, ChecklistsContext, ChecklistTemplateItem, ChecklistTemplateItemDto>
    {
// TODO: abstract into basechildtest
// TODO: verify 404s on all routes for: impossible parentid, parent id that doesn't exist, parent id that was deleted, parent id that i don't own
// TODO: verify failures when parentid doesn't match route
// TODO: better setup for parent data
// TODO: all permutations of invalid data (name/desc)

        private const long ParentIdThatIsOutOfRange = -15;
        private const long ParentIdThatWasDeleted = 15;
        private const long ParentIdOwnedBySomeoneElse = 25;
        private const long ParentIdThatIsValidButDoesNotOwnThisItem = 35;
        private const long ParentIdThatDoesNotExist = 215;
        private const long ParentIdThatIsValid = 75;
        
        public ChecklistTemplateItemsTest() : base("/api/v1/checklistTemplates/" + ParentIdThatIsValid + "/items/") { }

        protected override BaseRepository<ChecklistTemplateItem> SetupRepository(ChecklistsContext context)
        {
            var _checklistTemplateRepository = new ChecklistTemplateRepository(context);

            var itemToDelete = new ChecklistTemplate() { Id = ParentIdThatWasDeleted, Name = "test template" };
            _checklistTemplateRepository.CreateItem(itemToDelete, DefaultUsername);
            _checklistTemplateRepository.DeleteItem(itemToDelete);

            _checklistTemplateRepository.CreateItem(new ChecklistTemplate() { Id = ParentIdOwnedBySomeoneElse, Name = "test template" }, "bob");

            _checklistTemplateRepository.CreateItem(new ChecklistTemplate() { Id = ParentIdThatIsValidButDoesNotOwnThisItem, Name = "test template" }, DefaultUsername);
            _checklistTemplateRepository.CreateItem(new ChecklistTemplate() { Id = ParentIdThatIsValid, Name = "test template" }, DefaultUsername);

            return new ChecklistTemplateItemRepository(context);
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
            var response = await _client.GetAsync("/api/v1/checklistTemplates/" + invalidParentId + "/items/" + IdThatIsValid);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);

        }

        [Theory]
        [InlineData(ParentIdThatIsOutOfRange)]
        [InlineData(ParentIdThatWasDeleted)]
        [InlineData(ParentIdThatDoesNotExist)]
        [InlineData(ParentIdOwnedBySomeoneElse)]
        public async void Get_plural_fails_on_invalid_parentid(long invalidParentId)
        {
            var response = await _client.GetAsync("/api/v1/checklistTemplates/" + invalidParentId + "/items/");
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);

        }

        [Theory]
        [InlineData(ParentIdThatIsOutOfRange)]
        [InlineData(ParentIdThatWasDeleted)]
        [InlineData(ParentIdThatDoesNotExist)]
        [InlineData(ParentIdOwnedBySomeoneElse)]
        public async void Post_fails_on_invalid_parentid(long invalidParentId)
        {
            var tuple = CreateValidModelAndDtoWithoutId();
            var model = tuple.Item1;
            var dto = tuple.Item2;

            var response = await _client.PostAsync("/api/v1/checklistTemplates/" + invalidParentId + "/items/", SerializeBodyAsJson(dto));
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
            var tuple = ChangeModelAndDtoToValidState(ItemForCheckingPut);
            var updatedItem = tuple.Item1;
            var updatedDto = tuple.Item2;

            var response = await _client.PutAsync("/api/v1/checklistTemplates/" + invalidParentId + "/items/" + IdForSuccessfulPut, SerializeBodyAsJson(updatedDto));
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
            var response = await _client.DeleteAsync("/api/v1/checklistTemplates/" + invalidParentId + "/items/" + IdToDelete);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        protected override bool IsEqual(ChecklistTemplateItem expected, ChecklistTemplateItemDto actual)
        {
            return expected.Name.Equals(actual.Name) && expected.Description.Equals(actual.Description) && expected.ParentId == actual.ParentId;
        }

        protected override ChecklistTemplateItem CreateValidModelWithId(long id)
        {
            var name = System.Guid.NewGuid().ToString();
            var desc = System.Guid.NewGuid().ToString();
            return new ChecklistTemplateItem { Id = id, ParentId = ParentIdThatIsValid, Name = name, Description = desc };
        }

        protected override Tuple<ChecklistTemplateItem, ChecklistTemplateItemDto> CreateValidModelAndDtoWithoutId()
        {
            var newModel = new ChecklistTemplateItem() { Name = System.Guid.NewGuid().ToString(), Description = System.Guid.NewGuid().ToString(), ParentId = ParentIdThatIsValid };
            var newDto = new ChecklistTemplateItemDto() { Name = newModel.Name, Description = newModel.Description, ParentId = ParentIdThatIsValid };

            return new Tuple<ChecklistTemplateItem, ChecklistTemplateItemDto>(newModel, newDto);
        }

        protected override Tuple<ChecklistTemplateItem, ChecklistTemplateItemDto> ChangeModelAndDtoToValidState(ChecklistTemplateItem oldModel)
        {
            oldModel.Name = System.Guid.NewGuid().ToString();
            oldModel.Description = System.Guid.NewGuid().ToString();

            return new Tuple<ChecklistTemplateItem, ChecklistTemplateItemDto>(oldModel,
                new ChecklistTemplateItemDto() { Id = oldModel.Id, Name = oldModel.Name, Description = oldModel.Description, ParentId = ParentIdThatIsValid });
        }

        [Theory]
        [MemberData(nameof(GetInvalidDtosForPost))]
        public async void Post_fails_on_invalid_dtos(object dto)
        {
            Post_fails_on_invalid_dtos_base(dto);
        }

        [Theory]
        [MemberData(nameof(GetInvalidDtosForPut))]
        public async void Put_fails_on_invalid_dtos(object dto)
        {
            Put_fails_on_invalid_dtos_base(dto);
        }

        protected new static IEnumerable<object[]> GetInvalidDtosForPost()
        {
            return new List<object[]>
            {
                new object[] { new { } },
                new object[] { new { Name = new String('a', 51) } }
            };
        }

        protected new static IEnumerable<object[]> GetInvalidDtosForPut()
        {
            return new List<object[]>
            {
                new object[] { new { Id = IdForFailedPut } },
                new object[] { new { Id = IdForFailedPut, Name = new String('a', 51) } }
            };
        }

        [Fact]
        public async new void Get_singular_succeeds_on_valid_id()
        {
            base.Get_singular_succeeds_on_valid_id();
        }
    }

}

