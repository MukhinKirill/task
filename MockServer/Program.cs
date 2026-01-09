using System;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();



// Хранилище данных
var mockData = new Dictionary<string, object>
{
    ["users"] = new List<dynamic>
    {
        new { login = "Login7", status = "Unlock", lastName = "LastName7", firstName = "FirstName7", middleName = "middleName7", telephoneNumber = "TelephoneNumber7", isLead = false },
        new { login = "Login8", status = "Lock", lastName = "LastName8", firstName = "FirstName8", middleName = "middleName8", telephoneNumber = "TelephoneNumber8", isLead = false },
        new { login = "Login3", status = "Unlock", lastName = "LastName3", firstName = "FirstName3", middleName = "middleName3", telephoneNumber = "TelephoneNumber3", isLead = true },
        new { login = "Login1", status = "Unlock", lastName = "LastName1", firstName = "FirstName1", middleName = "middleName1", telephoneNumber = "TelephoneNumber1", isLead = true }
    },
    ["roles"] = new[]
    {
        new { id = 5, name = "ITRole5", corporatePhoneNumber = "000" },
        new { id = 9, name = "ITRole9", corporatePhoneNumber = "100" },
        new { id = 10, name = "ITRole10", corporatePhoneNumber = "200" },
        new { id = 11, name = "ITRole11", corporatePhoneNumber = "300" }
    },
    ["rights"] = new[]
    {
        new { id = 5, name = "RequestRight5", corporatePhoneNumber = "" },
        new { id = 6, name = "RequestRight6", corporatePhoneNumber = "" },
        new { id = 7, name = "RequestRight7", corporatePhoneNumber = "" }
    },
    // НОВОЕ: Связи пользователь-роли (логин -> список ID ролей)
    ["userRoles"] = new Dictionary<string, List<int>>
    {
        ["Login3"] = new List<int> { 11 }
    },
    // НОВОЕ: Связи пользователь-права (логин -> список ID прав)
    ["userRights"] = new Dictionary<string, List<int>>
    {
        ["Login3"] = new List<int> { 6, 7 }
    }
};

// 1. Эндпоинт аутентификации (как в спецификации)
app.MapPost("/api/v1/login", () =>
{
    return Results.Ok(new
    {
        data = new { access_token = "fake-jwt-token-for-testing", expires_in = 3600 },
        success = true,
        errorText = (string?)null,
        count = 1
    });
});


app.MapGet("/api/v1/users/all", () => Results.Ok(new
{
    data = mockData["users"],
    success = true,
    errorText = (string?)null,
    count = ((List<dynamic>)mockData["users"]).Count
}));


app.MapGet("/api/v1/users/{login}", (string login) =>
{
    var users = (IEnumerable<dynamic>)mockData["users"];
    var user = users.FirstOrDefault(u => u.login == login);

    if (user == null)
        return Results.NotFound();

    return Results.Ok(new
    {
        data = new
        {
            lastName = user.lastName,
            firstName = user.firstName,
            middleName = "",
            telephoneNumber = user.telephoneNumber,
            isLead = false,
            login = user.login,
            status = user.status
        },
        success = true,
        errorText = (string?)null,
        count = 1
    });
});

app.MapGet("/api/v1/roles/all", () => Results.Ok(new
{
    data = mockData["roles"],
    success = true,
    errorText = (string?)null,
    count = ((Array)mockData["roles"]).Length
}));


app.MapGet("/api/v1/rights/all", () => Results.Ok(new
{
    data = mockData["rights"],
    success = true,
    errorText = (string?)null,
    count = ((Array)mockData["rights"]).Length
}));

