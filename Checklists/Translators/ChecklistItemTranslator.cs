using Framework.Models;
using Framework.Dtos;
using Framework.Translators;
using AutoMapper;
using Checklists.Dtos;
using Checklists.Models;

namespace Checklists.Translators
{
    public class ChecklistItemTranslator : BaseTranslator<ChecklistItem, ChecklistItemDto> 
    {
        public ChecklistItemTranslator(IMapper mapper) : base(mapper) { }
    }
}
