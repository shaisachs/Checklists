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
    public class ChecklistItemController :
        BaseChildController<ChecklistItem, ChecklistItemDto, Checklist>
    {
        public ChecklistItemController(
            BaseTranslator<ChecklistItem, ChecklistItemDto> translator,
            BaseValidator<ChecklistItem> validator,
            ChecklistItemRepository repo,
            ChecklistRepository parentRepo,
            ValidationErrorTranslator errorTranslator) :
            base(translator, validator, repo, parentRepo, errorTranslator)
        {
        }

        [HttpGet("api/v1/checklists/{checklistId}/items")]
        public IActionResult GetAll(long checklistId)
        {
            return base.GetAllBase(checklistId);
        }

        [HttpGet("api/v1/checklists/{checklistId}/items/{id}")]
        public IActionResult GetById(long checklistId, long id)
        {
            return base.GetByIdBase(checklistId, id);
        }

        [HttpPut("api/v1/checklists/{checklistId}/items/{id}")]
        public IActionResult Update(long checklistId, long id, [FromBody] ChecklistItemDto newItem)
        {
            return base.UpdateBase(checklistId, id, newItem);
        }
        
        // todo: move to service
        protected override ChecklistItem UpdateExistingItem(ChecklistItem existingItem, ChecklistItem newItem) 
        {
            existingItem.Completed = newItem.Completed;

            return existingItem;
        }

    }
}