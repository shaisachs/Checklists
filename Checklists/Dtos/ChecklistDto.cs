using Checklists.Models;

namespace Checklists.Dtos
{
    public class ChecklistDto : BaseDto<Checklist>
    {
        public string Name { get; set; }
    }
}