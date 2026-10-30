using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DevShop.Common;
using DevShop.DTOs;
using DevShop.Models;
using DevShop.Repositories;

namespace DevShop.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(IRepository<Category> categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<IEnumerable<CategoryReadDto>>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<CategoryReadDto>>(categories);
            return ServiceResult<IEnumerable<CategoryReadDto>>.Success(dtos);
        }

        public async Task<ServiceResult<CategoryReadDto>> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return ServiceResult<CategoryReadDto>.NotFound($"Категория с Id={id} не найдена.");
            }

            var dto = _mapper.Map<CategoryReadDto>(category);
            return ServiceResult<CategoryReadDto>.Success(dto);
        }

        public async Task<ServiceResult<CategoryReadDto>> CreateCategoryAsync(CategoryCreateDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            var resultDto = _mapper.Map<CategoryReadDto>(category);
            return ServiceResult<CategoryReadDto>.Success(resultDto);
        }

        public async Task<ServiceResult<CategoryReadDto>> UpdateCategoryAsync(int id, CategoryUpdateDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return ServiceResult<CategoryReadDto>.NotFound($"Категория с Id={id} не найдена.");
            }

            _mapper.Map(dto, category);
            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();

            var resultDto = _mapper.Map<CategoryReadDto>(category);
            return ServiceResult<CategoryReadDto>.Success(resultDto);
        }

        public async Task<ServiceResult<CategoryReadDto>> PatchCategoryAsync(int id, PatchCategoryDTO dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return ServiceResult<CategoryReadDto>.NotFound($"Категория с Id={id} не найдена.");
            }

            if (dto.CategoryName != null) category.CategoryName = dto.CategoryName;
            if (dto.CategoryDescription != null) category.CategoryDescription = dto.CategoryDescription;

            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();

            var resultDto = _mapper.Map<CategoryReadDto>(category);
            return ServiceResult<CategoryReadDto>.Success(resultDto);
        }

        public async Task<ServiceResult<bool>> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return ServiceResult<bool>.NotFound($"Категория с Id={id} не найдена.");
            }

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }
    }
}
