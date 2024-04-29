using Microsoft.EntityFrameworkCore;

namespace Task.Connector.ContextConstruction.UserContext
{
    // Данный DbContext предусматривает, что у User хранится любой набор полей,
    // на самом же деле схемы данных пользователя хранятся в UserSchema,
    // и относительно этих схем UserModelGenerator задаёт объектные модели пользователя IModel,
    // поэтому у данного класса отсутствует метод OnModelCreating

    // Все свойства, как и логин, хранятся EF Core как property bag

    // В схеме базы данных пароли хранятся отдельно от свойств пользователя
    // однако т. к. в описании системы они являются свойством пользователя,
    // то я решил хранить их вместе с остальными свойствами,
    // а разбиение на несколько таблиц проводить через Table splitting

    // Остальные свойства пользователя не заданы явно в Task.Integration.Data.Models.Models,
    // поэтому я счёл их зависимостью, которую можно передать в IDynamicContextFactory
    // как словарь <имя> - <тип в таблице>

    public class DynamicUserContext : DbContext
    {
        public DynamicUserContext(DbContextOptions<DynamicUserContext> options) : base(options)
        { }

        public DbSet<Dictionary<string, object>> Users => Set<Dictionary<string, object>>("User");
    }
}
