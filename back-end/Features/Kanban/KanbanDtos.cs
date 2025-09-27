using Kanban.Models;

namespace Kanban.Features.Kanban;

// READ
public record BoardDto(Guid Id, string Name, string OwnerId);
public record ColumnDto(Guid Id, string Name, int Order);
public record CardDto(Guid Id, string Title, string? Description, int Order);
public record BoardMemberDto(string UserId, string Email, BoardRole Role);

// WRITE
public record CreateBoardRequest(string Name);
public record RenameRequest(string Name);

public record CreateColumnRequest(string Name);
public record ReorderColumnsRequest(List<Guid> ColumnIdsInOrder);

public record CreateCardRequest(string Title, string? Description);
public record ReorderCardsRequest(Guid ColumnId, List<Guid> CardIdsInOrder);

public record UpdateCardRequest(string? Title, string? Description);

public record MoveCardRequest(Guid CardId, Guid FromColumnId, Guid ToColumnId, int ToIndex);

public record AddMemberRequest(string Email, BoardRole Role);
public record UpdateMemberRoleRequest(BoardRole Role);
