using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Filters;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using FurnitureFactory.Areas.FurnitureFactory.Services;
using FurnitureFactory.Areas.FurnitureFactory.Services.Cache;
using FurnitureFactory.Areas.FurnitureFactory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace FurnitureFactory.Areas.FurnitureFactory.Controllers;

[Area("FurnitureFactory")]
[Authorize(Roles = "Admin,User,SuperUser")]
public class FurnitureController : Controller
{
    private const int PageSize = 8;
    private readonly AcmeDataContext _context;
    private readonly FurnitureCache _cache;

    public FurnitureController(AcmeDataContext context, FurnitureCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Index(string sortField = "", int page = 1)
    {
        var furniture = HttpContext.Session.Get<FurnitureViewModel>("Furniture") ?? new FurnitureViewModel();
        furniture.SortViewModel ??= new SortViewModel("", true);
        var sortOrder = furniture.SortViewModel.GetOrientedSortOrder(sortField);
        
        var furnitures = GetFurnitures();
        furnitures = Sort_Search(furnitures, furniture.FurnitureName, furniture.Description,
            furniture.MaterialType, furniture.Price, furniture.QuantityOnHand, sortOrder);
        
        var furnituresPage = furnitures.ToList();
        var count = furnituresPage.Count;
        furnitures = furnituresPage.Skip((page - 1) * PageSize).Take(PageSize);


        furniture.PageViewModel = new PageViewModel(count, page, PageSize);
        HttpContext.Session.Set("Furniture", furniture);
        furniture.Furnitures = furnitures;
        
        return View(furniture);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Index(FurnitureViewModel furniture)
    {
        HttpContext.Session.Set("Furniture", furniture);

        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var furniture = GetFurnitures()
            .FirstOrDefault(m => m.FurnitureId == id);
        if (furniture == null) return NotFound();

        return View(furniture);
    }

    [Authorize(Roles = "Admin,SuperUser")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> Create(
        [Bind("FurnitureId,FurnitureName,Description,MaterialType,Price,QuantityOnHand")]
        Furniture furniture)
    {
        if (!ModelState.IsValid) return View(furniture);

        _context.Add(furniture);
        await _context.SaveChangesAsync();
        SetFurnitures();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,SuperUser")]
    public IActionResult Edit(int? id)
    {
        if (id == null) return NotFound();
        var furniture = GetFurnitures().FirstOrDefault(f => f.FurnitureId == id);
        if (furniture == null) return NotFound();
        return View(furniture);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> Edit(int id,
        [Bind("FurnitureId,FurnitureName,Description,MaterialType,Price,QuantityOnHand")]
        Furniture furniture)
    {
        if (id != furniture.FurnitureId) return NotFound();
        if (!ModelState.IsValid) return View(furniture);
        try
        {
            _context.Update(furniture);
            await _context.SaveChangesAsync();
            _cache.Update();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!FurnitureExists(furniture.FurnitureId)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,SuperUser")]
    public IActionResult Delete(int? id)
    {
        if (id == null) return NotFound();
        var furniture = GetFurnitures().FirstOrDefault(f => f.FurnitureId == id);
        return furniture == null ? NotFound() : View(furniture);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var furniture = GetFurnitures().FirstOrDefault(f => f.FurnitureId == id);
        if (furniture != null) _context.Furnitures.Remove(furniture);

        await _context.SaveChangesAsync();
        _cache.Update();
        return RedirectToAction(nameof(Index));
    }

    private bool FurnitureExists(int id)
    {
        return GetFurnitures().Any(e => e.FurnitureId == id);
    }

    private static IEnumerable<Furniture> Sort_Search(IEnumerable<Furniture> furnitures, string furnitureName,
        string description,
        string materialType, decimal price, int quantityOnHand, string sortOrder)
    {
        furnitures = furnitures
            .Where(c => c.FurnitureName.Contains(furnitureName ?? "", StringComparison.OrdinalIgnoreCase)
                        && c.Description.Contains(description ?? "", StringComparison.OrdinalIgnoreCase)
                        && c.MaterialType.Contains(materialType ?? "", StringComparison.OrdinalIgnoreCase)
                        && (c.Price == price || price == 0)
                        && (c.QuantityOnHand == quantityOnHand || quantityOnHand == 0));
        
        if (!string.IsNullOrEmpty(sortOrder))
        {
            furnitures = furnitures.AsQueryable().OrderBy(sortOrder);
        }
        return furnitures;
    }

    public IEnumerable<Furniture> GetFurnitures()
    {
        return _cache.Get();
    }

    public IEnumerable<Furniture> SetFurnitures()
    {
        return _cache.Set();
    }
}