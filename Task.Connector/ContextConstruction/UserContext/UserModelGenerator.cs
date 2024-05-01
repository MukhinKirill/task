using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Task.Connector.ContextConstruction.Converter;
using Task.Connector.Models.Schemas;

namespace Task.Connector.ContextConstruction.UserContext
{
    // В базе данных нет ограничений Unique на userId в таблице паролей
    // однако в данном решении я предполагаю, что для каждого пользователя объявлен ровно один пароль;
    // в случае, если их несколько - будет взят случайный;
    // при попытке найти несуществующий пароль будет выбрасываться Exception.
    // В таком случае мы можем использовать Entity Splitting и хранить пароль в экземплярах User

    public class UserModelGenerator : IModelGenerator<DynamicUserContext>
    {
        private readonly UserSchema _schema;
        private readonly IConverter _converter;

        public UserModelGenerator(UserSchema userSchema, IConverter converter)
        {
            _schema = userSchema;
            _converter = converter;
        }

        public IModel GenerateModel(string schemaName)
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder.SharedTypeEntity<Dictionary<string, object>>(
            "User", user =>
            {
                user.Property<string>("login");
                user.Property<string>("password")
                    .IsRequired();
                foreach (var property in _schema.PropertyTypes)
                {
                    user.Property<string>(property.Key)
                        .HasColumnType(property.Value)
                        .IsRequired();
                    _converter.AddConversion(property.Value,
                        user.Property<string>(property.Key));
                }
                user.HasKey("login");
            });

            modelBuilder.SharedTypeEntity<Dictionary<string, object>>(
            "User", user =>
            {
                user.ToTable(_schema.UserTableName, schemaName)
                .SplitToTable(
                    _schema.PasswordTableName,
                    schemaName,
                    tableBuilder =>
                    {
                        tableBuilder.Property("login").HasColumnName("userId");
                        tableBuilder.Property("password").HasColumnName("password");
                    });
            });

            return modelBuilder.FinalizeModel();
        }
    }
}
