using Microsoft.AspNetCore.Identity;
using System;

namespace Kanban.Models;

public class Card
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ColumnId { get; set; }
    public Column Column { get; set; } = null!;
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int Order { get; set; }
}
