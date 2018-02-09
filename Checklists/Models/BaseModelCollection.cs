using System.Collections.Generic;

namespace Checklists.Models
{
    public class BaseModelCollection<T> where T : BaseModel
    {
        public IEnumerable<T> Items { get; set; }
    }
}