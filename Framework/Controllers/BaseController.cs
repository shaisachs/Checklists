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
    public abstract class BaseController<TModel, TDto> : Controller
        where TModel : BaseModel
        where TDto : BaseDto<TModel>
    {
        private readonly BaseTranslator<TModel, TDto> _translator;
        private readonly BaseValidator<TModel> _validator;
        protected BaseRepository<TModel> _repo { get; private set; }
        private readonly ValidationErrorTranslator _errorTranslator;

        public BaseController(
            BaseTranslator<TModel, TDto> translator,
            BaseValidator<TModel> validator,
            BaseRepository<TModel> repo,
            ValidationErrorTranslator errorTranslator)
        {
            _validator = validator;
            _translator = translator;
            _repo = repo;
            _errorTranslator = errorTranslator;
        }

        protected IActionResult GetAllBase(Func<TModel, bool> additionalFilter = null)
        {
            var models = _repo.GetAllItems(CurrentUserName(), additionalFilter);
            var dtos = from model in models select _translator.Translate(model);
            var answer = new BaseDtoCollection<TModel, TDto>() { Items = dtos };

            return new ObjectResult(answer);
        }

        protected IActionResult GetByIdBase(long id)
        {
            var model = _repo.GetSingleItem(id, CurrentUserName());

            if (model == null)
            {
                return NotFound();
            }

            Response.StatusCode = (int) HttpStatusCode.OK;
            return new ObjectResult(_translator.Translate(model));
        }

        protected IActionResult CreateBase(TDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new ValidationErrorDtoCollection(ValidationErrorDto.NullCreateInput));
            }

            var model = _translator.Translate(dto);
            if (model == null)
            {
                return BadRequest(new ValidationErrorDtoCollection(ValidationErrorDto.MalformedInput));                
            }

            var errors = _validator.ValidateForCreate(model, this.User);
            if (errors != null && errors.Any())
            {
                var errorDtos = from error in errors select _errorTranslator.Translate(error);
                return BadRequest(new ValidationErrorDtoCollection(errorDtos));
            }

            model = _repo.CreateItem(model, CurrentUserName());

            Response.StatusCode = (int) HttpStatusCode.Created;
            return new ObjectResult(_translator.Translate(model));
        }

        protected IActionResult UpdateBase(long id, TDto newDto)
        {
            if (newDto == null || newDto.Id != id)
            {
                return BadRequest(new ValidationErrorDtoCollection(ValidationErrorDto.NullUpdateInput));
            }

            var newModel = _translator.Translate(newDto);
            if (newModel == null)
            {
                return BadRequest(new ValidationErrorDtoCollection(ValidationErrorDto.MalformedInput));                
            }

            var existingModel = _repo.GetSingleItem(id, CurrentUserName());
            if (existingModel == null)
            {
                return NotFound();
            }

            var errors = _validator.ValidateForUpdate(existingModel, newModel, this.User);
            if (errors != null && errors.Any())
            {
                var errorDtos = from error in errors select _errorTranslator.Translate(error);
                return BadRequest(new ValidationErrorDtoCollection(errorDtos));
            }

            existingModel = UpdateExistingItem(existingModel, newModel);
            _repo.UpdateItem(existingModel);

            return new NoContentResult();
        }

        protected IActionResult DeleteBase(long id)
        {
            var existingModel = _repo.GetSingleItem(id, CurrentUserName());
            if (existingModel == null)
            {
                return NotFound();
            }

            _repo.DeleteItem(existingModel);
            return new NoContentResult();
        }

        // todo: move this into a service class
        protected abstract TModel UpdateExistingItem(TModel existingModel, TModel newModel);

        protected string CurrentUserName()
        {
            return this.User.Identity.Name;
        }
    }
}