using System.ComponentModel.DataAnnotations;
using Framework.Models;

namespace Checklists.Models
{
    public class ChecklistTemplate : BaseModel
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
    }
}