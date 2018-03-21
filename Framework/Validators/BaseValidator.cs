using System.Collections.Generic;
using Framework.Models;
using Framework.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Framework.Validators
{
    public class BaseValidator<TModel>
        where TModel : BaseModel
    {
        public BaseValidator()
        {
        }

        public IEnumerable<ValidationError> ValidateForCreate(TModel model, ClaimsPrincipal user)
        {
            var answer = new List<ValidationError>();

            var context = new ValidationContext(model, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, context, results, true);
            if (!isValid)
            {
                answer.AddRange(from result in results select TranslateValidationResult(result));
            }

            var customErrors = GetCustomValidationErrorsForCreate(model, user);
            if (customErrors != null)
            {
                answer.AddRange(customErrors);
            }

            return answer;
        }
        public IEnumerable<ValidationError> ValidateForUpdate(TModel existingModel, TModel newModel, ClaimsPrincipal user)
        {
            var answer = new List<ValidationError>();

            var context = new ValidationContext(newModel, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(newModel, context, results, true);
            if (!isValid)
            {
                answer.AddRange(from result in results select TranslateValidationResult(result));
            }

            var customErrors = GetCustomValidationErrorsForUpdate(existingModel, newModel, user);
            if (customErrors != null)
            {
                answer.AddRange(customErrors);
            }

            return answer;
        }

        protected virtual IEnumerable<ValidationError> GetCustomValidationErrorsForCreate(TModel model, ClaimsPrincipal user)
        {
            return null;
        }

        protected virtual IEnumerable<ValidationError> GetCustomValidationErrorsForUpdate(TModel existingModel, TModel newModel, ClaimsPrincipal user)
        {
            return null;
        }

        private ValidationError TranslateValidationResult(ValidationResult result)
        {
            return new ValidationError() { ErrorCode = ValidationError.PropertyInvalidErrorCode, Message = result.ErrorMessage };
        }

    }
}