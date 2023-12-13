using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Filters;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using FurnitureFactory.Areas.FurnitureFactory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureFactory.Areas.FurnitureFactory.Controllers;

[Area("FurnitureFactory")]
[Authorize(Roles = "Admin,User,SuperUser")]
public class FurnitureController : Controller
{
    private const int PageSize = 8;
    private readonly AcmeDataContext _context;
    private readonly IMemoryCache _cache;

    public FurnitureController(AcmeDataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Index(int page = 1)
    {
        var furniture = HttpContext.Session.Get<FurnitureViewModel>("Furniture") ?? new FurnitureViewModel();
        var furnitures = GetFurnitures();
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
            SetFurnitures();
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
        SetFurnitures();
        return RedirectToAction(nameof(Index));
    }

    private bool FurnitureExists(int id)
    {
        return GetFurnitures().Any(e => e.FurnitureId == id);
    }

    private static IEnumerable<Furniture> Sort_Search(IEnumerable<Furniture> furnitures, string furnitureName,
        string description,
        string materialType, decimal price, int quantityOnHand)
    {
        furnitures = furnitures
            .Where(c => c.FurnitureName.Contains(furnitureName ?? "", StringComparison.OrdinalIgnoreCase)
                        && c.Description.Contains(description ?? "", StringComparison.OrdinalIgnoreCase)
                        && c.MaterialType.Contains(materialType ?? "", StringComparison.OrdinalIgnoreCase)
                        && (c.Price == price || price == 0)
                        && (c.QuantityOnHand == quantityOnHand || quantityOnHand == 0));
        return furnitures;
    }
    
    public IEnumerable<Furniture> GetFurnitures()
    {
        _cache.TryGetValue("Furnitures", out IEnumerable<Furniture>? furnitures);

        return furnitures ?? SetFurnitures();
    }

    public IEnumerable<Furniture> SetFurnitures()
    {
        var furnitures = _context.Furnitures
            .ToList();
        _cache.Set("Furnitures", furnitures,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(100000)));
        return furnitures;
    }
}