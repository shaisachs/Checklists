using System.Collections.Generic;
using Framework.Models;

namespace Framework.Dtos
{
    public class ValidationErrorDtoCollection
    {
        public IEnumerable<ValidationErrorDto> Items { get; private set; }

        public ValidationErrorDtoCollection()
        {
            Items = new List<ValidationErrorDto>();
        }
        public ValidationErrorDtoCollection(ValidationErrorDto error)
        {
            Items = new[] { error };
        }

         public ValidationErrorDtoCollection(IEnumerable<ValidationErrorDto> errors)
         {
             Items = errors;
         }
       
    }
}