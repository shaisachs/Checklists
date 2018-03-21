using Framework.Models;
using Framework.Dtos;
using Framework.Translators;
using AutoMapper;
using Checklists.Dtos;
using Checklists.Models;

namespace Checklists.Translators
{
    public class ChecklistTranslator : BaseTranslator<Checklist, ChecklistDto> 
    {
        public ChecklistTranslator(IMapper mapper) : base(mapper) { }
    }
}
