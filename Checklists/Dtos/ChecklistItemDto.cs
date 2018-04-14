using Framework.Models;
using Framework.Dtos;
using Checklists.Models;
using System;

namespace Checklists.Dtos
{
    public class ChecklistItemDto : BaseChildDto<ChecklistItem, Checklist>
    {
        public DateTime? Completed { get; set; }

        public long ChecklistTemplateItemId { get; set; }
    }
}