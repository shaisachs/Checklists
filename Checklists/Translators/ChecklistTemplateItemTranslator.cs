using Framework.Models;
using Framework.Dtos;
using Framework.Translators;
using AutoMapper;
using Checklists.Dtos;
using Checklists.Models;

namespace Checklists.Translators
{
    public class ChecklistTemplateItemTranslator : BaseTranslator<ChecklistTemplateItem, ChecklistTemplateItemDto> 
    {
        public ChecklistTemplateItemTranslator(IMapper mapper) : base(mapper) { }
    }
}
