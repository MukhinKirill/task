using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Task.Connector.Models;
using Task.Connector.Models.Schemas;
using Task.Connector.ContextConstruction.Converter;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.ContextConstruction.PermissionContext
{
    public class PermissionModelGenerator : IModelGenerator<DynamicPermissionContext>
    {
        private readonly PermissionSchema _schema;
        private readonly IConverter _converter;

        public PermissionModelGenerator(PermissionSchema permissionSchema, IConverter converter)
        {
            _schema = permissionSchema;
            _converter = converter;
        }

        private void ConfigurePermissionTypeEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>().ToTable(_schema.PermissionTypeTableName, _schema.SchemaName);

            modelBuilder.Entity<Permission>()
                .Property(type => type.Id)
                .HasColumnName("id")
                .HasColumnType("integer");

            _converter.AddConversion("integer",
                        modelBuilder.Entity<Permission>().Property(type => type.Id));

            modelBuilder.Entity<Permission>()
                .HasKey(type => type.Id);

            modelBuilder.Entity<Permission>()
                .Property(type => type.Name)
                .IsRequired()
                .HasColumnName("name");

            if (_schema.ContainsDescription)
            {
                modelBuilder.Entity<Permission>()
                    .Property(type => type.Description)
                    .IsRequired()
                    .HasColumnName(_schema.DescriptionColumnName!);
            }
            else
            {
                // Если данная разновидность прав пользователя не хранит описания,
                // то свойство type.Description игнорируется EF Core, всегда заполняясь null
                modelBuilder.Entity<Permission>()
                    .Ignore(type => type.Description);
            }
        }

        private void ConfigureUserPermissionEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPermission>().ToTable(_schema.UserPermissionTableName, _schema.SchemaName);

            modelBuilder.Entity<UserPermission>()
                .Property(permission => permission.PermissionTypeId)
                .IsRequired()
                .HasColumnName(_schema.PermissionIdName)
                .HasColumnType("integer");

            _converter.AddConversion("integer",
                        modelBuilder.Entity<UserPermission>().Property(permission => permission.PermissionTypeId));

            modelBuilder.Entity<UserPermission>()
                .Property(permission => permission.UserId)
                .IsRequired()
                .HasColumnName("userId");

            modelBuilder.Entity<UserPermission>()
                .HasKey(permission => new { permission.UserId, permission.PermissionTypeId });
        }

        public IModel GenerateModel()
        {
            var modelBuilder = new ModelBuilder();

            ConfigurePermissionTypeEntity(modelBuilder);
            ConfigureUserPermissionEntity(modelBuilder);

            return modelBuilder.FinalizeModel();
        }
    }
}
