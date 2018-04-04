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
    public class ChecklistTemplateItemsTest : BaseChildTest<Startup, ChecklistsContext, ChecklistTemplateItem, ChecklistTemplateItemDto, ChecklistTemplate>
    {
// TODO: abstract into basechildtest
// TODO: better setup for parent data
// TODO: all permutations of invalid data (name/desc)

        public ChecklistTemplateItemsTest() : base("/api/v1/checklistTemplates/" + ParentIdThatIsValid + "/items/") { }

        protected override BaseRepository<ChecklistTemplateItem> SetupRepository(ChecklistsContext context)
        {
            return new ChecklistTemplateItemRepository(context);
        }

        protected override BaseRepository<ChecklistTemplate> SetupParentRepository(ChecklistsContext context)
        {
            return new ChecklistTemplateRepository(context);
        }
        protected override ChecklistTemplate CreateValidParentModelWithId(long id)
        {
            var name = DateTime.Now.ToString();
            return new ChecklistTemplate { Id = id, Name = name };
        }


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
                // name absent / fine / too long
                // desc absent / fine / too long
                new object[] { new { } },
                new object[] { new { Description = "a" } },
                new object[] { new { Description = new String('a', 501) } },

                //new object[] { new { Name = "a" } },
                //new object[] { new { Name = "a", Description = "a" } },
                new object[] { new { Name = "a", Description = new String('a', 501) } },

                new object[] { new { Name = new String('a', 51) } },
                new object[] { new { Name = new String('a', 51), Description = "a" } },
                new object[] { new { Name = new String('a', 51), Description = new String('a', 501) } },
            };
        }

        protected new static IEnumerable<object[]> GetInvalidDtosForPut()
        {
            return new List<object[]>
            {
                new object[] { new { Id = IdForFailedPut } },
                new object[] { new { Id = IdForFailedPut, Description = "a" } },
                new object[] { new { Id = IdForFailedPut, Description = new String('a', 501) } },

                new object[] { new { Id = IdForFailedPut, Name = "a", Description = new String('a', 501) } },

                new object[] { new { Id = IdForFailedPut, Name = new String('a', 51) } },
                new object[] { new { Id = IdForFailedPut, Name = new String('a', 51), Description = "a" } },
                new object[] { new { Id = IdForFailedPut, Name = new String('a', 51), Description = new String('a', 501) } },
            };
        }

    }

}

