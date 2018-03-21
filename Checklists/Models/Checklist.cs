using System.ComponentModel.DataAnnotations;
using Framework.Models;

namespace Checklists.Models
{
    public class Checklist : BaseModel
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
    }
}