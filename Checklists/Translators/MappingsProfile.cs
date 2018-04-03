using AutoMapper;
using Checklists.Models;
using Checklists.Dtos;
using Framework.Validators;
using Framework.Dtos;

namespace Checklists.Translators 
{
    public class MappingsProfile : Profile
    {
        public MappingsProfile()
        {
            CreateMap<ChecklistTemplate, ChecklistTemplateDto>()
                .ReverseMap();
            CreateMap<ChecklistTemplateItem, ChecklistTemplateItemDto>()
                .ReverseMap();
            CreateMap<ValidationError, ValidationErrorDto>()
                .ReverseMap();
        }
    }
}
