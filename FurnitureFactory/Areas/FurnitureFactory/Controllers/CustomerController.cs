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
public class CustomerController : Controller
{
    private const int PageSize = 8;
    private readonly AcmeDataContext _context;
    private readonly IMemoryCache _cache;
    
    public CustomerController(AcmeDataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Index(int page = 1)
    {
        var customer = HttpContext.Session.Get<CustomerViewModel>("Customer") ?? new CustomerViewModel();
        var customers = GetCustomers();
        customers = Sort_Search(customers, customer.CompanyName ?? "",
            customer.RepresentativeLastName ?? "", customer.RepresentativeFirstName,
            customer.RepresentativeMiddleName ?? "", customer.PhoneNumber ?? "", customer.Address ?? "");
        // Разбиение на страницы
        var customersPage = customers.ToList();
        var count = customersPage.Count;
        customers = customersPage.Skip((page - 1) * PageSize).Take(PageSize);


        var customerViewModel = new CustomerViewModel
        {
            Customers = customers,
            PageViewModel = new PageViewModel(count, page, PageSize),
            CompanyName = customer.CompanyName,
            RepresentativeLastName = customer.RepresentativeLastName,
            RepresentativeFirstName = customer.RepresentativeFirstName,
            RepresentativeMiddleName = customer.RepresentativeMiddleName,
            PhoneNumber = customer.PhoneNumber,
            Address = customer.Address
        };
        return View(customerViewModel);
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
            SetCustomers();
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
        SetCustomers();
        return RedirectToAction(nameof(Index));
    }

    private bool CustomerExists(int id)
    {
        return GetCustomers().Any(e => e.CustomerId == id);
    }

    private static IEnumerable<Customer> Sort_Search(IEnumerable<Customer> customers, string companyName,
        string representativeLastName, string representativeFirstName,
        string representativeMiddleName, string phoneNumber, string address)
    {
        customers = customers.Where(c => c.CompanyName.Contains(companyName ?? "", StringComparison.OrdinalIgnoreCase)
                                         && c.RepresentativeLastName.Contains(representativeLastName ?? "", StringComparison.OrdinalIgnoreCase)
                                         && c.RepresentativeFirstName.Contains(representativeFirstName ?? "", StringComparison.OrdinalIgnoreCase)
                                         && c.RepresentativeMiddleName.Contains(representativeMiddleName ?? "", StringComparison.OrdinalIgnoreCase)
                                         && c.PhoneNumber.Contains(phoneNumber ?? "", StringComparison.OrdinalIgnoreCase)
                                         && c.Address.Contains(address ?? "", StringComparison.OrdinalIgnoreCase));
        return customers;
    }
    
    public IEnumerable<Customer> GetCustomers()
    {
        _cache.TryGetValue("Customers", out IEnumerable<Customer>? customers);

        return customers ?? SetCustomers();
    }

    public IEnumerable<Customer> SetCustomers()
    {
        var customers = _context.Customers
            .ToList();
        _cache.Set("Customers", customers,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(100000)));
        return customers;
    }
}