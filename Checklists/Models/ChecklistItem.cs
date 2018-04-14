using System.ComponentModel.DataAnnotations;
using Framework.Models;
using System;

namespace Checklists.Models
{
    public class ChecklistItem : BaseChildModel<Checklist>
    {
        [Required]
        public DateTime Completed { get; set; }

        [Required]
        public long ChecklistTemplateItemId { get; set; }
    }
}