using Checklists.Models;
using Checklists.Dtos;
using AutoMapper;
using Checklists.Validators;

namespace Checklists.Translators
{
    public class ValidationErrorTranslator
    {
        private readonly IMapper _mapper;

        public ValidationErrorTranslator(IMapper mapper)
        {
            _mapper = mapper;
        }

        public ValidationErrorDto Translate(ValidationError error)
        {
            return _mapper.Map<ValidationErrorDto>(error);
        }

    }
}
