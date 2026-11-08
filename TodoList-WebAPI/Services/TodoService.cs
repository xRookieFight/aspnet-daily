using System.Collections.Concurrent;
using TodoList_WebAPI.Models;

namespace TodoList_WebAPI.Services;

public class TodoService
{
    private readonly ConcurrentDictionary<int, TodoItem> _todos = new();
    private int _lastId;

    public Task<IReadOnlyList<TodoItem>> GetAllTodosAsync()
    {
        IReadOnlyList<TodoItem> todos = _todos.Values
            .OrderBy(t => t.Id)
            .Select(Clone)
            .ToList();
        return Task.FromResult(todos);
    }

    public Task<TodoItem?> GetTodoByIdAsync(int id)
    {
        return Task.FromResult(_todos.TryGetValue(id, out var todo) ? Clone(todo) : null);
    }

    public Task<TodoItem> CreateTodoAsync(TodoItem todoItem)
    {
        // ok so the client might send an id, we always hand out our own
        var created = Clone(todoItem);
        created.Id = Interlocked.Increment(ref _lastId);
        created.CreatedAt = DateTime.UtcNow;

        _todos[created.Id] = created;
        return Task.FromResult(Clone(created));
    }

    public Task<TodoItem?> UpdateTodoAsync(TodoItem todoItem)
    {
        while (_todos.TryGetValue(todoItem.Id, out var existing))
        {
            var updated = Clone(todoItem);
            updated.CreatedAt = existing.CreatedAt;

            // hmm, retry if someone else updated it in between, otherwise we'd clobber a delete
            if (_todos.TryUpdate(todoItem.Id, updated, existing))
            {
                return Task.FromResult<TodoItem?>(Clone(updated));
            }
        }

        return Task.FromResult<TodoItem?>(null);
    }

    public Task<bool> DeleteTodoAsync(int id)
    {
        return Task.FromResult(_todos.TryRemove(id, out _));
    }

    private static TodoItem Clone(TodoItem source) => new()
    {
        Id = source.Id,
        Title = source.Title,
        Description = source.Description,
        IsCompleted = source.IsCompleted,
        CreatedAt = source.CreatedAt
    };
}
