using Microsoft.AspNetCore.Mvc;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Example.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ExampleController : ControllerBase
{
    private readonly IConnector _connector;

    public ExampleController(IConnector connector)
    {
        _connector = connector;
    }

    [HttpPost]
    public IActionResult CreateUser(UserToCreate request)
    {
        _connector.CreateUser(request);
        return Ok();
    }

    [HttpGet]
    public IActionResult GetAllProperties()
    {
        return Ok(_connector.GetAllProperties());
    }

    [HttpGet]
    public IActionResult GetUserProperties(string userLogin)
    {
        return Ok(_connector.GetUserProperties(userLogin));
    }

    [HttpGet]
    public IActionResult IsUserExists(string userLogin)
    {
        return Ok(_connector.IsUserExists(userLogin));
    }

    [HttpPut]
    public IActionResult UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        _connector.UpdateUserProperties(properties, userLogin);
        return Ok();
    }

    [HttpGet]
    public IActionResult GetAllPermissions()
    {
        return Ok(_connector.GetAllPermissions());
    }

    [HttpPost]
    public IActionResult AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        _connector.AddUserPermissions(userLogin, rightIds);
        return Ok();
    }

    [HttpDelete]
    public IActionResult RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        _connector.RemoveUserPermissions(userLogin, rightIds);
        return Ok();
    }

    [HttpGet]
    public IActionResult GetUserPermissions(string userId)
    {
        return Ok(_connector.GetUserPermissions(userId));
    }
}