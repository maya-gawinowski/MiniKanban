using Microsoft.AspNetCore.Identity;
using System;

namespace Kanban.Models;

public class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public DateTime CreatedAtActionResult { get; set; } = DateTime.UtcNow;
}
