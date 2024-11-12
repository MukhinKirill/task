using Microsoft.Extensions.Logging;
using Task.Connector.Data;
using Task.Connector.Models;

namespace Task.Connector.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public void AddUser(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        
        _logger.LogInformation("Начинаем добавление пользователя {UserName}", user.Name);
        
        if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email))
        {
            _logger.LogError("Некорректные данные пользователя {UserName}", user.Name);
            throw new InvalidOperationException("Имя и Email пользователя не могут быть пустыми.");
        }
        
        _userRepository.AddUser(user);

        _logger.LogInformation("Пользователь {UserName} успешно добавлен", user.Name);
    }
}