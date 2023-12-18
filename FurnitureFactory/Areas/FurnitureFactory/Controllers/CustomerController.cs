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
public class CustomerController : Controller
{
    private const int PageSize = 8;
    private readonly AcmeDataContext _context;
    private readonly CustomerCache _cache;

    public CustomerController(AcmeDataContext context, CustomerCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Index(string sortField = "", int page = 1)
    {
        var customer = HttpContext.Session.Get<CustomerViewModel>("Customer") ?? new CustomerViewModel();
        customer.SortViewModel ??= new SortViewModel("", true);
        var sortOrder = customer.SortViewModel.GetOrientedSortOrder(sortField);
        
        var customers = GetCustomers();
        customers = Sort_Search(customers, customer.CompanyName ?? "",
            customer.RepresentativeLastName ?? "", customer.RepresentativeFirstName ?? "",
            customer.RepresentativeMiddleName ?? "", customer.PhoneNumber ?? "", customer.Address ?? "",
            sortOrder);
        
        // Разбиение на страницы
        var customersPage = customers.ToList();
        var count = customersPage.Count;
        customers = customersPage.Skip((page - 1) * PageSize).Take(PageSize);

        customer.PageViewModel = new PageViewModel(count, page, PageSize);
        HttpContext.Session.Set("Customer", customer);
        customer.Customers = customers;
        
        return View(customer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Index(CustomerViewModel customer)
    {
        HttpContext.Session.Set("Customer", customer);

        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var customer = GetCustomers().FirstOrDefault(m => m.CustomerId == id);
        if (customer == null) return NotFound();

        return View(customer);
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
        [Bind(
            "CustomerId,CompanyName,RepresentativeLastName,RepresentativeFirstName,RepresentativeMiddleName,PhoneNumber,Address")]
        Customer customer)
    {
        if (!ModelState.IsValid) return View(customer);

        _context.Add(customer);
        await _context.SaveChangesAsync();
        SetCustomers();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,SuperUser")]
    public IActionResult Edit(int? id)
    {
        if (id == null) return NotFound();

        var customer = GetCustomers().FirstOrDefault(c => c.CustomerId == id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> Edit(int id,
        [Bind(
            "CustomerId,CompanyName,RepresentativeLastName,RepresentativeFirstName,RepresentativeMiddleName,PhoneNumber,Address")]
        Customer customer)
    {
        if (id != customer.CustomerId) return NotFound();

        if (!ModelState.IsValid) return View(customer);
        try
        {
            _context.Update(customer);
            await _context.SaveChangesAsync();
            _cache.Update();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CustomerExists(customer.CustomerId)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,SuperUser")]
    public IActionResult Delete(int? id)
    {
        if (id == null) return NotFound();
        var customer = GetCustomers().FirstOrDefault(c => c.CustomerId == id);
        return customer == null ? NotFound() : View(customer);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = GetCustomers().FirstOrDefault(c => c.CustomerId == id);
        if (customer != null) _context.Customers.Remove(customer);

        await _context.SaveChangesAsync();
        _cache.Update();
        return RedirectToAction(nameof(Index));
    }

    private bool CustomerExists(int id)
    {
        return GetCustomers().Any(e => e.CustomerId == id);
    }

    private static IEnumerable<Customer> Sort_Search(IEnumerable<Customer> customers, string companyName,
        string representativeLastName, string representativeFirstName,
        string representativeMiddleName, string phoneNumber, string address, string sortOrder = "")
    {
        customers = customers.Where(c => c.CompanyName.Contains(companyName ?? "", StringComparison.OrdinalIgnoreCase)
                                         && c.RepresentativeLastName.Contains(representativeLastName ?? "",
                                             StringComparison.OrdinalIgnoreCase)
                                         && c.RepresentativeFirstName.Contains(representativeFirstName ?? "",
                                             StringComparison.OrdinalIgnoreCase)
                                         && c.RepresentativeMiddleName.Contains(representativeMiddleName ?? "",
                                             StringComparison.OrdinalIgnoreCase)
                                         && c.PhoneNumber.Contains(phoneNumber ?? "",
                                             StringComparison.OrdinalIgnoreCase)
                                         && c.Address.Contains(address ?? "", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(sortOrder))
        {
            customers = customers.AsQueryable().OrderBy(sortOrder);
        }
        return customers;
    }

    public IEnumerable<Customer> GetCustomers()
    {
        return _cache.Get();
    }

    public IEnumerable<Customer> SetCustomers()
    {
        return _cache.Set();
    }
}