using Checklists.Models;

namespace Checklists.Dtos
{
    public class BaseDto<T> where T : BaseModel
    {
        public long Id { get; set; }
    }
}