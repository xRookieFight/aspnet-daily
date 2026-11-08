using Microsoft.AspNetCore.Mvc;
using TodoList_WebAPI.Models;
using TodoList_WebAPI.Services;

namespace TodoList_WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly TodoService _todoService;

    public TodoController(TodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTodos()
    {
        var todos = await _todoService.GetAllTodosAsync();
        return Ok(todos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTodoById(int id)
    {
        var todo = await _todoService.GetTodoByIdAsync(id);
        if (todo == null)
        {
            return NotFound();
        }
        return Ok(todo);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTodo([FromBody] TodoItem todoItem)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdTodo = await _todoService.CreateTodoAsync(todoItem);
        return CreatedAtAction(nameof(GetTodoById), new { id = createdTodo.Id }, createdTodo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodo(int id, [FromBody] TodoItem todoItem)
    {
        // an idiot admires complexity, a genius admires simplicity, a physicist tries to make it simple... - Terry Davis
        if (id != todoItem.Id)
        {
            return BadRequest("ID mismatch");
        }

        var updatedTodo = await _todoService.UpdateTodoAsync(todoItem);
        if (updatedTodo == null)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var deleted = await _todoService.DeleteTodoAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}