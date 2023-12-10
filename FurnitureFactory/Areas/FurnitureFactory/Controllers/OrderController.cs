using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Filters;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using FurnitureFactory.Areas.FurnitureFactory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureFactory.Areas.FurnitureFactory.Controllers;

[Area("FurnitureFactory")]
[Authorize(Roles = "Admin")]
public class OrderController : Controller
{
    private const int PageSize = 8;
    private readonly AcmeDataContext _context;
    private readonly IMemoryCache _cache;


    public OrderController(AcmeDataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public IActionResult Index(int page = 1)
    {
        var order = HttpContext.Session.Get<OrderViewModel>("Order") ?? new OrderViewModel();
        var orders = GetOrders();
        orders = Sort_Search(orders, order.OrderDate, order.SpecialDiscount, order.IsCompleted,
            order.ResponsibleEmployeeFirstName, order.CustomerCompanyName);
        // Разбиение на страницы
        var ordersPage = orders.ToList();
        var count = ordersPage.Count;
        orders = ordersPage.Skip((page - 1) * PageSize).Take(PageSize);


        var orderViewModel = new OrderViewModel
        {
            Orders = orders,
            PageViewModel = new PageViewModel(count, page, PageSize),
            OrderDate = order.OrderDate,
            SpecialDiscount = order.SpecialDiscount,
            IsCompleted = order.IsCompleted,
            ResponsibleEmployeeFirstName = order.ResponsibleEmployeeFirstName,
            CustomerCompanyName = order.CustomerCompanyName
        };
        return View(orderViewModel);
    }

    [HttpPost]
    public IActionResult Index(OrderViewModel order)
    {
        HttpContext.Session.Set("Order", order);

        return RedirectToAction("Index");
    }

    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var order = GetOrders()
            .FirstOrDefault(m => m.OrderId == id);
        if (order == null) return NotFound();

        return View(order);
    }

    public IActionResult Create()
    {
        ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CompanyName");
        ViewData["ResponsibleEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("OrderId,OrderDate,CustomerId,SpecialDiscount,IsCompleted,ResponsibleEmployeeId")]
        Order order)
    {
        if (ModelState.IsValid)
        {
            _context.Add(order);
            await _context.SaveChangesAsync();
            SetOrders();
            return RedirectToAction(nameof(Index));
        }

        ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CompanyName", order.CustomerId);
        ViewData["ResponsibleEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstName",
            order.ResponsibleEmployeeId);
        return View(order);
    }

    public IActionResult Edit(int? id)
    {
        if (id == null) return NotFound();

        var order = GetOrders().FirstOrDefault(o => o.OrderId == id);
        if (order == null) return NotFound();
        ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CompanyName", order.CustomerId);
        ViewData["ResponsibleEmployeeId"] =
            new SelectList(_context.Employees, "EmployeeId", "FirstName", order.ResponsibleEmployeeId);
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("OrderId,OrderDate,CustomerId,SpecialDiscount,IsCompleted,ResponsibleEmployeeId")]
        Order order)
    {
        if (id != order.OrderId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                SetOrders();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.OrderId)) return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CompanyName", order.CustomerId);
        ViewData["ResponsibleEmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstName",
            order.ResponsibleEmployeeId);
        return View(order);
    }

    public IActionResult Delete(int? id)
    {
        if (id == null) return NotFound();
        var order = GetOrders().FirstOrDefault(m => m.OrderId == id);
        return order == null ? NotFound() : View(order);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var order = GetOrders().FirstOrDefault(o => o.OrderId == id);
        if (order != null) _context.Orders.Remove(order);

        await _context.SaveChangesAsync();
        SetOrders();
        return RedirectToAction(nameof(Index));
    }

    private bool OrderExists(int id)
    {
        return GetOrders().Any(e => e.OrderId == id);
    }

    private static IEnumerable<Order> Sort_Search(IEnumerable<Order> orders, DateTime? orderDate, decimal specialDiscount,
        bool isCompleted, string responsibleEmployeeFirstName, string customerCompanyName)
    {
        orders = orders.Where(c => (c.OrderDate.Date == orderDate || orderDate == new DateTime() || orderDate == null)
                                   && (c.SpecialDiscount == specialDiscount || specialDiscount == 0)
                                   && c.ResponsibleEmployee.FirstName.Contains(responsibleEmployeeFirstName ?? "")
                                   && c.IsCompleted == isCompleted
                                   && c.Customer.CompanyName.Contains(customerCompanyName ?? ""));
        return orders;
    }
    
    public IEnumerable<Order> GetOrders()
    {
        _cache.TryGetValue("Orders", out IEnumerable<Order>? orders);

        return orders ?? SetOrders();
    }

    public IEnumerable<Order> SetOrders()
    {
        var orders = _context.Orders.Include(o => o.ResponsibleEmployee).Include(o => o.Customer);
        var result = orders.ToList();
        _cache.Set("Orders", result,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(100000)));
        return result;
    }
}