using Framework.Models;
using Framework.Dtos;
using Checklists.Models;

namespace Checklists.Dtos
{
    public class ChecklistTemplateDto : BaseDto<ChecklistTemplate>
    {
        public string Name { get; set; }
    }
}