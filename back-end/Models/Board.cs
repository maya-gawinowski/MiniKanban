using Microsoft.AspNetCore.Identity;
using System;

namespace Kanban.Models;

public class Board
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string OwnerId { get; set; } = null!;
    public AppUser Owner { get; set; } = null!;
    public ICollection<BoardUser> Members { get; set; } = new List<BoardUser>();
    public ICollection<Column> Columns { get; set; } = new List<Column>();
}
