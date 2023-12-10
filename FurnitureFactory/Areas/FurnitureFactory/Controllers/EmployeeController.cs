using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureFactory.Areas.FurnitureFactory.Controllers;

[Area("FurnitureFactory")]
[Authorize(Roles = "Admin")]
public class EmployeeController : Controller
{
    private readonly AcmeDataContext _context;
    private readonly IMemoryCache _cache;

    public EmployeeController(AcmeDataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public IActionResult Index()
    {
        return View(GetEmployees());
    }

    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var employee = GetEmployees()
            .FirstOrDefault(m => m.EmployeeId == id);
        if (employee == null) return NotFound();

        return View(employee);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

    public IActionResult Edit(int? id)
    {
        if (id == null) return NotFound();

        var employee = GetEmployees().FirstOrDefault(e => e.EmployeeId == id);
        if (employee == null) return NotFound();
        return View(employee);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
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

    public IActionResult Delete(int? id)
    {
        if (id == null) return NotFound();
        var employee = GetEmployees().FirstOrDefault(m => m.EmployeeId == id);
        return employee == null ? NotFound() : View(employee);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
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