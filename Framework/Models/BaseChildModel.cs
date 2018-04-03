using System;

namespace Framework.Models
{
    public abstract class BaseChildModel<TParent> : BaseModel
        where TParent : BaseModel
    {
        public long ParentId { get; set; }
    }
}