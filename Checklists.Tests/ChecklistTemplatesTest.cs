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
    public class ChecklistTemplatesTest : BaseTest<Startup, ChecklistsContext, ChecklistTemplate, ChecklistTemplateDto>
    {
        
        public ChecklistTemplatesTest() : base("/api/v1/checklistTemplates/") { }

        protected override BaseRepository<ChecklistTemplate> SetupRepository(ChecklistsContext context)
        {
            return new ChecklistTemplateRepository(context);
        }

        protected override bool IsEqual(ChecklistTemplate expected, ChecklistTemplateDto actual)
        {
            return expected.Name.Equals(actual.Name);
        }

        protected override ChecklistTemplate CreateValidModelWithId(long id)
        {
            var name = DateTime.Now.ToString();
            return new ChecklistTemplate { Id = id, Name = name };
        }

        protected override Tuple<ChecklistTemplate, ChecklistTemplateDto> CreateValidModelAndDtoWithoutId()
        {
            var newModel = new ChecklistTemplate() { Name = DateTime.Now.ToString() };
            var newDto = new ChecklistTemplateDto() { Name = newModel.Name };

            return new Tuple<ChecklistTemplate, ChecklistTemplateDto>(newModel, newDto);
        }

        protected override Tuple<ChecklistTemplate, ChecklistTemplateDto> ChangeModelAndDtoToValidState(ChecklistTemplate oldModel)
        {
            oldModel.Name = DateTime.Now.ToString();

            return new Tuple<ChecklistTemplate, ChecklistTemplateDto>(oldModel,
                new ChecklistTemplateDto() { Id = oldModel.Id, Name = oldModel.Name });
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

    }

}

