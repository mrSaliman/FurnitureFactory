using Microsoft.AspNetCore.Identity;

namespace FurnitureFactory.Areas.FurnitureFactory.ViewModels;

public class ChangeRoleViewModel
{
    public string UserId { get; set; }
    public string UserEmail { get; set; }
    public List<IdentityRole> AllRoles { get; set; } = new();
    public IList<string> UserRoles { get; set; } = new List<string>();
}