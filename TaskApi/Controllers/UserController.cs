using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Task.Connector;
using System.Collections.Generic;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.Models;

namespace Task.Connector.Controllers
{
    [Route("api/[controller]")]
    public class UserController :Controller
    {
        private readonly IConnector _connectorDb;
        private readonly ILogger<UserController> _logger;

        public UserController(IConnector connectorDb, ILogger<UserController> logger)
        {
            _connectorDb = connectorDb;
            _logger = logger;
        }

        // Endpoint для создания пользователя
        [HttpPost("create")]
        public IActionResult CreateUser([FromBody] UserToCreate user)
        {
            try
            {
                _connectorDb.CreateUser(user);
                return Ok("Пользователь успешно создан.");
            }
            catch(System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании пользователя.");
                return BadRequest("Ошибка при создании пользователя.");
            }
        }

        // Endpoint для проверки существования пользователя
        [HttpGet("exists/{userLogin}")]
        public IActionResult IsUserExists(string userLogin)
        {
            try
            {
                var exists = _connectorDb.IsUserExists(userLogin);
                return Ok(exists);
            }
            catch(System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования пользователя.");
                return BadRequest("Ошибка при проверке существования пользователя.");
            }
        }

        // Endpoint для получения всех свойств пользователей
        [HttpGet("properties")]
        public IActionResult GetAllProperties()
        {
            try
            {
                var properties = _connectorDb.GetAllProperties();
                return Ok(properties);
            }
            catch(System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех свойств.");
                return BadRequest("Ошибка при получении всех свойств.");
            }
        }

        // Endpoint для получения свойств пользователя по логину
        [HttpGet("properties/{userLogin}")]
        public IActionResult GetUserProperties(string userLogin)
        {
            try
            {
                var userProperties = _connectorDb.GetUserProperties(userLogin);
                return Ok(userProperties);
            }
            catch(System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении свойств пользователя.");
                return BadRequest("Ошибка при получении свойств пользователя.");
            }
        }

        // Endpoint для обновления свойств пользователя
        [HttpPut("properties/{userLogin}")]
        public IActionResult UpdateUserProperties(string userLogin, [FromBody] IEnumerable<UserProperty> properties)
        {
            try
            {
                _connectorDb.UpdateUserProperties(properties, userLogin);
                return Ok("Свойства пользователя успешно обновлены.");
            }
            catch(System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении свойств пользователя.");
                return BadRequest("Ошибка при обновлении свойств пользователя.");
            }
        }

        // Endpoint для получения всех прав пользователей
        [HttpGet("permissions")]
        public IActionResult GetAllPermissions()
        {
            try
            {
                var permissions = _connectorDb.GetAllPermissions();
                return Ok(permissions);
            }
            catch(System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех прав.");
                return BadRequest("Ошибка при получении всех прав.");
            }
        }

        // Endpoint для добавления прав пользователю
        [HttpPost("permissions/{userLogin}")]
        public IActionResult AddUserPermissions(string userLogin, [FromBody] IEnumerable<string> rightIds)
        {
            try
            {
                _connectorDb.AddUserPermissions(userLogin, rightIds);
                return Ok("Права успешно добавлены пользователю.");
            }
            catch(System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении прав пользователю.");
                return BadRequest("Ошибка при добавлении прав пользователю.");
            }
        }

        // Endpoint для удаления прав пользователя
        [HttpDelete("permissions/{userLogin}")]
        public IActionResult RemoveUserPermissions(string userLogin, [FromBody] IEnumerable<string> rightIds)
        {
            try
            {
                _connectorDb.RemoveUserPermissions(userLogin, rightIds);
                return Ok("Права успешно удалены у пользователя.");
            }
            catch(System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении прав у пользователя.");
                return BadRequest("Ошибка при удалении прав у пользователя.");
            }
        }

        // Endpoint для получения прав пользователя
        [HttpGet("permissions/{userLogin}")]
        public IActionResult GetUserPermissions(string userLogin)
        {
            try
            {
                var userPermissions = _connectorDb.GetUserPermissions(userLogin);
                return Ok(userPermissions);
            }
            catch(System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении прав пользователя.");
                return BadRequest("Ошибка при получении прав пользователя.");
            }
        }
    }
}
