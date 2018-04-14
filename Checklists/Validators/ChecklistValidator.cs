using System.Collections.Generic;
using Framework.Models;
using Framework.Dtos;
using Framework.Validators;
using Framework.Repositories;
using Checklists.Models;
using Checklists.Repositories;
using System.Security.Claims;

namespace Checklists.Validators
{
    public class ChecklistValidator : BaseValidator<Checklist>
    {
        public ChecklistValidator(BaseRepository<ChecklistTemplate> templateRepo)
        {
            TemplateRepo = templateRepo;
        }

        protected BaseRepository<ChecklistTemplate> TemplateRepo { get; private set; }

        // todo: better general-purpose attribute for checking foreign key ids?
        protected override IEnumerable<ValidationError> GetCustomValidationErrorsForCreate(Checklist model, ClaimsPrincipal user)
        {
            var answer = new List<ValidationError>();

            // todo: change framework contract to just inject username?
            var template = TemplateRepo.GetSingleItem(model.ChecklistTemplateId, user.Identity.Name);
            if (template == null)
            {
                answer.Add(new ValidationError() { ErrorCode = ValidationError.PropertyInvalidErrorCode, Message = "Checklist Template is not available" });
            }

            return answer;
        }

        protected override IEnumerable<ValidationError> GetCustomValidationErrorsForUpdate(Checklist oldModel, Checklist newModel, ClaimsPrincipal user)
        {
            var answer = new List<ValidationError>();

            if (oldModel.ChecklistTemplateId != newModel.ChecklistTemplateId)
            {
                answer.Add(new ValidationError() { ErrorCode = ValidationError.PropertyInvalidErrorCode, Message = "Checklist Template may not be changed" });
            }

            return answer;
        }
    }
}