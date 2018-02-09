using AutoMapper;
using Checklists.Models;
using Checklists.Dtos;
using Checklists.Validators;

namespace Checklists.Translators 
{
    public class MappingsProfile : Profile
    {
        public MappingsProfile()
        {
            CreateMap<Checklist, ChecklistDto>()
                .ReverseMap();
            CreateMap<ValidationError, ValidationErrorDto>()
                .ReverseMap();
        }
    }
}
