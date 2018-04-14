using System.ComponentModel.DataAnnotations;
using Framework.Models;

namespace Checklists.Models
{
    public class Checklist : BaseModel
    {
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public long ChecklistTemplateId { get; set; }
    }
}