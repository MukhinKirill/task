using Microsoft.AspNetCore.Mvc;
using Task.Connector.Models;
using Task.Connector.Services;

namespace Task.Connector.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost]
    public IActionResult AddUser([FromBody] User user)
    {
        if (user == null)
        {
            return BadRequest("Некорректные данные пользователя.");
        }

        try
        {
            _userService.AddUser(user);
            return Ok("Пользователь успешно добавлен.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Ошибка: {ex.Message}");
        }
    }
}