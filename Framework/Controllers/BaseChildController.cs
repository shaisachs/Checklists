using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Framework.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Framework.Dtos;
using Framework.Translators;
using Framework.Repositories;
using Framework.Validators;
using System.Net;

namespace Framework.Controllers
{
    public abstract class BaseChildController<TModel, TDto, TParent> : BaseController<TModel, TDto>
        where TModel : BaseChildModel<TParent>
        where TDto : BaseChildDto<TModel, TParent>
        where TParent : BaseModel
    {
        private readonly BaseRepository<TParent> _parentRepo;

        public BaseChildController(
            BaseTranslator<TModel, TDto> translator,
            BaseValidator<TModel> validator,
            BaseRepository<TModel> repo,
            BaseRepository<TParent> parentRepo,
            ValidationErrorTranslator errorTranslator) : base(translator, validator, repo, errorTranslator)
        {
            _parentRepo = parentRepo;
        }
        
        protected IActionResult GetAllBase(long parentId)
        {
            var checkParentResult = EnsureValidParent(parentId);
            if (checkParentResult != null)
            {
                return checkParentResult;
            }

            return base.GetAllBase(t => t.ParentId == parentId);
        }

        protected IActionResult GetByIdBase(long parentId, long id)
        {
            var checkParentResult = EnsureValidParent(parentId, id);
            if (checkParentResult != null)
            {
                return checkParentResult;
            }

            return base.GetByIdBase(id);
        }

        protected IActionResult CreateBase(long parentId, TDto item)
        {
            var checkParentResult = EnsureValidParent(parentId);
            if (checkParentResult != null)
            {
                return checkParentResult;
            }

            if (item != null)
            {
                item.ParentId = parentId;
            }

            return base.CreateBase(item);
        }

        protected IActionResult UpdateBase(long parentId, long id, TDto newItem)
        {
            var checkParentResult = EnsureValidParent(parentId, id);
            if (checkParentResult != null)
            {
                return checkParentResult;
            }

            return base.UpdateBase(id, newItem);
        }

        protected IActionResult DeleteBase(long parentId, long id)
        {
            var checkParentResult = EnsureValidParent(parentId, id);
            if (checkParentResult != null)
            {
                return checkParentResult;
            }
            
            return base.DeleteBase(id);
        }

        // todo: move to service
        protected IActionResult EnsureValidParent(long parentId)
        {
            var model = _parentRepo.GetSingleItem(parentId, CurrentUserName());

            if (model == null)
            {
                return NotFound();
            }

            return null;
        }

        // todo: move to service
        protected IActionResult EnsureValidParent(long parentId, long id)
        {
            var model = _repo.GetSingleItem(id, CurrentUserName());

            if (model == null)
            {
                return null;
            }

            if (model.ParentId != parentId)
            {
                return NotFound();
            }

            return null;
        }

    }
}