app.MapGet("/api/v1/users/{login}/roles", (string login) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] GET /api/v1/users/{login}/roles");

    try
    {
        // 1. Проверяем существование пользователя
        var usersList = (List<dynamic>)mockData["users"];
        var user = usersList.FirstOrDefault(u => u.login == login);

        if (user == null)
        {
            // Формат ошибки как в спецификации API
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} не найден"
            });
        }

        // 2. Проверяем статус пользователя
        if (user.status == "Lock")
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} заблокирован"
            });
        }

        // 3. Получаем роли пользователя из связей
        var userRoles = (Dictionary<string, List<int>>)mockData["userRoles"];
        var allRoles = (dynamic[])mockData["roles"];

        var userRolesList = new List<dynamic>();

        if (userRoles.ContainsKey(login))
        {
            foreach (var roleId in userRoles[login])
            {
                var role = allRoles.FirstOrDefault(r => (int)r.id == roleId);
                if (role != null)
                {
                    userRolesList.Add(new
                    {
                        id = (int)role.id,
                        name = (string)role.name,
                        corporatePhoneNumber = (string)role.corporatePhoneNumber
                    });
                }
            }
        }

        // 4. Возвращаем ответ в формате UserRoleResponse (как в Connector1.cs)
        return Results.Ok(new
        {
            data = userRolesList,
            success = true,
            errorText = (string?)null,
            count = userRolesList.Count
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Ошибка: {ex.Message}");
        return Results.Ok(new
        {
            success = false,
            errorText = $"Внутренняя ошибка сервера: {ex.Message}"
        });
    }
});

app.MapGet("/api/v1/users/{login}/rights", (string login) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] GET /api/v1/users/{login}/rights");

    try
    {
        // 1. Проверяем существование пользователя
        var usersList = (List<dynamic>)mockData["users"];
        var user = usersList.FirstOrDefault(u => u.login == login);

        if (user == null)
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} не найден"
            });
        }

        // 2. Проверяем статус пользователя
        if (user.status == "Lock")
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} заблокирован"
            });
        }

        // 3. Получаем права пользователя из связей
        var userRights = (Dictionary<string, List<int>>)mockData["userRights"];
        var allRights = (dynamic[])mockData["rights"];

        var userRightsList = new List<dynamic>();

        if (userRights.ContainsKey(login))
        {
            foreach (var rightId in userRights[login])
            {
                var right = allRights.FirstOrDefault(r => (int)r.id == rightId);
                if (right != null)
                {
                    userRightsList.Add(new
                    {
                        id = (int)right.id,
                        name = (string)right.name,
                        // В коннекторе используется UserrightResponse с полем "users"
                        users = (object?)null
                    });
                }
            }
        }

        // 4. Возвращаем ответ в формате UserrightResponse (как в Connector1.cs)
        return Results.Ok(new
        {
            data = userRightsList,
            success = true,
            errorText = (string?)null,
            count = userRightsList.Count
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Ошибка: {ex.Message}");
        return Results.Ok(new
        {
            success = false,
            errorText = $"Внутренняя ошибка сервера: {ex.Message}"
        });
    }
});


