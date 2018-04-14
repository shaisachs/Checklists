
using System.Collections.Generic;
using Framework.Models;
using Framework.Dtos;
using Checklists.Models;
using Framework.Validators;
using System.Security.Claims;

namespace Checklists.Validators
{
    public class ChecklistItemValidator : BaseValidator<ChecklistItem>
    {

        protected override IEnumerable<ValidationError> GetCustomValidationErrorsForUpdate(ChecklistItem oldModel, ChecklistItem newModel, ClaimsPrincipal user)
        {
            var answer = new List<ValidationError>();

            if (oldModel.ChecklistTemplateItemId != newModel.ChecklistTemplateItemId)
            {
                answer.Add(new ValidationError() { ErrorCode = ValidationError.PropertyInvalidErrorCode, Message = "Checklist Template Item may not be changed" });
            }

            return answer;
        }
    }
}