using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Task.Connector.Models;
using Task.Connector.Models.Schemas;
using Task.Connector.ContextConstruction.Converter;

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

        private void ConfigurePermissionTypeEntity(ModelBuilder modelBuilder, string schemaName)
        {
            modelBuilder.Entity<PermissionType>().ToTable(_schema.PermissionTypeTableName, schemaName);

            modelBuilder.Entity<PermissionType>()
                .Property(type => type.Id)
                .HasColumnName("id")
                .HasColumnType("integer");

            _converter.AddConversion("integer",
                        modelBuilder.Entity<PermissionType>().Property(type => type.Id));

            modelBuilder.Entity<PermissionType>()
                .HasKey(type => type.Id);

            modelBuilder.Entity<PermissionType>()
                .Property(type => type.Name)
                .IsRequired()
                .HasColumnName("name");

            if (_schema.ContainsDescription)
            {
                modelBuilder.Entity<PermissionType>()
                    .Property(type => type.Description)
                    .IsRequired()
                    .HasColumnName(_schema.DescriptionColumnName!);
            }
            else
            {
                // Если данная разновидность прав пользователя не хранит описания,
                // то свойство type.Description игнорируется EF Core, всегда заполняясь null
                modelBuilder.Entity<PermissionType>()
                    .Ignore(type => type.Description);
            }
        }

        private void ConfigureUserPermissionEntity(ModelBuilder modelBuilder, string schemaName)
        {
            modelBuilder.Entity<UserPermission>().ToTable(_schema.UserPermissionTableName, schemaName);

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

        public IModel GenerateModel(string schemaName)
        {
            var modelBuilder = new ModelBuilder();

            ConfigurePermissionTypeEntity(modelBuilder, schemaName);
            ConfigureUserPermissionEntity(modelBuilder, schemaName);

            return modelBuilder.FinalizeModel();
        }
    }
}
