using Microsoft.AspNetCore.Mvc;
using Task.Domain.Users;
using Task.Infrastructure.EntityFrameWork;
using ILogger = Task.Integration.Data.Models.ILogger;

namespace Task.Example.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class UsersController : ControllerBase
{
    private readonly TaskDbContext _context;
    private readonly ILogger _logger;

    public UsersController(TaskDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public void CreateUser()
    {
        var user = new User
        {
            login = "pendos",
            firstName = "Вадим",
            middleName = "Александрович",
            lastName = "Бумагин",
            telephoneNumber = "88005553535"
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        _logger.Debug("Сработало!");
    }
}