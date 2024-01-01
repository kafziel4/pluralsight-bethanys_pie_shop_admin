using Microsoft.EntityFrameworkCore;

namespace BethanysPieShopAdmin.Models.Repositories
{
    public class PieRepository : IPieRepository
    {
        private readonly BethanysPieShopDbContext _bethanysPieShopDbContext;

        public PieRepository(BethanysPieShopDbContext bethanysPieShopDbContext)
        {
            _bethanysPieShopDbContext = bethanysPieShopDbContext;
        }

        public async Task<IEnumerable<Pie>> GetAllPiesAsync()
        {
            return await _bethanysPieShopDbContext.Pies
                .AsNoTracking()
                .OrderBy(p => p.PieId)
                .ToListAsync();
        }

        public async Task<Pie?> GetPieByIdAsync(int pieId)
        {
            return await _bethanysPieShopDbContext.Pies
                .AsNoTracking()
                .Include(p => p.Ingredients)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.PieId == pieId);
        }

        public async Task<int> AddPieAsync(Pie pie)
        {
            _bethanysPieShopDbContext.Pies.Add(pie);
            return await _bethanysPieShopDbContext.SaveChangesAsync();
        }

        public async Task<int> UpdatePieAsync(Pie pie)
        {
            var pieToUpdate = await _bethanysPieShopDbContext.Pies
                .FirstOrDefaultAsync(p => p.PieId == pie.PieId);

            if (pieToUpdate == null)
            {
                throw new ArgumentException("The pie to update can't be found.");
            }

            _bethanysPieShopDbContext.Entry(pieToUpdate).Property("RowVersion").OriginalValue = pie.RowVersion;

            pieToUpdate.CategoryId = pie.CategoryId;
            pieToUpdate.ShortDescription = pie.ShortDescription;
            pieToUpdate.LongDescription = pie.LongDescription;
            pieToUpdate.Price = pie.Price;
            pieToUpdate.AllergyInformation = pie.AllergyInformation;
            pieToUpdate.ImageThumbnailUrl = pie.ImageThumbnailUrl;
            pieToUpdate.ImageUrl = pie.ImageUrl;
            pieToUpdate.InStock = pie.InStock;
            pieToUpdate.IsPieOfTheWeek = pie.IsPieOfTheWeek;
            pieToUpdate.Name = pie.Name;

            return await _bethanysPieShopDbContext.SaveChangesAsync();
        }

        public async Task<int> DeletePieAsync(int id)
        {
            var pieToDelete = await _bethanysPieShopDbContext.Pies.FirstOrDefaultAsync(c => c.PieId == id);

            if (pieToDelete == null)
            {
                throw new ArgumentException($"The pie to delete can't be found.");
            }

            _bethanysPieShopDbContext.Pies.Remove(pieToDelete);
            return await _bethanysPieShopDbContext.SaveChangesAsync();
        }

        public async Task<int> GetAllPiesCountAsync()
        {
            var count = await _bethanysPieShopDbContext.Pies.CountAsync();
            return count;
        }

        public async Task<IEnumerable<Pie>> GetPiesPagedAsync(int? pageNumber, int pageSize)
        {
            IQueryable<Pie> pies = _bethanysPieShopDbContext.Pies;

            pageNumber ??= 1;

            pies = pies.Skip((pageNumber.Value - 1) * pageSize).Take(pageSize);

            return await pies.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Pie>> GetPiesSortedAndPagedAsync(string sortBy, int? pageNumber, int pageSize)
        {
            IQueryable<Pie> pies = _bethanysPieShopDbContext.Pies;

            pies = sortBy switch
            {
                "name_desc" => pies.OrderByDescending(p => p.Name),
                "name" => pies.OrderBy(p => p.Name),
                "id_desc" => pies.OrderByDescending(p => p.PieId),
                "id" => pies.OrderBy(p => p.PieId),
                "price_desc" => pies.OrderByDescending(p => p.Price),
                "price" => pies.OrderBy(p => p.Price),
                _ => pies.OrderBy(p => p.PieId)
            };

            pageNumber ??= 1;

            pies = pies.Skip((pageNumber.Value - 1) * pageSize).Take(pageSize);

            return await pies.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Pie>> SearchPies(string searchQuery, int? categoryId)
        {
            IQueryable<Pie> pies = _bethanysPieShopDbContext.Pies;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                pies = pies.Where(p =>
                    p.Name.Contains(searchQuery) ||
                    p.ShortDescription.Contains(searchQuery) ||
                    p.LongDescription.Contains(searchQuery));
            }

            if (categoryId != null)
            {
                pies = pies.Where(p => p.CategoryId == categoryId);
            }

            return await pies.AsNoTracking().ToListAsync();
        }
    }
}
