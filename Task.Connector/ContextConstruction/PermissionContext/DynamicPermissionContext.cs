using Microsoft.EntityFrameworkCore;
using Task.Connector.Models;

namespace Task.Connector.ContextConstruction.PermissionContext
{
    // Так как разные виды прав пользователя (т е ItRole и RequestRight) 
    // не связаны между собой и не знают друг о друге,
    // и т к Permission не знает о существовании различных таблиц с правами,
    // то я решил создать единую схему DbContext,
    // позволяющую работать с одним обобщенным видом прав пользователя

    // Для работы с разными видами прав потребуются разные DbContext,
    // построенные на разных IModel

    // Описания конкретных разновидностей прав предоставляют PermissionSchema,
    // относительно них PermissionModelGenerator создаёт объектные модели прав IModel,
    // поэтому у данного класса отсутствует метод OnModelCreating
    public class DynamicPermissionContext : DbContext
    {
        public DynamicPermissionContext(DbContextOptions<DynamicPermissionContext> options) : base(options)
        { }

        public DbSet<PermissionType> PermissionTypes { get; set; }

        public DbSet<UserPermission> UserPermissions { get; set; }
    }
}
