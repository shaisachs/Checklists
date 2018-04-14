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
    public class ChecklistsTest : BaseTest<Startup, ChecklistsContext, Checklist, ChecklistDto>
    {
        
        public ChecklistsTest() : base("/api/v1/checklists/") { }

        protected ChecklistTemplateRepository TemplateRepo { get; set; }
        protected ChecklistTemplateItemRepository TemplateItemRepo { get; set; }

        protected override void BeforeSetupRepository(ChecklistsContext context)
        {
            TemplateRepo = new ChecklistTemplateRepository(context);
            TemplateItemRepo = new ChecklistTemplateItemRepository(context);
        }

        protected override void BeforeSetupTestData()
        {
            var itemToDelete = CreateValidTemplateWithId(TemplateIdThatWasDeleted);
            TemplateRepo.CreateItem(itemToDelete, DefaultUsername);
            TemplateRepo.DeleteItem(itemToDelete);

            TemplateRepo.CreateItem(CreateValidTemplateWithId(TemplateIdOwnedBySomeoneElse), "bob");

            TemplateRepo.CreateItem(CreateValidTemplateWithId(TemplateIdThatIsValidButDoesNotOwnThisItem), DefaultUsername);
            TemplateRepo.CreateItem(CreateValidTemplateWithId(TemplateIdThatIsValid), DefaultUsername);

            var templateItem = new ChecklistTemplateItem() { Name = System.Guid.NewGuid().ToString(), Id = TemplateItemIdBelongingToValidTemplate, ParentId = TemplateIdThatIsValid };
            TemplateItemRepo.CreateItem(templateItem, DefaultUsername);
        }

        protected ChecklistTemplate CreateValidTemplateWithId(long id)
        {
            return new ChecklistTemplate() { Name = System.Guid.NewGuid().ToString(), Id = id };
        }

        protected const long TemplateIdThatIsOutOfRange = -15;
        protected const long TemplateIdThatWasDeleted = 15;
        protected const long TemplateIdOwnedBySomeoneElse = 25;
        protected const long TemplateIdThatIsValidButDoesNotOwnThisItem = 35;
        protected const long TemplateIdThatDoesNotExist = 215;
        protected const long TemplateIdThatIsValid = 75;
        protected const long TemplateItemIdBelongingToValidTemplate = 500;

        protected override BaseRepository<Checklist> SetupRepository(ChecklistsContext context)
        {
            return new ChecklistRepository(context);
        }

        protected override bool IsEqual(Checklist expected, ChecklistDto actual)
        {
            return expected.Name.Equals(actual.Name);
        }

        protected override Checklist CreateValidModelWithId(long id)
        {
            var name = DateTime.Now.ToString();
            return new Checklist { Id = id, Name = name, ChecklistTemplateId = TemplateIdThatIsValid };
        }

        protected override Tuple<Checklist, ChecklistDto> CreateValidModelAndDtoWithoutId()
        {
            var newModel = new Checklist() { Name = DateTime.Now.ToString(), ChecklistTemplateId = TemplateIdThatIsValid };
            var newDto = new ChecklistDto() { Name = newModel.Name, ChecklistTemplateId = TemplateIdThatIsValid };

            return new Tuple<Checklist, ChecklistDto>(newModel, newDto);
        }

        protected override Tuple<Checklist, ChecklistDto> ChangeModelAndDtoToValidState(Checklist oldModel)
        {
            oldModel.Name = DateTime.Now.ToString();

            return new Tuple<Checklist, ChecklistDto>(oldModel,
                new ChecklistDto() { Id = oldModel.Id, Name = oldModel.Name, ChecklistTemplateId = oldModel.ChecklistTemplateId });
        }

        [Fact]
        public async void Post_successfully_creates_items_from_template()
        {            
            var tuple = CreateValidModelAndDtoWithoutId();
            var model = tuple.Item1;
            var dto = tuple.Item2;

            var postResponse = await _client.PostAsync("/api/v1/checklists/", SerializeBodyAsJson(dto));            
            var postData = await postResponse.Content.ReadAsAsync<ChecklistDto>();
            var checklistId = postData.Id;

            var getItemsResponse = await _client.GetAsync("/api/v1/checklists/" + checklistId + "/items/");
            var getItemsData = await getItemsResponse.Content.ReadAsAsync<BaseDtoCollection<ChecklistItem, ChecklistItemDto>>();
            Assert.NotNull(getItemsData);
            Assert.NotNull(getItemsData.Items);
            Assert.True(getItemsData.Items.Any(i => i.ChecklistTemplateItemId == TemplateItemIdBelongingToValidTemplate));
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

        protected static IEnumerable<object[]> GetInvalidDtosForPost()
        {
            // name absent / fine / too long
            // template id absent / fine / bad
            // todo: better code for combinatorially generating bad dtos and sharing post/put logic
            return new List<object[]>
            {
                new object[] { new { } },
                new object[] { new { Name = "a" } },
                new object[] { new { Name = new String('a', 51) } },

                new object[] { new { Name = "a", ChecklistTemplateId =  TemplateIdThatIsOutOfRange } },
                new object[] { new { Name = "a", ChecklistTemplateId =  TemplateIdThatWasDeleted } },
                new object[] { new { Name = "a", ChecklistTemplateId =  TemplateIdOwnedBySomeoneElse } },
                new object[] { new { Name = "a", ChecklistTemplateId =  TemplateIdThatDoesNotExist } },

                new object[] { new { Name = new String('a', 51), ChecklistTemplateId =  TemplateIdThatIsOutOfRange } },
                new object[] { new { Name = new String('a', 51), ChecklistTemplateId =  TemplateIdThatWasDeleted } },
                new object[] { new { Name = new String('a', 51), ChecklistTemplateId =  TemplateIdOwnedBySomeoneElse } },
                new object[] { new { Name = new String('a', 51), ChecklistTemplateId =  TemplateIdThatDoesNotExist } }
            };
        }

        protected static IEnumerable<object[]> GetInvalidDtosForPut()
        {
            // no need to test template id here, since we don't update it anyway
            return new List<object[]>
            {
                new object[] { new { Id = IdForFailedPut } },
                new object[] { new { Id = IdForFailedPut, Name = "a" } },
                new object[] { new { Id = IdForFailedPut, Name = new String('a', 51) } },

                new object[] { new { Id = IdForFailedPut, Name = new String('a', 51), ChecklistTemplateId = TemplateIdThatIsValid } },

                new object[] { new { Id = IdForFailedPut, ChecklistTemplateId = TemplateIdThatIsValidButDoesNotOwnThisItem } },
                new object[] { new { Id = IdForFailedPut, Name = "a", ChecklistTemplateId = TemplateIdThatIsValidButDoesNotOwnThisItem } },
                new object[] { new { Id = IdForFailedPut, Name = new String('a', 51), ChecklistTemplateId = TemplateIdThatIsValidButDoesNotOwnThisItem } }
            };
        }

    }

}

