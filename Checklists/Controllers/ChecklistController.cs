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
    [Route("api/v1/checklists")]
    [Authorize(AuthenticationSchemes = "RapidApi")]
    public class ChecklistController : BaseController<Checklist, ChecklistDto>
    {
        public ChecklistController(
            BaseTranslator<Checklist, ChecklistDto> translator,
            BaseValidator<Checklist> validator,
            ChecklistRepository repo,
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