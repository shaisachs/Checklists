using System.ComponentModel.DataAnnotations;
using Framework.Models;

namespace Checklists.Models
{
    public class ChecklistTemplateItem : BaseChildModel<ChecklistTemplate>
    {
        [Required, StringLength(50)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }
}