app.MapPost("/api/v1/users/create", async (HttpRequest request) =>
{
    try
    {
        // Читаем тело запроса
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();

        Console.WriteLine($"[CREATE USER] Получен запрос: {body}");

        // Парсим JSON (минимальная валидация)
        var userData = JsonSerializer.Deserialize<UserRequest>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (userData == null)
        {
            return Results.BadRequest(new { success = false, errorText = "Неверный формат запроса" });
        }

        if (string.IsNullOrEmpty(userData.login))
        {
            return Results.BadRequest(new { success = false, errorText = "Логин обязателен" });
        }

        if (string.IsNullOrWhiteSpace(userData.password))
        {
            return Results.BadRequest(new { success = false, errorText = "Логин обязателен" });
        }

        // Проверяем, не существует ли уже пользователь с таким логином
        var users = (List<dynamic>)mockData["users"];
        if (users.Any(u => u.login == userData.login))
        {
            return Results.Ok(new { success = false, errorText = $"Пользователь с логином {userData.login} уже существует" });
        }

        // Добавляем нового пользователя в наше хранилище
        var newUser = new
        {
            login = userData.login,
            status = "Unlock", // По умолчанию разблокирован
            lastName = userData.lastName ?? "",
            firstName = userData.firstName ?? "",
            middleName = userData.middleName ?? "",
            telephoneNumber = userData.telephoneNumber ?? "",
            isLead = userData.isLead
        };

        users.Add(newUser);

        Console.WriteLine($"[CREATE USER] Создан пользователь: {userData.login}");

        // Возвращаем успешный ответ в формате API
        return Results.Ok(new
        {
            success = true,
            errorText = (string?)null,
            count = 1,
            data = new { message = $"Пользователь {userData.login} создан" }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[CREATE USER ERROR] {ex.Message}");
        return Results.StatusCode(500);
    }
});


app.MapPut("/api/v1/users/edit", async (HttpRequest request) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PUT /api/v1/users/edit вызван");

    try
    {
        // 1. Читаем тело запроса
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        Console.WriteLine($"   Тело запроса: {body}");

        if (string.IsNullOrEmpty(body))
        {
            return Results.Ok(new { success = false, errorText = "Пустое тело запроса" });
        }

        // 2. Парсим JSON с обновленными данными пользователя
        var userData = JsonSerializer.Deserialize<UserRequest>(
            body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (userData == null)
        {
            return Results.Ok(new { success = false, errorText = "Неверный формат JSON" });
        }

        // 3. Проверяем наличие логина
        if (string.IsNullOrWhiteSpace(userData.login))
        {
            return Results.Ok(new { success = false, errorText = "Логин обязателен" });
        }

        // 4. Ищем пользователя в хранилище
        var usersList = (List<dynamic>)mockData["users"];
        var existingUser = usersList.FirstOrDefault(u => u.login == userData.login);

        if (existingUser == null)
        {
            Console.WriteLine($"   Ошибка: пользователь {userData.login} не найден");
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь с логином '{userData.login}' не найден"
            });
        }

        // 5. Проверяем, не заблокирован ли пользователь
        if (existingUser.status == "Lock")
        {
            Console.WriteLine($"   Ошибка: пользователь {userData.login} заблокирован");
            return Results.Ok(new
            {
                success = false,
                errorText = $"Невозможно обновить свойства, пользователь {userData.login} залочен"
            });
        }

        Console.WriteLine($"   Найден пользователь: {userData.login}, статус: {existingUser.status}");

        // 6. Создаем обновленную запись пользователя
        // Сохраняем неизменяемые поля из существующего пользователя
        var updatedUser = new
        {
            login = userData.login,
            status = existingUser.status, // Статус остается прежним
            lastName = userData.lastName ?? existingUser.lastName,
            firstName = userData.firstName ?? existingUser.firstName,
            middleName = userData.middleName ?? existingUser.middleName ?? "",
            telephoneNumber = userData.telephoneNumber ?? existingUser.telephoneNumber ?? "",
            isLead = userData.isLead
        };

        // 7. Заменяем старую запись на обновленную
        var index = usersList.IndexOf(existingUser);
        usersList[index] = updatedUser;

        Console.WriteLine($"   Успех: обновлен пользователь {userData.login}");
        Console.WriteLine($"   Новые данные: {JsonSerializer.Serialize(updatedUser)}");

        // 8. Возвращаем успешный ответ
        return Results.Ok(new
        {
            success = true,
            errorText = (string?)null,
            count = 1,
            data = new { message = $"Пользователь {userData.login} обновлен" }
        });
    }
    catch (JsonException jsonEx)
    {
        Console.WriteLine($"   Ошибка парсинга JSON: {jsonEx.Message}");
        return Results.Ok(new { success = false, errorText = "Неверный формат JSON в запросе" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Неожиданная ошибка: {ex.Message}");
        return Results.Ok(new { success = false, errorText = "Внутренняя ошибка сервера" });
    }
});


app.MapPut("/api/v1/users/{login}/add/role/{roleId}", (string login, string roleId) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PUT /api/v1/users/{login}/add/role/{roleId}");

    try
    {
        // 1. Проверяем существование пользователя
        var usersList = (List<dynamic>)mockData["users"];
        var user = usersList.FirstOrDefault(u => u.login == login);

        if (user == null)
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} не найден"
            });
        }

        // 2. Проверяем, что пользователь не заблокирован
        if (user.status == "Lock")
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} заблокирован"
            });
        }

        // 3. Проверяем существование роли
        var roles = (dynamic[])mockData["roles"];
        if (!int.TryParse(roleId, out int roleIdInt))
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Неверный формат ID роли: {roleId}"
            });
        }

        var roleExists = roles.Any(r => r.id == roleIdInt);
        if (!roleExists)
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Роль {roleId} не найдена"
            });
        }

        // 4. Добавляем роль пользователю
        var userRoles = (Dictionary<string, List<int>>)mockData["userRoles"];

        if (!userRoles.ContainsKey(login))
        {
            userRoles[login] = new List<int>();
        }

        if (!userRoles[login].Contains(roleIdInt))
        {
            userRoles[login].Add(roleIdInt);
            Console.WriteLine($"   Роль {roleId} добавлена пользователю {login}");
        }
        else
        {
            Console.WriteLine($"   Роль {roleId} уже была у пользователя {login}");
        }

        // 5. Возвращаем успешный ответ
        return Results.Ok(new
        {
            success = true,
            errorText = (string?)null,
            count = 1,
            data = new { message = $"Роль {roleId} добавлена пользователю {login}" }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Ошибка: {ex.Message}");
        return Results.Ok(new
        {
            success = false,
            errorText = $"Внутренняя ошибка сервера: {ex.Message}"
        });
    }
});



