using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Checklists.Models;
using Checklists.Dtos;
using Checklists.Translators;
using Checklists.Repositories;
using Checklists.Validators;

namespace Checklists.Controllers
{
    [Route("api/v1/checklists")]
    [Authorize(AuthenticationSchemes = "RapidApi")]
    public class ChecklistController : BaseController<Checklist, ChecklistDto>
    {
        public ChecklistController(
            BaseTranslator<Checklist, ChecklistDto> translator,
            BaseValidator<Checklist> validator,
            ChecklistRepository repo,
            ValidationErrorTranslator errorTranslator) :
            base("GetChecklist", translator, validator, repo, errorTranslator)
        {
        }

        [HttpGet]
        public BaseDtoCollection<Checklist, ChecklistDto> GetAll()
        {
            return base.GetAllBase();
        }

        [HttpGet("{id}", Name = "GetChecklist")]
        public IActionResult GetById(long id)
        {
            return base.GetByIdBase(id);
        }

        [HttpPost]
        public IActionResult Create([FromBody] ChecklistDto item)
        {
            return base.CreateBase(item);
        }

        [HttpPut("{id}")]
        public IActionResult Update(long id, [FromBody] ChecklistDto newItem)
        {
            return base.UpdateBase(id, newItem);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            return base.DeleteBase(id);
        }
        
        protected override Checklist UpdateExistingItem(Checklist existingItem, Checklist newItem) 
        {
            existingItem.Name = newItem.Name;

            return existingItem;
        }

    }
}