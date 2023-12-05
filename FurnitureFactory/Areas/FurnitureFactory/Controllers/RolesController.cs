using FurnitureFactory.Areas.FurnitureFactory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureFactory.Areas.FurnitureFactory.Controllers;

[Area("FurnitureFactory")]
[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    public RolesController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View(_roleManager.Roles.ToList());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name)
    {
        if (string.IsNullOrEmpty(name)) return View(name);
        
        var result = await _roleManager.CreateAsync(new IdentityRole(name));
        if (result.Succeeded)
            return RedirectToAction("Index");
        
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);

        return View(name);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role != null) await _roleManager.DeleteAsync(role);
        return RedirectToAction("Index");
    }

    public IActionResult UserList()
    {
        return View(_userManager.Users.ToList());
    }

    public async Task<IActionResult> Edit(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser.Id == user.Id)
        {
            return Forbid();
        }
        
        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.ToList();
        
        var model = new ChangeRoleViewModel
        {
            UserId = user.Id,
            UserEmail = user.Email,
            UserRoles = userRoles,
            AllRoles = allRoles
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string userId, List<string> roles)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        
        var userRoles = await _userManager.GetRolesAsync(user);
        var addedRoles = roles.Except(userRoles);
        var removedRoles = userRoles.Except(roles);

        await _userManager.AddToRolesAsync(user, addedRoles);

        await _userManager.RemoveFromRolesAsync(user, removedRoles);

        return RedirectToAction("UserList");
    }
}