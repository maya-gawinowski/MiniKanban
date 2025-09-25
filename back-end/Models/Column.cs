using Microsoft.AspNetCore.Identity;
using System;

namespace Kanban.Models;

public class Column
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BoardId { get; set; }
    public Board Board { get; set; } = null!;
    public string Name { get; set; } = "";
    public int Order { get; set; }
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
