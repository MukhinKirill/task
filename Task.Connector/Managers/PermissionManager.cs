using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.DbCommon;
using Microsoft.EntityFrameworkCore;

namespace Task.Connector.Managers
{
    public class PermissionManager
    {
        private DataContext dbContext;
        private ILogger _logger;

        public PermissionManager(DataContext dbContext, ILogger logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger;
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                var requestRights = dbContext.RequestRights
                    .Select(requestRights => new Permission(requestRights.Id.ToString(), requestRights.Name, string.Empty)).ToList();

                var itRoles = dbContext.ITRoles
                    .Select(itRoles => new Permission(itRoles.Id.ToString(), itRoles.Name, itRoles.CorporatePhoneNumber)).ToList();

                var permissions = requestRights.Concat(itRoles);

                _logger?.Debug("Все права в системе успешно получены!");

                return permissions;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Ошибка при получении прав системы: {ex.Message}");
                throw;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {

                foreach (var permission in rightIds)
                {
                    //Из теста делаю вывод что permission должен иметь следующий формат PermissionType (Role или Request) : ID
                    var permissionParts = permission.Split(':');
                    if (permissionParts.Length != 2)
                    {
                        _logger?.Error($"Неверный формат строки разрешения: {permission}.\n"
                            + "Ожидаемый формат вида PermissionType:ID");
                        throw new Exception($"Неверный формат строки разрешения: {permission}.\n"
                            + "Ожидаемый формат вида PermissionType:ID");
                    }

                    var permissionType = permissionParts[0];
                    var permissionId = permissionParts[1];

                    switch (permissionType)
                    {
                        case "Role":
                            if (int.TryParse(permissionId, out int roleID) && dbContext.ITRoles.Any(role_id => role_id.Id == roleID))
                                dbContext.UserITRoles.Add(new UserITRole() { UserId = userLogin, RoleId = roleID });
                            break;
                        case "Request":
                            if (int.TryParse(permissionId, out int rightID) && dbContext.RequestRights.Any(right_id => right_id.Id == rightID))
                                dbContext.UserRequestRights.Add(new UserRequestRight() { UserId = userLogin, RightId = rightID });
                            break;
                        default:
                            _logger?.Error("Не верное значения ID или PermissionType");
                            throw new Exception("Не верное значения ID или PermissionType");
                    }

                    dbContext.SaveChanges();

                    _logger?.Debug("Права успешно добавлены пользователю!");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Ошибка добавления прав пользователю: {ex.Message}");
                throw;
            }

        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                foreach (var permission in rightIds)
                {

                    var permissionParts = permission.Split(':');
                    if (permissionParts.Length != 2)
                    {
                        _logger?.Error($"Неверный формат строки разрешения: {permission}.\n"
                            + "Ожидаемый формат вида PermissionType:ID");
                        throw new Exception($"Неверный формат строки разрешения: {permission}.\n"
                            + "Ожидаемый формат вида PermissionType:ID");
                    }

                    var permissionType = permissionParts[0];
                    var permissionId = int.Parse(permissionParts[1]);

                    switch (permissionType)
                    {
                        case "Role":
                            var roleToRemove = dbContext.UserITRoles.FirstOrDefault(role => role.UserId == userLogin && role.RoleId == permissionId);
                            if (permissionId != null)
                                dbContext.UserITRoles.Remove(roleToRemove);
                            break;
                        case "Request":
                            var rightToRemove = dbContext.UserRequestRights.FirstOrDefault(role => role.UserId == userLogin && role.RightId == permissionId);
                            if (permissionId != null)
                                dbContext.UserRequestRights.Remove(rightToRemove);
                            break;
                        default:
                            _logger?.Error("Не верное значения ID или PermissionType");
                            throw new Exception("Не верное значения ID или PermissionType");
                    }

                    _logger?.Debug("Права пользователя успешно удалены!");
                }
                dbContext.SaveChanges();

            }
            catch (Exception ex)
            {
                _logger?.Error($"Ошибка удаления прав пользователя: {ex.Message}");
                throw;
            }

        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                var rightValues = dbContext.UserRequestRights
                    .Where(userID => userID.UserId == userLogin)
                    .Select(value => value.RightId.ToString()).ToList();

                var roleValues = dbContext.UserITRoles
                    .Where(userID => userID.UserId == userLogin)
                    .Select(value => value.RoleId.ToString()).ToList();


                var userPermissions = rightValues.Concat(roleValues);

                _logger?.Debug("Права пользователя успешно получены!");

                return userPermissions;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Ошибка получения прав пользователя: {ex.Message}");
                throw;
            }
        }
    }
}
