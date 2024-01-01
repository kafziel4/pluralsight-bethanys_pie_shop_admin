using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BethanysPieShopAdmin.Models.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private const string AllCategoriesCacheName = "AllCategories";

        private readonly IMemoryCache _memoryCache;
        private readonly BethanysPieShopDbContext _bethanysPieShopDbContext;

        public CategoryRepository(BethanysPieShopDbContext bethanysPieShopDbContext, IMemoryCache memoryCache)
        {
            _bethanysPieShopDbContext = bethanysPieShopDbContext;
            _memoryCache = memoryCache;
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _bethanysPieShopDbContext.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryId);
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            List<Category> allCategories = null;

            if (!_memoryCache.TryGetValue(AllCategoriesCacheName, out allCategories))
            {
                allCategories = await _bethanysPieShopDbContext.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryId)
                .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60));

                _memoryCache.Set(AllCategoriesCacheName, allCategories, cacheEntryOptions);
            }

            return allCategories;
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _bethanysPieShopDbContext.Categories
                .AsNoTracking()
                .Include(c => c.Pies)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<int> AddCategoryAsync(Category category)
        {
            bool categoryWithSameNameExists = await _bethanysPieShopDbContext.Categories
                .AnyAsync(c => c.Name == category.Name);

            if (categoryWithSameNameExists)
            {
                throw new Exception("A category with the same name already exists");
            }

            _bethanysPieShopDbContext.Categories.Add(category);

            int result = await _bethanysPieShopDbContext.SaveChangesAsync();

            _memoryCache.Remove(AllCategoriesCacheName);

            return result;
        }

        public async Task<int> UpdateCategoryAsync(Category category)
        {
            bool categoryWithSameNameExists = await _bethanysPieShopDbContext.Categories
                .AnyAsync(c => c.Name == category.Name && c.CategoryId != category.CategoryId);

            if (categoryWithSameNameExists)
            {
                throw new Exception("A category with the same name already exists");
            }

            var categoryToUpdate = await _bethanysPieShopDbContext.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);

            if (categoryToUpdate == null)
            {
                throw new ArgumentException("The category to update can't be found.");
            }

            categoryToUpdate.Name = category.Name;
            categoryToUpdate.Description = category.Description;

            int result = await _bethanysPieShopDbContext.SaveChangesAsync();

            _memoryCache.Remove(AllCategoriesCacheName);

            return result;
        }

        public async Task<int> DeleteCategoryAsync(int id)
        {
            var categoryToDelete = await _bethanysPieShopDbContext.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (categoryToDelete == null)
            {
                throw new ArgumentException("The category to delete can't be found.");
            }

            var piesInCategory = await _bethanysPieShopDbContext.Pies
                .AnyAsync(p => p.CategoryId == id);

            if (piesInCategory)
            {
                throw new Exception(
                    "Pies exist in this category. Delete all pies in this category before deleting the category.");
            }

            _bethanysPieShopDbContext.Categories.Remove(categoryToDelete);
            int result = await _bethanysPieShopDbContext.SaveChangesAsync();

            _memoryCache.Remove(AllCategoriesCacheName);

            return result;
        }

        public async Task<int> UpdateCategoryNamesAsync(List<Category> categories)
        {
            foreach (var category in categories)
            {
                var categoryToUpdate = await _bethanysPieShopDbContext.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);

                if (categoryToUpdate != null)
                {
                    categoryToUpdate.Name = category.Name;
                }
            }

            int result = await _bethanysPieShopDbContext.SaveChangesAsync();

            _memoryCache.Remove(AllCategoriesCacheName);

            return result;
        }
    }
}
