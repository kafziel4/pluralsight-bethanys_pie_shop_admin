using BethanysPieShopAdmin.Models;
using BethanysPieShopAdmin.Models.Repositories;
using BethanysPieShopAdmin.Utilities;
using BethanysPieShopAdmin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BethanysPieShopAdmin.Controllers
{
    public class PieController : Controller
    {
        private readonly int _pageSize = 5;
        private readonly IPieRepository _pieRepository;
        private readonly ICategoryRepository _categoryRepository;

        public PieController(IPieRepository pieRepository, ICategoryRepository categoryRepository)
        {
            _pieRepository = pieRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var pies = await _pieRepository.GetAllPiesAsync();
            return View(pies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var pie = await _pieRepository.GetPieByIdAsync(id);
            return View(pie);
        }

        public async Task<IActionResult> Add()
        {
            try
            {
                IEnumerable<Category>? allCategories = await _categoryRepository.GetAllCategoriesAsync();
                IEnumerable<SelectListItem> selectListItems = new SelectList(
                    allCategories, "CategoryId", "Name", null);

                PieAddViewModel pieAddViewModel = new() { Categories = selectListItems };
                return View(pieAddViewModel);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"There was an error: {ex.Message}";
            }

            return View(new PieAddViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Add(PieAddViewModel pieAddViewModel)
        {
            if (ModelState.IsValid)
            {
                Pie pie = new()
                {
                    CategoryId = pieAddViewModel.Pie.CategoryId,
                    ShortDescription = pieAddViewModel.Pie.ShortDescription,
                    LongDescription = pieAddViewModel.Pie.LongDescription,
                    Price = pieAddViewModel.Pie.Price,
                    AllergyInformation = pieAddViewModel.Pie.AllergyInformation,
                    ImageThumbnailUrl = pieAddViewModel.Pie.ImageThumbnailUrl,
                    ImageUrl = pieAddViewModel.Pie.ImageUrl,
                    InStock = pieAddViewModel.Pie.InStock,
                    IsPieOfTheWeek = pieAddViewModel.Pie.IsPieOfTheWeek,
                    Name = pieAddViewModel.Pie.Name
                };

                await _pieRepository.AddPieAsync(pie);
                return RedirectToAction(nameof(Index));
            }

            var allCategories = await _categoryRepository.GetAllCategoriesAsync();

            IEnumerable<SelectListItem> selectListItems = new SelectList(
                allCategories, "CategoryId", "Name", null);

            pieAddViewModel.Categories = selectListItems;

            return View(pieAddViewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allCategories = await _categoryRepository.GetAllCategoriesAsync();

            IEnumerable<SelectListItem> selectListItems = new SelectList(
                allCategories, "CategoryId", "Name", null);

            var selectedPie = await _pieRepository.GetPieByIdAsync(id.Value);

            PieEditViewModel pieEditViewModel = new() { Categories = selectListItems, Pie = selectedPie };
            return View(pieEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PieEditViewModel pieEditViewModel)
        {
            Pie pieToUpdate = await _pieRepository.GetPieByIdAsync(pieEditViewModel.Pie.PieId);

            try
            {
                if (pieToUpdate == null)
                {
                    ModelState.AddModelError(
                        "", "The pie you want to update doesn't exist or was already deleted by someone else.");
                }

                if (ModelState.IsValid)
                {
                    await _pieRepository.UpdatePieAsync(pieEditViewModel.Pie);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var exceptionPie = ex.Entries.Single();
                var entityValues = (Pie)exceptionPie.Entity;
                var databasePie = exceptionPie.GetDatabaseValues();

                if (databasePie == null)
                {
                    ModelState.AddModelError("", "The pie was already deleted by another user.");
                }
                else
                {
                    var databaseValues = (Pie)databasePie.ToObject();

                    if (databaseValues.Name != entityValues.Name)
                    {
                        ModelState.AddModelError("Pie.Name", $"Current value: {databaseValues.Name}");
                    }
                    if (databaseValues.Price != entityValues.Price)
                    {
                        ModelState.AddModelError("Pie.Price", $"Current value: {databaseValues.Price:c}");
                    }
                    if (databaseValues.ShortDescription != entityValues.ShortDescription)
                    {
                        ModelState.AddModelError("Pie.ShortDescription", $"Current value: {databaseValues.ShortDescription}");
                    }
                    if (databaseValues.LongDescription != entityValues.LongDescription)
                    {
                        ModelState.AddModelError("Pie.LongDescription", $"Current value: {databaseValues.LongDescription}");
                    }
                    if (databaseValues.AllergyInformation != entityValues.AllergyInformation)
                    {
                        ModelState.AddModelError("Pie.AllergyInformation", $"Current value: {databaseValues.AllergyInformation}");
                    }
                    if (databaseValues.ImageThumbnailUrl != entityValues.ImageThumbnailUrl)
                    {
                        ModelState.AddModelError("Pie.ImageThumbnailUrl", $"Current value: {databaseValues.ImageThumbnailUrl}");
                    }
                    if (databaseValues.ImageUrl != entityValues.ImageUrl)
                    {
                        ModelState.AddModelError("Pie.ImageUrl", $"Current value: {databaseValues.ImageUrl}");
                    }
                    if (databaseValues.IsPieOfTheWeek != entityValues.IsPieOfTheWeek)
                    {
                        ModelState.AddModelError("Pie.IsPieOfTheWeek", $"Current value: {databaseValues.IsPieOfTheWeek}");
                    }
                    if (databaseValues.InStock != entityValues.InStock)
                    {
                        ModelState.AddModelError("Pie.InStock", $"Current value: {databaseValues.InStock}");
                    }
                    if (databaseValues.CategoryId != entityValues.CategoryId)
                    {
                        ModelState.AddModelError("Pie.CategoryId", $"Current value: {databaseValues.CategoryId}");
                    }

                    ModelState.AddModelError(
                        "", "The pie was modified already by another user. The database values are now shown. Hit Save again to store these values.");

                    pieToUpdate.RowVersion = databaseValues.RowVersion;

                    ModelState.Remove("Pie.RowVersion");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Updating the category failed, please try again! Error: {ex.Message}");
            }

            var allCategories = await _categoryRepository.GetAllCategoriesAsync();

            IEnumerable<SelectListItem> selectListItems = new SelectList(
                allCategories, "CategoryId", "Name", null);

            pieEditViewModel.Categories = selectListItems;
            pieEditViewModel.Pie = pieToUpdate;
            return View(pieEditViewModel);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var selectedCategory = await _pieRepository.GetPieByIdAsync(id);

            return View(selectedCategory);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int? pieId)
        {
            if (pieId == null)
            {
                ViewData["ErrorMessage"] = "Deleting the pie failed, invalid ID!";
                return View();
            }

            try
            {
                await _pieRepository.DeletePieAsync(pieId.Value);
                TempData["PieDeleted"] = "Pie deleted successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Deleting the pie failed, please try again! Error: {ex.Message}";
            }

            var selectedPie = await _pieRepository.GetPieByIdAsync(pieId.Value);
            return View(selectedPie);
        }

        public async Task<IActionResult> IndexPaging(int? pageNumber)
        {
            pageNumber ??= 1;

            var pies = await _pieRepository.GetPiesPagedAsync(pageNumber, _pageSize);

            var count = await _pieRepository.GetAllPiesCountAsync();

            return View(new PagedList<Pie>(pies, count, pageNumber.Value, _pageSize));
        }

        public async Task<IActionResult> IndexPagingSorting(string sortBy, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortBy;

            ViewData["IdSortParam"] = string.IsNullOrEmpty(sortBy) || sortBy == "id" ? "id_desc" : "id";
            ViewData["NameSortParam"] = sortBy == "name" ? "name_desc" : "name";
            ViewData["PriceSortParam"] = sortBy == "price" ? "price_desc" : "price";

            pageNumber ??= 1;

            var pies = await _pieRepository.GetPiesSortedAndPagedAsync(sortBy, pageNumber, _pageSize);

            var count = await _pieRepository.GetAllPiesCountAsync();

            return View(new PagedList<Pie>(pies, count, pageNumber.Value, _pageSize));
        }

        public async Task<IActionResult> Search(string? searchQuery, int? searchCategory)
        {
            var allCategories = await _categoryRepository.GetAllCategoriesAsync();

            IEnumerable<SelectListItem> selectListItems = new SelectList(
                allCategories, "CategoryId", "Name", null);

            if (searchQuery != null)
            {
                var pies = await _pieRepository.SearchPies(searchQuery, searchCategory);

                return View(new PieSearchViewModel
                {
                    Pies = pies,
                    Categories = selectListItems,
                    SearchQuery = searchQuery,
                    SearchCategory = searchCategory
                });
            }

            return View(new PieSearchViewModel
            {
                Pies = new List<Pie>(),
                Categories = selectListItems,
                SearchQuery = string.Empty,
                SearchCategory = null
            });
        }
    }
}
