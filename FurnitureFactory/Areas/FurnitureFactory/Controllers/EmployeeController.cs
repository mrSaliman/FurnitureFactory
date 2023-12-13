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
public class EmployeeController : Controller
{
    private const int PageSize = 8;
    private readonly AcmeDataContext _context;
    private readonly IMemoryCache _cache;

    public EmployeeController(AcmeDataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Index(int page = 1)
    {
        var employee = HttpContext.Session.Get<EmployeeViewModel>("Employee") ?? new EmployeeViewModel();
        var employees = GetEmployees();
        employees = Sort_Search(employees, employee.LastName ?? "",
            employee.FirstName ?? "", employee.MiddleName,
            employee.Position ?? "", employee.Education ?? "");
        // Разбиение на страницы
        var customersPage = employees.ToList();
        var count = customersPage.Count;
        employees = customersPage.Skip((page - 1) * PageSize).Take(PageSize);


        var customerViewModel = new EmployeeViewModel
        {
            Employees = employees,
            PageViewModel = new PageViewModel(count, page, PageSize),
            LastName = employee.LastName,
            FirstName = employee.FirstName,
            MiddleName = employee.MiddleName,
            Position = employee.Position,
            Education = employee.Education
        };
        return View(customerViewModel);
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Index(EmployeeViewModel employee)
    {
        HttpContext.Session.Set("Employee", employee);

        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var employee = GetEmployees()
            .FirstOrDefault(m => m.EmployeeId == id);
        if (employee == null) return NotFound();

        return View(employee);
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
        [Bind("EmployeeId,LastName,FirstName,MiddleName,Position,Education")]
        Employee employee)
    {
        if (!ModelState.IsValid) return View(employee);
        
        _context.Add(employee);
        await _context.SaveChangesAsync();
        SetEmployees();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,SuperUser")]
    public IActionResult Edit(int? id)
    {
        if (id == null) return NotFound();

        var employee = GetEmployees().FirstOrDefault(e => e.EmployeeId == id);
        if (employee == null) return NotFound();
        return View(employee);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> Edit(int id,
        [Bind("EmployeeId,LastName,FirstName,MiddleName,Position,Education")]
        Employee employee)
    {
        if (id != employee.EmployeeId) return NotFound();

        if (!ModelState.IsValid) return View(employee);
        try
        {
            _context.Update(employee);
            await _context.SaveChangesAsync();
            SetEmployees();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EmployeeExists(employee.EmployeeId)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,SuperUser")]
    public IActionResult Delete(int? id)
    {
        if (id == null) return NotFound();
        var employee = GetEmployees().FirstOrDefault(m => m.EmployeeId == id);
        return employee == null ? NotFound() : View(employee);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var employee = GetEmployees().FirstOrDefault(e => e.EmployeeId == id);
        if (employee != null) _context.Employees.Remove(employee);

        await _context.SaveChangesAsync();
        SetEmployees();
        return RedirectToAction(nameof(Index));
    }

    private bool EmployeeExists(int id)
    {
        return GetEmployees().Any(e => e.EmployeeId == id);
    }
    
    private static IEnumerable<Employee> Sort_Search(IEnumerable<Employee> employees, string lastName, string firstName,
        string middleName, string position, string education)
    {
        employees = employees.Where(e => e.LastName.Contains(lastName ?? "", StringComparison.OrdinalIgnoreCase)
                                         && e.FirstName.Contains(firstName ?? "", StringComparison.OrdinalIgnoreCase)
                                         && e.MiddleName.Contains(middleName ?? "", StringComparison.OrdinalIgnoreCase)
                                         && e.Position.Contains(position ?? "", StringComparison.OrdinalIgnoreCase)
                                         && e.Education.Contains(education ?? "", StringComparison.OrdinalIgnoreCase));
        return employees;
    }
    
    public IEnumerable<Employee> GetEmployees()
    {
        _cache.TryGetValue("Employees", out IEnumerable<Employee>? employees);

        return employees ?? SetEmployees();
    }

    public IEnumerable<Employee> SetEmployees()
    {
        var employees = _context.Employees
            .ToList();
        _cache.Set("Employees", employees,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(100000)));
        return employees;
    }
}
