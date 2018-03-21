using System.Collections.Generic;
using Framework.Models;

namespace Framework.Dtos
{
    public class BaseDtoCollection<TModel, TDto>
        where TModel : BaseModel
        where TDto : BaseDto<TModel>
    {
        public IEnumerable<TDto> Items { get; set; }
    }
}