using System.Collections.Generic;
using Checklists.Models;

namespace Checklists.Dtos
{
    public class BaseDtoCollection<TModel, TDto>
        where TModel : BaseModel
        where TDto : BaseDto<TModel>
    {
        public IEnumerable<TDto> Items { get; set; }
    }
}