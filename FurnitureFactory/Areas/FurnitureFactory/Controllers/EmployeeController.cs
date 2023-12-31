using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Filters;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using FurnitureFactory.Areas.FurnitureFactory.Services;
using FurnitureFactory.Areas.FurnitureFactory.Services.Cache;
using FurnitureFactory.Areas.FurnitureFactory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Dynamic.Core;

namespace FurnitureFactory.Areas.FurnitureFactory.Controllers;

[Area("FurnitureFactory")]
[Authorize(Roles = "Admin,User,SuperUser")]
public class EmployeeController : Controller
{
    private const int PageSize = 8;
    private readonly AcmeDataContext _context;
    private readonly EmployeeCache _cache;

    public EmployeeController(AcmeDataContext context, EmployeeCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [Authorize(Roles = "Admin,User,SuperUser")]
    public IActionResult Index(string sortField = "", int page = 1)
    {
        var employee = HttpContext.Session.Get<EmployeeViewModel>("Employee") ?? new EmployeeViewModel();
        employee.SortViewModel ??= new SortViewModel("", true);
        var sortOrder = employee.SortViewModel.GetOrientedSortOrder(sortField);
        
        var employees = GetEmployees();
        employees = Sort_Search(employees, employee.LastName ?? "",
            employee.FirstName ?? "", employee.MiddleName ?? "",
            employee.Position ?? "", employee.Education ?? "", sortOrder);
        
        // Разбиение на страницы
        var customersPage = employees.ToList();
        var count = customersPage.Count;
        employees = customersPage.Skip((page - 1) * PageSize).Take(PageSize);

        employee.PageViewModel = new PageViewModel(count, page, PageSize);
        HttpContext.Session.Set("Employee", employee);
        employee.Employees = employees;

        return View(employee);
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
            _cache.Update();
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
        _cache.Update();
        return RedirectToAction(nameof(Index));
    }

    private bool EmployeeExists(int id)
    {
        return GetEmployees().Any(e => e.EmployeeId == id);
    }

    private static IEnumerable<Employee> Sort_Search(IEnumerable<Employee> employees, string lastName, string firstName,
        string middleName, string position, string education, string sortOrder = "")
    {
        employees = employees.Where(e => e.LastName.Contains(lastName ?? "", StringComparison.OrdinalIgnoreCase)
                                         && e.FirstName.Contains(firstName ?? "", StringComparison.OrdinalIgnoreCase)
                                         && e.MiddleName.Contains(middleName ?? "", StringComparison.OrdinalIgnoreCase)
                                         && e.Position.Contains(position ?? "", StringComparison.OrdinalIgnoreCase)
                                         && e.Education.Contains(education ?? "", StringComparison.OrdinalIgnoreCase));
        
        if (!string.IsNullOrEmpty(sortOrder))
        {
            employees = employees.AsQueryable().OrderBy(sortOrder);
        }
        return employees;
    }

    public IEnumerable<Employee> GetEmployees()
    {
        return _cache.Get();
    }

    public IEnumerable<Employee> SetEmployees()
    {
        return _cache.Set();
    }
}