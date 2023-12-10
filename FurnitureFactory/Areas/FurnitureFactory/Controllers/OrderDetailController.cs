using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureFactory.Areas.FurnitureFactory.Controllers;

[Area("FurnitureFactory")]
[Authorize(Roles = "Admin")]
public class OrderDetailController : Controller
{
    private readonly AcmeDataContext _context;
    private readonly IMemoryCache _cache;

    public OrderDetailController(AcmeDataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public IActionResult Index()
    {
        return View(GetOrderDetails());
    }

    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var orderDetail = GetOrderDetails().FirstOrDefault(m => m.OrderDetailId == id);
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
            SetOrderDetails();
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

        var orderDetail = GetOrderDetails().FirstOrDefault(od => od.OrderDetailId == id);
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
                SetOrderDetails();
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

        var orderDetail = GetOrderDetails()
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
        SetOrderDetails();
        return RedirectToAction(nameof(Index));
    }

    private bool OrderDetailExists(int id)
    {
        return GetOrderDetails().Any(e => e.OrderDetailId == id);
    }
    
    public IEnumerable<OrderDetail> GetOrderDetails()
    {
        _cache.TryGetValue("OrderDetails", out IEnumerable<OrderDetail>? orders);

        return orders ?? SetOrderDetails();
    }

    public IEnumerable<OrderDetail> SetOrderDetails()
    {
        var orders = _context.OrderDetails.Include(od => od.Furniture).Include(od => od.Order).ToList();
        _cache.Set("OrderDetails", orders,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(100000)));
        return orders;
    }
}