using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using DevShop.Common;
using DevShop.DTOs;
using DevShop.Models;
using DevShop.Repositories;

namespace DevShop.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public ProductService(
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<PagedResult<ProductReadDto>>> GetPagedProductsAsync(
            int page, 
            int pageSize, 
            int? categoryId = null, 
            string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            Expression<Func<Product, bool>>? predicate = null;
            if (categoryId.HasValue && !string.IsNullOrWhiteSpace(search))
            {
                predicate = p => p.CategoryId == categoryId.Value && 
                                (p.ProductName.Contains(search) || (p.ProductDescription != null && p.ProductDescription.Contains(search)));
            }
            else if (categoryId.HasValue)
            {
                predicate = p => p.CategoryId == categoryId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(search))
            {
                predicate = p => p.ProductName.Contains(search) || 
                                (p.ProductDescription != null && p.ProductDescription.Contains(search));
            }

            var pagedEntities = await _productRepository.GetPagedAsync(
                page, 
                pageSize, 
                predicate, 
                q => q.OrderBy(p => p.IdProducts),
                p => p.Category!);

            var dtos = _mapper.Map<IEnumerable<ProductReadDto>>(pagedEntities.Items);
            var result = new PagedResult<ProductReadDto>(dtos, pagedEntities.TotalCount, page, pageSize);

            return ServiceResult<PagedResult<ProductReadDto>>.Success(result);
        }

        public async Task<ServiceResult<ProductReadDto>> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return ServiceResult<ProductReadDto>.NotFound($"Товар с Id={id} не найден.");
            }

            // Загружаем категорию для DTO
            if (product.Category == null)
            {
                product.Category = (await _categoryRepository.GetByIdAsync(product.CategoryId))!;
            }

            var dto = _mapper.Map<ProductReadDto>(product);
            return ServiceResult<ProductReadDto>.Success(dto);
        }

        public async Task<ServiceResult<ProductReadDto>> CreateProductAsync(ProductCreateDto dto)
        {
            // Проверяем существование категории
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                return ServiceResult<ProductReadDto>.Failed($"Категория с Id={dto.CategoryId} не существует.", ServiceErrorType.Validation);
            }

            var product = _mapper.Map<Product>(dto);
            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            product.Category = category;
            var resultDto = _mapper.Map<ProductReadDto>(product);
            return ServiceResult<ProductReadDto>.Success(resultDto);
        }

        public async Task<ServiceResult<ProductReadDto>> UpdateProductAsync(int id, ProductUpdateDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return ServiceResult<ProductReadDto>.NotFound($"Товар с Id={id} не найден.");
            }

            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                return ServiceResult<ProductReadDto>.Failed($"Категория с Id={dto.CategoryId} не существует.", ServiceErrorType.Validation);
            }

            _mapper.Map(dto, product);
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            product.Category = category;
            var resultDto = _mapper.Map<ProductReadDto>(product);
            return ServiceResult<ProductReadDto>.Success(resultDto);
        }

        public async Task<ServiceResult<ProductReadDto>> PatchProductAsync(int id, PatchProductDTO dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return ServiceResult<ProductReadDto>.NotFound($"Товар с Id={id} не найден.");
            }

            if (dto.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(dto.CategoryId.Value);
                if (category == null)
                {
                    return ServiceResult<ProductReadDto>.Failed($"Категория с Id={dto.CategoryId.Value} не существует.", ServiceErrorType.Validation);
                }
                product.CategoryId = dto.CategoryId.Value;
            }

            if (dto.ProductName != null) product.ProductName = dto.ProductName;
            if (dto.ProductDescription != null) product.ProductDescription = dto.ProductDescription;
            if (dto.ProductPrice.HasValue) product.ProductPrice = dto.ProductPrice.Value;
            if (dto.ProductStockQuantity.HasValue) product.ProductStockQuantity = dto.ProductStockQuantity.Value;
            if (dto.ProductImageUrl != null) product.ProductImageUrl = dto.ProductImageUrl;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            if (product.Category == null)
            {
                product.Category = (await _categoryRepository.GetByIdAsync(product.CategoryId))!;
            }

            var resultDto = _mapper.Map<ProductReadDto>(product);
            return ServiceResult<ProductReadDto>.Success(resultDto);
        }

        public async Task<ServiceResult<bool>> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return ServiceResult<bool>.NotFound($"Товар с Id={id} не найден.");
            }

            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }
    }
}
