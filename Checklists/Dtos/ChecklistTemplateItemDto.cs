using Framework.Models;
using Framework.Dtos;
using Checklists.Models;

namespace Checklists.Dtos
{
    public class ChecklistTemplateItemDto : BaseChildDto<ChecklistTemplateItem, ChecklistTemplate>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}