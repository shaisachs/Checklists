using Framework.Models;

namespace Framework.Dtos
{
    public class BaseChildDto<TModel, TParent> : BaseDto<TModel>
        where TModel : BaseChildModel<TParent>
        where TParent : BaseModel
    {
        public long ParentId { get; set; }
    }
}