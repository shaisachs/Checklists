using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Framework.Models;
using Framework.Dtos;
using Framework.Translators;
using Framework.Repositories;
using Framework.Validators;
using Framework.Controllers;
using Checklists.Repositories;
using Checklists.Models;
using Checklists.Dtos;

namespace Checklists.Controllers
{
    [Route("api/v1/checklistTemplates")]
    [Authorize(AuthenticationSchemes = "RapidApi")]
    public class ChecklistTemplateController : BaseController<ChecklistTemplate, ChecklistTemplateDto>
    {
        public ChecklistTemplateController(
            BaseTranslator<ChecklistTemplate, ChecklistTemplateDto> translator,
            BaseValidator<ChecklistTemplate> validator,
            ChecklistTemplateRepository repo,
            ValidationErrorTranslator errorTranslator) :
            base(translator, validator, repo, errorTranslator)
        {
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return base.GetAllBase();
        }

        [HttpGet("{id}")]
        public IActionResult GetById(long id)
        {
            return base.GetByIdBase(id);
        }

        [HttpPost]
        public IActionResult Create([FromBody] ChecklistTemplateDto item)
        {
            return base.CreateBase(item);
        }

        [HttpPut("{id}")]
        public IActionResult Update(long id, [FromBody] ChecklistTemplateDto newItem)
        {
            return base.UpdateBase(id, newItem);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            return base.DeleteBase(id);
        }
        
        protected override ChecklistTemplate UpdateExistingItem(ChecklistTemplate existingItem, ChecklistTemplate newItem) 
        {
            existingItem.Name = newItem.Name;

            return existingItem;
        }

    }
}