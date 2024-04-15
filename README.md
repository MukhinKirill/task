# Инстркуция по запуску
## 1. Запуск Docker образа с базой данных
cd Docker 

docker-compose up -d

![docker](https://github.com/Eh0neer/testAvanpost/assets/114866823/1fbc2524-a6d0-4c8f-83bc-98bdb60d237d)

## 2. Создание миграции данных в базу
cd DbCreationUtility

### Команда для миграции данных:
```bash
Task.Integration.Data.DbCreationUtility.exe -s "Host=localhost;Port=13470;Database=TestDB;User Id=postgres;Password=QWEasd123;Include Error Detail=true;" -p "POSTGRE"
```

![dbDataMigraton](https://github.com/Eh0neer/testAvanpost/assets/114866823/495adb96-caee-4b46-9785-f0cdf79d47b2)

## Успешное завершение Unit тестов

![testResult](https://github.com/Eh0neer/testAvanpost/assets/114866823/d6dee3d6-6b65-418f-a8f1-f37e27756849)


# Описание системы
Система представляет собой сервис для технического обслуживания.
Внутри сервиса существуют пользователи(User), имеющие свойства(Properties) и права(RequestRight и ItRole).
Свойства пользователя(его атрибуты) имеют постоянный состав(lastName, firstName, middleName, telephoneNumber, isLead). Стоит отметить, что Логин является уникальным идентификатором пользователя в системе и свойством не является!
Права(RequestRight и ItRole) позволяют пользователю выполнять те или иные операции в системе(например просмотривать необходимые для пользователя отчеты)
Список актуальных прав будет находится в соотвутствующих таблицах после развертывания/заполнения бд через утилиту Task.Integration.Data.DbCreationUtility, которая будет описана ниже.
Инициализация конфигурации конектора происходит через метод StartUp и обязательное требование - наличие пустого конструктора.

# Таблицы БД:
* Таблица с пользователями User(все столбцы ненулевые);
* Таблица с паролями Passwords(все столбцы ненулевые);
* Таблица с правами по изменению заявок RequestRight;
* Таблица с ролями исполнителей ItRole;
* Таблицы для связи пользователей и прав UserItRole, UserRequestRight(Все столбцы ненулевые, изменение прав пользователя состоит в добавлении и удалении данных из этих таблиц);

# Развертывание системы
Для создание схемы, таблиц и заполнения данными используется утилита Task.Integration.Data.DbCreationUtility.exe(папка DbCreationUtility). Поддерживаются MSSQL и Postgre. Поддерживаемые значения параметра -p POSTGRE, MSSQL.

команды:
Task.Integration.Data.DbCreationUtility.exe -s "строка подключения к бд" -p "провайдер бд"
пример: Task.Integration.Data.DbCreationUtility.exe -s "Server=127.0.0.1;Port=5432;Database=testDb;Username=testUser;Password=12345678;" -p "POSTGRE"

# Структура решения:
* Task.Connector.Tests - проект с тестами коннектора(его можно и нужно использовать как точку входа в методы при отладке коннектора);
* Task.Connector - проект с реализуемым коннектором

# Задание
* Развернуть бд (Postgres или MSSQL) в Docker или с помощью других средств;
* Заполнить тестовыми данными с помощью утилиты Avanpost.Integration.DbCreationUtility;
* Реализовать интерфейс коннектора:
```csharp
        public ILogger Logger { get; set; }
        void StartUp(string connectionString); //Конфигурация коннектора через строку подключения (настройки для подключения к ресурсу(строка подключения к бд, 
        // путь к хосту с логином и паролем, дополнительные параметры конфигурации бизнес-логики и тд, формат любой, например: "key1=value1;key2=value2...";
        
		void CreateUser(UserToCreate user); // Создать пользователя с набором свойств по умолчанию.
		bool IsUserExists(string userLogin); // Проверка существования пользователя
        
		IEnumerable<Property> GetAllProperties(); // Метод позволяет получить все свойства пользователя(смотри Описание системы), пароль тоже считать свойством
        IEnumerable<UserProperty> GetUserProperties(string userLogin); // Получить все значения свойств пользователя
        void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);// Метод позволяет устанавливать значения свойств пользователя
        
		IEnumerable<Permission> GetAllPermissions();// Получить все права в системе (смотри Описание системы)
        void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);// Добавить права пользователю в системе
        void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);// Удалить права пользователю в системе
        IEnumerable<string> GetUserPermissions(string userLogin);// Получить права пользователя в системе
```

# Требования по реализации интерфейса коннектора
* Коннектор реализует интерфейс IConnector (все методы интерфейса);
* Коннектор проходит все тесты
* Коннектор не изменяет данные в таблицах RequestRights и ItRole;
* Коннектор использует логирование через свойство Logger;
* При работе с Permission разделяет ItRole и RequestRight;


