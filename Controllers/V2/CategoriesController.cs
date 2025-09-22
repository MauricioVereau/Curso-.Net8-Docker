using ApiEcommerce.Constants;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers.V2
{

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCategoriesOrderById()
        {
            var categories = _categoryRepository.GetCategories().OrderBy(c => c.Id);
            var categoriesDto = new List<CategoryDto>();
            foreach (var category in categories)
            {
                categoriesDto.Add(_mapper.Map<CategoryDto>(category));
            }
            return Ok(categoriesDto);
        }


        [AllowAnonymous]
        [HttpGet("{id:int}", Name = "GetCategory")]
        //[ResponseCache(Duration = 10)]
        [ResponseCache(CacheProfileName = CacheProfiles.Default10)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCategory(int id)
        {
            System.Console.WriteLine($"ID recibido: {id} a las {DateTime.Now}"); // Línea de depuración
            var categories = _categoryRepository.GetCategory(id);
            System.Console.WriteLine($"Respuesta con el ID : {id}");
            if (categories == null)
            {
                return NotFound($"La categoria con el id {id} no existe");
            }
            var categoryDto = _mapper.Map<CategoryDto>(categories);
            return Ok(categoryDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateCateory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (createCategoryDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_categoryRepository.CategoryExists(createCategoryDto.Name))
            {
                ModelState.AddModelError("CustomError", "La categoria ya existe");
                return BadRequest(ModelState);
            }

            var category = _mapper.Map<Category>(createCategoryDto);
            if (!_categoryRepository.CreateCategory(category))
            {
                ModelState.AddModelError("CustomError", "Error al crear la categoria");
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            return CreatedAtRoute("GetCategory", new { id = category.Id }, category);
        }

        [HttpPatch("{id:int}", Name = "UpdateCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateCategory(int id, [FromBody] CreateCategoryDto updateCategoryDto)
        {
            if (!_categoryRepository.CategoryExists(id))
            {
                return NotFound($"La categoría con el id {id} no existe");
            }
            if (updateCategoryDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_categoryRepository.CategoryExists(updateCategoryDto.Name))
            {
                ModelState.AddModelError("CustomError", "La categoría ya existe");
                return BadRequest(ModelState);
            }
            var category = _mapper.Map<Category>(updateCategoryDto);
            category.Id = id;
            if (!_categoryRepository.UpdateCategory(category))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al actualizar el registro {category.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "DeleteCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteCateory(int id)
        {
            if (id == 0 || !_categoryRepository.CategoryExists(id))
            {
                return NotFound($"La categoria con el id {id} no existe");
            }

            var category = _categoryRepository.GetCategory(id);
            if (category == null)
            {
                return NotFound($"La categoria con el id {id} no existe");
            }

            if (!_categoryRepository.DeleteCategory(category))
            {
                ModelState.AddModelError("CustomError", "Algo salió mal al eliminar la categoria {category.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
