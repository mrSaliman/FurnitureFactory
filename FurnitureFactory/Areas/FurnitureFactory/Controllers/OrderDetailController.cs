using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Filters;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using FurnitureFactory.Areas.FurnitureFactory.Services;
using FurnitureFactory.Areas.FurnitureFactory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureFactory.Areas.FurnitureFactory.Controllers;

[Area("FurnitureFactory")]
[Authorize(Roles = "Admin,User,SuperUser")]
public class OrderDetailController : Controller
{
    private const int PageSize = 8;
    private readonly AcmeDataContext _context;
    private readonly OrderDetailCache _cache;

    public OrderDetailController(AcmeDataContext context, OrderDetailCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Index(int page = 1)
    {
        var orderDetail = HttpContext.Session.Get<OrderDetailViewModel>("OrderDetail") ?? new OrderDetailViewModel();
        var orderDetails = GetOrderDetails();
        orderDetails = Sort_Search(orderDetails, orderDetail.OrderDate, orderDetail.FurnitureName, orderDetail.Quantity);
        // Разбиение на страницы
        var ordersPage = orderDetails.ToList();
        var count = ordersPage.Count;
        orderDetails = ordersPage.Skip((page - 1) * PageSize).Take(PageSize);


        var orderDetailViewModel = new OrderDetailViewModel
        {
            OrderDetails = orderDetails,
            PageViewModel = new PageViewModel(count, page, PageSize),
            OrderDate = orderDetail.OrderDate,
            FurnitureName = orderDetail.FurnitureName,
            Quantity = orderDetail.Quantity
        };
        return View(orderDetailViewModel);
    }

    private static IEnumerable<OrderDetail> Sort_Search(IEnumerable<OrderDetail> orderDetails,
        DateTime? orderDate, string furnitureName, int quantity)
    {
        orderDetails = orderDetails.Where(od =>
            (od.Order.OrderDate.Date == orderDate || orderDate == new DateTime() || orderDate == null)
            && od.Furniture.FurnitureName.Contains(furnitureName ?? "", StringComparison.OrdinalIgnoreCase)
            && (od.Quantity == quantity || quantity == 0));

        return orderDetails;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Index(OrderDetailViewModel orderDetail)
    {
        HttpContext.Session.Set("OrderDetail", orderDetail);

        return RedirectToAction("Index");
    }
    
    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var orderDetail = GetOrderDetails().FirstOrDefault(m => m.OrderDetailId == id);
        return orderDetail == null ? NotFound() : View(orderDetail);
    }

    [Authorize(Roles = "Admin,SuperUser")]
    public IActionResult Create()
    {
        ViewData["FurnitureId"] = new SelectList(_context.Furnitures, "FurnitureId", "FurnitureName");
        ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderDate");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,SuperUser")]
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

    [Authorize(Roles = "Admin,SuperUser")]
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
    [Authorize(Roles = "Admin,SuperUser")]
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
                _cache.Update();
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

    [Authorize(Roles = "Admin,SuperUser")]
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
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var orderDetail = await _context.OrderDetails.FindAsync(id);
        if (orderDetail != null) _context.OrderDetails.Remove(orderDetail);

        await _context.SaveChangesAsync();
        _cache.Update();
        return RedirectToAction(nameof(Index));
    }

    private bool OrderDetailExists(int id)
    {
        return GetOrderDetails().Any(e => e.OrderDetailId == id);
    }
    
    public IEnumerable<OrderDetail> GetOrderDetails()
    {
        return _cache.Get();
    }

    public IEnumerable<OrderDetail> SetOrderDetails()
    {
        return _cache.Set();
    }
}