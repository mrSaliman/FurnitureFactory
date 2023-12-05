using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using FurnitureFactory.Areas.FurnitureFactory.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FurnitureFactory.Areas.FurnitureFactory.Controllers;

[Area("FurnitureFactory")]
[Authorize(Roles = "Admin")]
public class OrderDetailController : Controller
{
    private readonly AcmeDataContext _context;
    private readonly FurnitureFactoryCacheService _cache;

    public OrderDetailController(AcmeDataContext context, FurnitureFactoryCacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public IActionResult Index()
    {
        return View(_cache.GetOrderDetails());
    }

    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var orderDetail = _cache.GetOrderDetails().FirstOrDefault(m => m.OrderDetailId == id);
        return orderDetail == null ? NotFound() : View(orderDetail);
    }

    public IActionResult Create()
    {
        ViewData["FurnitureId"] = new SelectList(_context.Furnitures, "FurnitureId", "FurnitureName");
        ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderDate");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("OrderDetailId,OrderId,FurnitureId,Quantity")]
        OrderDetail orderDetail)
    {
        if (ModelState.IsValid)
        {
            _context.Add(orderDetail);
            await _context.SaveChangesAsync();
            _cache.SetOrderDetails();
            return RedirectToAction(nameof(Index));
        }

        ViewData["FurnitureId"] = 
            new SelectList(_context.Furnitures, "FurnitureId", "FurnitureName", orderDetail.FurnitureId);
        ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderDate", orderDetail.OrderId);
        return View(orderDetail);
    }

    public ActionResult Edit(int? id)
    {
        if (id == null) return NotFound();

        var orderDetail = _cache.GetOrderDetails().FirstOrDefault(od => od.OrderDetailId == id);
        if (orderDetail == null) return NotFound();
        ViewData["FurnitureId"] =
            new SelectList(_context.Furnitures, "FurnitureId", "FurnitureName", orderDetail.FurnitureId);
        ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderDate", orderDetail.OrderId);
        return View(orderDetail);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("OrderDetailId,OrderId,FurnitureId,Quantity")] OrderDetail orderDetail)
    {
        if (id != orderDetail.OrderDetailId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(orderDetail);
                await _context.SaveChangesAsync();
                _cache.SetOrderDetails();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderDetailExists(orderDetail.OrderDetailId)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        ViewData["FurnitureId"] =
            new SelectList(_context.Furnitures, "FurnitureId", "FurnitureName", orderDetail.FurnitureId);
        ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderDate", orderDetail.OrderId);
        return View(orderDetail);
    }

    public IActionResult Delete(int? id)
    {
        if (id == null) return NotFound();

        var orderDetail = _cache.GetOrderDetails()
            .FirstOrDefault(od => od.OrderDetailId == id);
        if (orderDetail == null) return NotFound();

        return View(orderDetail);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var orderDetail = await _context.OrderDetails.FindAsync(id);
        if (orderDetail != null) _context.OrderDetails.Remove(orderDetail);

        await _context.SaveChangesAsync();
        _cache.SetOrderDetails();
        return RedirectToAction(nameof(Index));
    }

    private bool OrderDetailExists(int id)
    {
        return _cache.GetOrderDetails().Any(e => e.OrderDetailId == id);
    }
}