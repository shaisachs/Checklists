using Framework.Models;
using Framework.Dtos;
using AutoMapper;
using Framework.Validators;

namespace Framework.Translators
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
