using Framework.Models;

namespace Framework.Dtos
{
    public class BaseDto<T> where T : BaseModel
    {
        public long Id { get; set; }
    }
}