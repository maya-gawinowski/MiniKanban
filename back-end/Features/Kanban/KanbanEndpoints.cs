using System.Security.Claims;
using Kanban.Data;
using Kanban.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Features.Kanban;

public static class KanbanEndpoints
{
    public static RouteGroupBuilder MapKanban(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/kanban")
                   .RequireAuthorization(); // all routes require a valid JWT

        // --------------------------
        // Helpers
        // --------------------------
        static string CurrentUserId(HttpContext ctx) =>
            ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        static Task<bool> IsMember(ApplicationDbContext db, Guid boardId, string userId) =>
            db.BoardUsers.AnyAsync(mu => mu.BoardId == boardId && mu.UserId == userId);

        static Task<bool> IsOwner(ApplicationDbContext db, Guid boardId, string userId) =>
            db.Boards.AnyAsync(b => b.Id == boardId && b.OwnerId == userId);

        static async Task<bool> CanEdit(ApplicationDbContext db, Guid boardId, string userId)
        {
            var role = await db.BoardUsers
                .Where(mu => mu.BoardId == boardId && mu.UserId == userId)
                .Select(mu => mu.Role)
                .FirstOrDefaultAsync();

            return role is BoardRole.Owner or BoardRole.Editor;
        }

        // --------------------------
        // Boards
        // --------------------------

        // GET /api/kanban/boards  → boards where current user is a member
        g.MapGet("/boards", async (ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);

            var boards = await db.BoardUsers
                .Where(mu => mu.UserId == uid)
                .Select(mu => new BoardDto(mu.Board.Id, mu.Board.Name, mu.Board.OwnerId))
                .ToListAsync();

            return Results.Ok(boards);
        });

        // POST /api/kanban/boards  → create board; creator becomes Owner + Member
        g.MapPost("/boards", async (CreateBoardRequest req, ApplicationDbContext db, HttpContext ctx) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { message = "Name is required" });

