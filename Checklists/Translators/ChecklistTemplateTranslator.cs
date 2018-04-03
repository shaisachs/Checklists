using Framework.Models;
using Framework.Dtos;
using Framework.Translators;
using AutoMapper;
using Checklists.Dtos;
using Checklists.Models;

namespace Checklists.Translators
{
    public class ChecklistTemplateTranslator : BaseTranslator<ChecklistTemplate, ChecklistTemplateDto> 
    {
        public ChecklistTemplateTranslator(IMapper mapper) : base(mapper) { }
    }
}
