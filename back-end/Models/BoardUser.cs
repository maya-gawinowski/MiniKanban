using Microsoft.AspNetCore.Identity;
using System;

namespace Kanban.Models;

public enum BoardRole {Owner, Editor, Viewer}
public class BoardUser
{
    public Guid BoardId { get; set; }
    public Board Board { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public AppUser User { get; set; } = null!;
    public BoardRole Role { get; set; } = BoardRole.Editor;
}