            var uid = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);

            await using var tx = await db.Database.BeginTransactionAsync();

            var board = new Board { Name = req.Name.Trim(), OwnerId = uid! };
            db.Boards.Add(board);
            db.BoardUsers.Add(new BoardUser { Board = board, UserId = uid!, Role = BoardRole.Owner });

            db.Columns.AddRange(
                new Column { Board = board, Name = "todo",  Order = 0 },
                new Column { Board = board, Name = "doing", Order = 1 },
                new Column { Board = board, Name = "done",  Order = 2 }
            );

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return Results.Ok(new BoardDto(board.Id, board.Name, board.OwnerId));
        });

        // PUT /api/kanban/boards/{boardId}/rename  (Owner only)
        g.MapPut("/boards/{boardId:guid}/rename", async (Guid boardId, RenameRequest req, ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);
            if (!await IsOwner(db, boardId, uid)) return Results.Forbid();

            var board = await db.Boards.FirstOrDefaultAsync(b => b.Id == boardId);
            if (board is null) return Results.NotFound();

            board.Name = string.IsNullOrWhiteSpace(req.Name) ? board.Name : req.Name.Trim();
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // DELETE /api/kanban/boards/{boardId}  (Owner only)
        g.MapDelete("/boards/{boardId:guid}", async (Guid boardId, ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);
            if (!await IsOwner(db, boardId, uid)) return Results.Forbid();

            var board = await db.Boards.FirstOrDefaultAsync(b => b.Id == boardId);
            if (board is null) return Results.NotFound();

            db.Boards.Remove(board);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // --------------------------
        // Members (collaboration)
        // --------------------------

        // GET /api/kanban/boards/{boardId}/members  (any member can read)
        g.MapGet("/boards/{boardId:guid}/members", async (Guid boardId, ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);
            if (!await IsMember(db, boardId, uid)) return Results.NotFound();

            var members = await db.BoardUsers
                .Where(mu => mu.BoardId == boardId)
                .Select(mu => new BoardMemberDto(mu.UserId, mu.User.Email!, mu.Role))
                .ToListAsync();

            return Results.Ok(members);
        });

        // POST /api/kanban/boards/{boardId}/members  (Owner only)
        g.MapPost("/boards/{boardId:guid}/members", async (Guid boardId, AddMemberRequest req,
            ApplicationDbContext db, UserManager<AppUser> userManager, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);
            if (!await IsOwner(db, boardId, uid)) return Results.Forbid();

            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null) return Results.BadRequest(new { message = "User not found" });

            var exists = await db.BoardUsers.AnyAsync(mu => mu.BoardId == boardId && mu.UserId == user.Id);
            if (exists) return Results.Conflict(new { message = "Already a member" });

            db.BoardUsers.Add(new BoardUser { BoardId = boardId, UserId = user.Id, Role = req.Role });
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // PUT /api/kanban/boards/{boardId}/members/{userId}  (Owner only)
        g.MapPut("/boards/{boardId:guid}/members/{userId}", async (Guid boardId, string userId,
            UpdateMemberRoleRequest req, ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);
            if (!await IsOwner(db, boardId, uid)) return Results.Forbid();

            var mu = await db.BoardUsers.FirstOrDefaultAsync(x => x.BoardId == boardId && x.UserId == userId);
            if (mu is null) return Results.NotFound();

            mu.Role = req.Role;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // DELETE /api/kanban/boards/{boardId}/members/{userId}  (Owner only)
        g.MapDelete("/boards/{boardId:guid}/members/{userId}", async (Guid boardId, string userId,
            ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);
            if (!await IsOwner(db, boardId, uid)) return Results.Forbid();

            var mu = await db.BoardUsers.FirstOrDefaultAsync(x => x.BoardId == boardId && x.UserId == userId);
            if (mu is null) return Results.NotFound();

            db.BoardUsers.Remove(mu);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // --------------------------
        // Columns
        // --------------------------

        // GET /api/kanban/boards/{boardId}/columns (any member)
        g.MapGet("/boards/{boardId:guid}/columns", async (Guid boardId, ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);
            if (!await IsMember(db, boardId, uid)) return Results.NotFound();

            var cols = await db.Columns
                .Where(c => c.BoardId == boardId)
                .OrderBy(c => c.Order)
                .Select(c => new ColumnDto(c.Id, c.Name, c.Order))
                .ToListAsync();

            return Results.Ok(cols);
        });

        // POST /api/kanban/boards/{boardId}/columns (editors/owners)
        g.MapPost("/boards/{boardId:guid}/columns", async (Guid boardId, CreateColumnRequest req, ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);
            if (!await CanEdit(db, boardId, uid)) return Results.Forbid();

            var nextOrder = await db.Columns.Where(c => c.BoardId == boardId).MaxAsync(c => (int?)c.Order) ?? -1;
            var col = new Column { BoardId = boardId, Name = req.Name.Trim(), Order = nextOrder + 1 };
            db.Columns.Add(col);
            await db.SaveChangesAsync();

            return Results.Ok(new ColumnDto(col.Id, col.Name, col.Order));
        });

        // POST /api/kanban/boards/{boardId}/columns/reorder (editors/owners)
        g.MapPost("/boards/{boardId:guid}/columns/reorder", async (Guid boardId, ReorderColumnsRequest req, ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);
            if (!await CanEdit(db, boardId, uid)) return Results.Forbid();

            var cols = await db.Columns.Where(c => c.BoardId == boardId).ToListAsync();
            var order = 0;
            foreach (var id in req.ColumnIdsInOrder)
            {
                var c = cols.FirstOrDefault(x => x.Id == id);
                if (c != null) c.Order = order++;
            }
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // --------------------------
        // Cards
        // --------------------------

        // GET /api/kanban/columns/{columnId}/cards (any member)
        g.MapGet("/columns/{columnId:guid}/cards", async (Guid columnId, ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);

            var col = await db.Columns.Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == columnId);
            if (col is null || !await IsMember(db, col.BoardId, uid)) return Results.NotFound();

            var cards = await db.Cards
                .Where(x => x.ColumnId == columnId)
                .OrderBy(x => x.Order)
                .Select(x => new CardDto(x.Id, x.Title, x.Description, x.Order))
                .ToListAsync();

            return Results.Ok(cards);
        });

        // POST /api/kanban/columns/{columnId}/cards
        g.MapPost("/columns/{columnId:guid}/cards", async (Guid columnId, CreateCardRequest req,
            ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);

            var col = await db.Columns.Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == columnId);
            if (col is null || !await CanEdit(db, col.BoardId, uid)) return Results.Forbid();

            var nextOrder = await db.Cards.Where(c => c.ColumnId == columnId).MaxAsync(c => (int?)c.Order) ?? -1;
            var card = new Card
            {
                ColumnId = columnId,
                Title = string.IsNullOrWhiteSpace(req.Title) ? "Untitled" : req.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description!.Trim(),
                Order = nextOrder + 1
            };

            db.Cards.Add(card);
            await db.SaveChangesAsync();

            return Results.Ok(new CardDto(card.Id, card.Title, card.Description, card.Order));
        });


        // POST /api/kanban/cards/reorder (editors/owners) — same column
        g.MapPost("/cards/reorder", async (ReorderCardsRequest req, ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);

            var col = await db.Columns.Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == req.ColumnId);
            if (col is null || !await CanEdit(db, col.BoardId, uid)) return Results.Forbid();

            var cards = await db.Cards.Where(c => c.ColumnId == req.ColumnId).ToListAsync();
            for (int i = 0; i < req.CardIdsInOrder.Count; i++)
            {
                var card = cards.FirstOrDefault(x => x.Id == req.CardIdsInOrder[i]);
                if (card != null) card.Order = i;
            }
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // Endpoint
        g.MapPost("/cards/move", async (MoveCardRequest req, ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);

            // Load the card and its current column/board
            var card = await db.Cards
                .Include(c => c.Column)
                .ThenInclude(col => col.Board)
                .FirstOrDefaultAsync(c => c.Id == req.CardId);
            if (card is null) return Results.NotFound(new { message = "Card not found" });

            if (card.ColumnId != req.FromColumnId)
                return Results.BadRequest(new { message = "Card not in source column" });

            // Auth on source board
            if (!await CanEdit(db, card.Column.BoardId, uid)) return Results.Forbid();

            // Load target column (+ board) and auth
            var toCol = await db.Columns.Include(c => c.Board).FirstOrDefaultAsync(c => c.Id == req.ToColumnId);
            if (toCol is null) return Results.NotFound(new { message = "Target column not found" });
            if (!await CanEdit(db, toCol.BoardId, uid)) return Results.Forbid();

            await using var tx = await db.Database.BeginTransactionAsync();

            if (req.FromColumnId == req.ToColumnId)
            {
                // SAME COLUMN: compact and insert
                var list = await db.Cards
                    .Where(c => c.ColumnId == req.FromColumnId)
                    .OrderBy(c => c.Order)
                    .ToListAsync();

                list.RemoveAll(c => c.Id == req.CardId);
                var insertAt = Math.Clamp(req.ToIndex, 0, list.Count);
                list.Insert(insertAt, card);

                for (int i = 0; i < list.Count; i++)
                    list[i].Order = i;
            }
            else
            {
                // SOURCE: close gap
                var source = await db.Cards
                    .Where(c => c.ColumnId == req.FromColumnId)
                    .OrderBy(c => c.Order)
                    .ToListAsync();

                source.RemoveAll(c => c.Id == req.CardId);
                for (int i = 0; i < source.Count; i++)
                    source[i].Order = i;

                // TARGET: insert at index
                var target = await db.Cards
                    .Where(c => c.ColumnId == req.ToColumnId)
                    .OrderBy(c => c.Order)
                    .ToListAsync();

                var insertAt = Math.Clamp(req.ToIndex, 0, target.Count);
                target.Insert(insertAt, card);

                for (int i = 0; i < target.Count; i++)
                {
                    target[i].ColumnId = req.ToColumnId;
                    target[i].Order = i;
                }
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return Results.NoContent();
        });

        // PUT /api/kanban/cards/{cardId}  (Editors/Owners)
        g.MapPut("/cards/{cardId:guid}", async (Guid cardId, UpdateCardRequest req,
            ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);

            var card = await db.Cards
                .Include(c => c.Column)
                .ThenInclude(col => col.Board)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card is null) return Results.NotFound();
            if (!await CanEdit(db, card.Column.BoardId, uid)) return Results.Forbid();

            if (!string.IsNullOrWhiteSpace(req.Title))
                card.Title = req.Title.Trim();

            // optional if you support editing description
            if (req.Description is not null)
                card.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();

            await db.SaveChangesAsync();
            return Results.NoContent();
        });


        // DELETE /api/kanban/cards/{cardId}  (Owner only)
        g.MapDelete("/cards/{cardId:guid}", async (Guid cardId, ApplicationDbContext db, HttpContext ctx) =>
        {
            var uid = CurrentUserId(ctx);

            // Load the card with its Column and Board for auth
            var card = await db.Cards
                .Include(c => c.Column)
                .ThenInclude(col => col.Board)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card is null) return Results.NotFound();

            if (!await CanEdit(db, card.Column.BoardId, uid)) return Results.Forbid();

            var columnId = card.ColumnId;

            // Delete
            db.Cards.Remove(card);
            await db.SaveChangesAsync();

            // Re-compact orders in the column
            var remaining = await db.Cards
                .Where(c => c.ColumnId == columnId)
                .OrderBy(c => c.Order)
                .ToListAsync();

            for (int i = 0; i < remaining.Count; i++)
                remaining[i].Order = i;

            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return g;
    }
}