app.MapPut("/api/v1/users/{login}/add/right/{rightId}", (string login, string rightId) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PUT /api/v1/users/{login}/add/right/{rightId}");

    try
    {
        var usersList = (List<dynamic>)mockData["users"];
        var user = usersList.FirstOrDefault(u => u.login == login);

        if (user == null)
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} не найден"
            });
        }

        if (user.status == "Lock")
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} заблокирован"
            });
        }

        var rights = (dynamic[])mockData["rights"];
        if (!int.TryParse(rightId, out int rightIdInt))
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Неверный формат ID права: {rightId}"
            });
        }

        var rightExists = rights.Any(r => r.id == rightIdInt);
        if (!rightExists)
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Право {rightId} не найдено"
            });
        }

        var userRights = (Dictionary<string, List<int>>)mockData["userRights"];

        if (!userRights.ContainsKey(login))
        {
            userRights[login] = new List<int>();
        }

        if (!userRights[login].Contains(rightIdInt))
        {
            userRights[login].Add(rightIdInt);
            Console.WriteLine($"   Право {rightId} добавлено пользователю {login}");
        }
        else
        {
            Console.WriteLine($"   Право {rightId} уже было у пользователя {login}");
        }

        return Results.Ok(new
        {
            success = true,
            errorText = (string?)null,
            count = 1,
            data = new { message = $"Право {rightId} добавлено пользователю {login}" }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Ошибка: {ex.Message}");
        return Results.Ok(new
        {
            success = false,
            errorText = $"Внутренняя ошибка сервера: {ex.Message}"
        });
    }
});



app.MapDelete("/api/v1/users/{login}/drop/role/{roleId}", (string login, string roleId) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] DELETE /api/v1/users/{login}/drop/role/{roleId}");

    try
    {
        var usersList = (List<dynamic>)mockData["users"];
        var user = usersList.FirstOrDefault(u => u.login == login);

        if (user == null)
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} не найден"
            });
        }

        if (user.status == "Lock")
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} заблокирован"
            });
        }

        if (!int.TryParse(roleId, out int roleIdInt))
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Неверный формат ID роли: {roleId}"
            });
        }

        var userRoles = (Dictionary<string, List<int>>)mockData["userRoles"];

        bool removed = false;
        if (userRoles.ContainsKey(login) && userRoles[login].Contains(roleIdInt))
        {
            userRoles[login].Remove(roleIdInt);
            removed = true;
            Console.WriteLine($"   Роль {roleId} удалена у пользователя {login}");

            // Если список стал пустым, удаляем запись
            if (userRoles[login].Count == 0)
            {
                userRoles.Remove(login);
            }
        }
        else
        {
            Console.WriteLine($"   У пользователя {login} не было роли {roleId}");
        }

        return Results.Ok(new
        {
            success = true,
            errorText = (string?)null,
            count = removed ? 1 : 0,
            data = new { message = $"Роль {roleId} удалена у пользователя {login}" }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Ошибка: {ex.Message}");
        return Results.Ok(new
        {
            success = false,
            errorText = $"Внутренняя ошибка сервера: {ex.Message}"
        });
    }
});


