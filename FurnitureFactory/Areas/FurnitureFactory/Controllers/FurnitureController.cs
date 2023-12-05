using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Filters;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using FurnitureFactory.Areas.FurnitureFactory.Services;
using FurnitureFactory.Areas.FurnitureFactory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureFactory.Areas.FurnitureFactory.Controllers;

[Area("FurnitureFactory")]
[Authorize(Roles = "Admin")]
public class FurnitureController : Controller
{
    private const int PageSize = 8;
    private readonly AcmeDataContext _context;
    private readonly FurnitureFactoryCacheService _cache;

    public FurnitureController(AcmeDataContext context, FurnitureFactoryCacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public IActionResult Index(int page = 1)
    {
        var furniture = HttpContext.Session.Get<FurnitureViewModel>("Furniture") ?? new FurnitureViewModel();
        var furnitures = _cache.GetFurnitures();
        furnitures = Sort_Search(furnitures, furniture.FurnitureName, furniture.Description,
            furniture.MaterialType, furniture.Price, furniture.QuantityOnHand);
        var furnituresPage = furnitures.ToList();
        var count = furnituresPage.Count;
        furnitures = furnituresPage.Skip((page - 1) * PageSize).Take(PageSize);


        var furnitureViewModel = new FurnitureViewModel
        {
            Furnitures = furnitures,
            PageViewModel = new PageViewModel(count, page, PageSize),
            FurnitureName = furniture.FurnitureName,
            Description = furniture.Description,
            MaterialType = furniture.MaterialType,
            Price = furniture.Price,
            QuantityOnHand = furniture.QuantityOnHand
        };
        return View(furnitureViewModel);
    }

    [HttpPost]
    public IActionResult Index(FurnitureViewModel furniture)
    {
        HttpContext.Session.Set("Furniture", furniture);

        return RedirectToAction("Index");
    }


    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var furniture = _cache.GetFurnitures()
            .FirstOrDefault(m => m.FurnitureId == id);
        if (furniture == null) return NotFound();

        return View(furniture);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("FurnitureId,FurnitureName,Description,MaterialType,Price,QuantityOnHand")]
        Furniture furniture)
    {
        if (!ModelState.IsValid) return View(furniture);
        
        _context.Add(furniture);
        await _context.SaveChangesAsync();
        _cache.SetFurnitures();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int? id)
    {
        if (id == null) return NotFound();
        var furniture = _cache.GetFurnitures().FirstOrDefault(f => f.FurnitureId == id);
        if (furniture == null) return NotFound();
        return View(furniture);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
            _cache.SetFurnitures();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!FurnitureExists(furniture.FurnitureId)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int? id)
    {
        if (id == null) return NotFound();
        var furniture = _cache.GetFurnitures().FirstOrDefault(f => f.FurnitureId == id);
        return furniture == null ? NotFound() : View(furniture);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var furniture = _cache.GetFurnitures().FirstOrDefault(f => f.FurnitureId == id);
        if (furniture != null) _context.Furnitures.Remove(furniture);

        await _context.SaveChangesAsync();
        _cache.SetFurnitures();
        return RedirectToAction(nameof(Index));
    }

    private bool FurnitureExists(int id)
    {
        return _cache.GetFurnitures().Any(e => e.FurnitureId == id);
    }

    private static IEnumerable<Furniture> Sort_Search(IEnumerable<Furniture> furnitures, string furnitureName,
        string description,
        string materialType, decimal price, int quantityOnHand)
    {
        furnitures = furnitures
            .Where(c => c.FurnitureName.Contains(furnitureName ?? "")
                        && c.Description.Contains(description ?? "")
                        && c.MaterialType.Contains(materialType ?? "")
                        && (c.Price == price || price == 0)
                        && (c.QuantityOnHand == quantityOnHand || quantityOnHand == 0));
        return furnitures;
    }
}