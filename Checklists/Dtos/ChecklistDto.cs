using Framework.Models;
using Framework.Dtos;
using Checklists.Models;

namespace Checklists.Dtos
{
    public class ChecklistDto : BaseDto<Checklist>
    {
        public string Name { get; set; }
        public long ChecklistTemplateId { get; set; }
    }
}