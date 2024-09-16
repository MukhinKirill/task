using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _connectionString = connectionString;
        }

        public void Initialize()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                     CREATE TABLE [User] (
                         [Id] INT IDENTITY(1,1) PRIMARY KEY,
                         [Login] NVARCHAR(255) NOT NULL,
                         [Password] NVARCHAR(255) NOT NULL
                     );

                     CREATE TABLE [Passwords] (
                         [Id] INT IDENTITY(1,1) PRIMARY KEY,
                         [UserId] INT NOT NULL,
                         [Password] NVARCHAR(255) NOT NULL,
                         CONSTRAINT FK_User_Passwords FOREIGN KEY ([UserId]) REFERENCES [User]([Id])
                     );

                     CREATE TABLE [RequestRight] (
                         [Id] INT IDENTITY(1,1) PRIMARY KEY,
                         [RightName] NVARCHAR(255) NOT NULL
                     );

                     CREATE TABLE [ItRole] (
                         [Id] INT IDENTITY(1,1) PRIMARY KEY,
                         [RoleName] NVARCHAR(255) NOT NULL
                     );

                     CREATE TABLE [UserItRole] (
                         [UserId] INT NOT NULL,
                         [RoleId] INT NOT NULL,
                         CONSTRAINT FK_UserItRole_User FOREIGN KEY ([UserId]) REFERENCES [User]([Id]),
                         CONSTRAINT FK_UserItRole_ItRole FOREIGN KEY ([RoleId]) REFERENCES [ItRole]([Id])
                     );

                     CREATE TABLE [UserRequestRight] (
                         [UserId] INT NOT NULL,
                         [RightId] INT NOT NULL,
                         CONSTRAINT FK_UserRequestRight_User FOREIGN KEY ([UserId]) REFERENCES [User]([Id]),
                         CONSTRAINT FK_UserRequestRight_RequestRight FOREIGN KEY ([RightId]) REFERENCES [RequestRight]([Id])
                     );
                 ";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
