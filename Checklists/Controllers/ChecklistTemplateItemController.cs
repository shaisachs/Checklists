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
    [Authorize(AuthenticationSchemes = "RapidApi")]
    public class ChecklistTemplateItemController :
        BaseChildController<ChecklistTemplateItem, ChecklistTemplateItemDto, ChecklistTemplate>
    {
        public ChecklistTemplateItemController(
            BaseTranslator<ChecklistTemplateItem, ChecklistTemplateItemDto> translator,
            BaseValidator<ChecklistTemplateItem> validator,
            ChecklistTemplateItemRepository repo,
            ChecklistTemplateRepository parentRepo,
            ValidationErrorTranslator errorTranslator) :
            base(translator, validator, repo, parentRepo, errorTranslator)
        {
        }

        [HttpGet("api/v1/checklistTemplates/{checklistTemplateId}/items")]
        public IActionResult GetAll(long checklistTemplateId)
        {
            return base.GetAllBase(checklistTemplateId);
        }

        [HttpGet("api/v1/checklistTemplates/{checklistTemplateId}/items/{id}")]
        public IActionResult GetById(long checklistTemplateId, long id)
        {
            return base.GetByIdBase(checklistTemplateId, id);
        }

        [HttpPost("api/v1/checklistTemplates/{checklistTemplateId}/items/")]
        public IActionResult Create(long checklistTemplateId, [FromBody] ChecklistTemplateItemDto item)
        {
            return base.CreateBase(checklistTemplateId, item);
        }

        [HttpPut("api/v1/checklistTemplates/{checklistTemplateId}/items/{id}")]
        public IActionResult Update(long checklistTemplateId, long id, [FromBody] ChecklistTemplateItemDto newItem)
        {
            return base.UpdateBase(checklistTemplateId, id, newItem);
        }

        [HttpDelete("api/v1/checklistTemplates/{checklistTemplateId}/items/{id}")]
        public IActionResult Delete(long checklistTemplateId, long id)
        {
            return base.DeleteBase(checklistTemplateId, id);
        }
        
        // todo: move to service
        protected override ChecklistTemplateItem UpdateExistingItem(ChecklistTemplateItem existingItem, ChecklistTemplateItem newItem) 
        {
            existingItem.Name = newItem.Name;
            existingItem.Description = newItem.Description;

            return existingItem;
        }

    }
}