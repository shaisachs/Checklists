using Checklists.Models;
using Checklists.Dtos;
using AutoMapper;

namespace Checklists.Translators
{
    public class ChecklistTranslator : BaseTranslator<Checklist, ChecklistDto> 
    {
        public ChecklistTranslator(IMapper mapper) : base(mapper) { }
    }
}