app.MapDelete("/api/v1/users/{login}/drop/right/{rightId}", (string login, string rightId) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] DELETE /api/v1/users/{login}/drop/right/{rightId}");

    try
    {
        var usersList = (List<dynamic>)mockData["users"];
        var user = usersList.FirstOrDefault(u => u.login == login);

        if (user == null)
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} не найден"
            });
        }

        if (user.status == "Lock")
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} заблокирован"
            });
        }

        if (!int.TryParse(rightId, out int rightIdInt))
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Неверный формат ID права: {rightId}"
            });
        }

        var userRights = (Dictionary<string, List<int>>)mockData["userRights"];

        bool removed = false;
        if (userRights.ContainsKey(login) && userRights[login].Contains(rightIdInt))
        {
            userRights[login].Remove(rightIdInt);
            removed = true;
            Console.WriteLine($"   Право {rightId} удалено у пользователя {login}");

            if (userRights[login].Count == 0)
            {
                userRights.Remove(login);
            }
        }
        else
        {
            Console.WriteLine($"   У пользователя {login} не было права {rightId}");
        }

        return Results.Ok(new
        {
            success = true,
            errorText = (string?)null,
            count = removed ? 1 : 0,
            data = new { message = $"Право {rightId} удалено у пользователя {login}" }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Ошибка: {ex.Message}");
        return Results.Ok(new
        {
            success = false,
            errorText = $"Внутренняя ошибка сервера: {ex.Message}"
        });
    }
});


app.MapPut("/api/v1/users/{login}/lock", (string login) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PUT /api/v1/users/{login}/lock");

    try
    {
        // 1. Находим пользователя
        var usersList = (List<dynamic>)mockData["users"];
        var userIndex = usersList.FindIndex(u => u.login == login);

        if (userIndex == -1)
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} не найден"
            });
        }

        var existingUser = usersList[userIndex];

        // 2. Проверяем текущий статус
        if (existingUser.status == "Lock")
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} уже заблокирован"
            });
        }

        // 3. Создаем обновленную запись с измененным статусом
        var updatedUser = new
        {
            login = existingUser.login,
            status = "Lock",
            lastName = existingUser.lastName,
            firstName = existingUser.firstName,
            middleName = existingUser.middleName ?? "",
            telephoneNumber = existingUser.telephoneNumber ?? "",
            isLead = existingUser.isLead is bool ? existingUser.isLead : false
        };

        // 4. Заменяем запись пользователя
        usersList[userIndex] = updatedUser;

        Console.WriteLine($"   Пользователь {login} заблокирован");

        // 5. Возвращаем успешный ответ
        return Results.Ok(new
        {
            success = true,
            errorText = (string?)null,
            count = 1,
            data = new { message = $"Пользователь {login} заблокирован" }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Ошибка: {ex.Message}");
        return Results.Ok(new
        {
            success = false,
            errorText = $"Внутренняя ошибка сервера: {ex.Message}"
        });
    }
});


app.MapPut("/api/v1/users/{login}/unlock", (string login) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PUT /api/v1/users/{login}/unlock");

    try
    {
        // 1. Находим пользователя
        var usersList = (List<dynamic>)mockData["users"];
        var userIndex = usersList.FindIndex(u => u.login == login);

        if (userIndex == -1)
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} не найден"
            });
        }

        var existingUser = usersList[userIndex];

        // 2. Проверяем текущий статус
        if (existingUser.status == "Unlock")
        {
            return Results.Ok(new
            {
                success = false,
                errorText = $"Пользователь {login} уже разблокирован"
            });
        }

        // 3. Создаем обновленную запись с измененным статусом
        var updatedUser = new
        {
            login = existingUser.login,
            status = "Unlock",
            lastName = existingUser.lastName,
            firstName = existingUser.firstName,
            middleName = existingUser.middleName ?? "",
            telephoneNumber = existingUser.telephoneNumber ?? "",
            isLead = existingUser.isLead is bool ? existingUser.isLead : false
        };

        // 4. Заменяем запись пользователя
        usersList[userIndex] = updatedUser;

        Console.WriteLine($"   Пользователь {login} разблокирован");

        // 5. Возвращаем успешный ответ
        return Results.Ok(new
        {
            success = true,
            errorText = (string?)null,
            count = 1,
            data = new { message = $"Пользователь {login} разблокирован" }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Ошибка: {ex.Message}");
        return Results.Ok(new
        {
            success = false,
            errorText = $"Внутренняя ошибка сервера: {ex.Message}"
        });
    }
});


app.Run("http://localhost:5000");




// Универсальный класс для создания И редактирования пользователя
class UserRequest
{
    // Обязательные поля для обоих эндпоинтов
    public string login { get; set; } = "";
    public string lastName { get; set; } = "";
    public string firstName { get; set; } = "";
    public string middleName { get; set; } = "";
    public string telephoneNumber { get; set; } = "";
    public bool isLead { get; set; }
    public string status { get; set; } = "";

    // Специфичное поле ТОЛЬКО для создания пользователя
    public string password { get; set; } = "";
}
