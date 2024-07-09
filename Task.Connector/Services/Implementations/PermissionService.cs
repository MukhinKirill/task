using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Task.Connector.DAL;
using Task.Connector.Services.Interfaces;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.Implementations
{
    internal class PermissionService : IPermissionService
    {
        private static string _requestRightGroupName = "Request";
        private static string _itRoleRightGroupName = "Role";
        private static string _delimeter = ":";
        private ConnectorDbContext _dbContext;
        private ILogger _logger;

        public PermissionService(ConnectorDbContext dbContext, ILogger logger)
        {
            this._dbContext = dbContext;
            this._logger = logger;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            
            if (!string.IsNullOrEmpty(userLogin) && rightIds != null)
            {
                using var transaction = _dbContext.Database.BeginTransaction();
                try
                {
                    foreach (var rightId in rightIds)
                    {
                        var valueInRight = int.Parse(rightId.Split(_delimeter)[1]);
                        if (rightId.Contains(_requestRightGroupName))
                        {
                            _dbContext.UserRequestRights.Add(new UserRequestRight()
                            {
                                UserId = userLogin,
                                RightId = valueInRight
                            });
                        }
                        else if (rightId.Contains(_itRoleRightGroupName))
                        {
                            _dbContext.UsersITRoles.Add(new UserITRole()
                            {
                                UserId = userLogin,
                                RoleId = valueInRight
                            });
                        }

                    }
                    _dbContext.SaveChanges();
                    transaction.Commit();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Error($"DB error: {ex.Message}");
                    throw new Exception($"DB error: {ex.Message}");
                }
            }
            _logger.Error($"input Data must be not null");
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!string.IsNullOrEmpty(userLogin) && rightIds != null)
            {
                using var transaction = _dbContext.Database.BeginTransaction();
                try
                {
                    foreach (var rightId in rightIds)
                    {
                        var valueInRight = rightId.Split(_delimeter)[1];
                        if (rightId.Contains(_requestRightGroupName))
                        {
                            var a = _dbContext.UserRequestRights
                                .Where(ur => ur.UserId == userLogin && ur.RightId.ToString() == valueInRight).ToList();
                            _dbContext.UserRequestRights.RemoveRange(a);
                        }
                        else if (rightId.Contains(_itRoleRightGroupName))
                        {
                            var a = _dbContext.UsersITRoles
                                .Where(ur => ur.UserId == userLogin && ur.RoleId.ToString() == valueInRight).ToList();
                            _dbContext.UsersITRoles.RemoveRange(a);
                        }
                    }
                    _dbContext.SaveChanges();
                    transaction.Commit();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Error($"DB error: {ex.Message}");
                    throw new Exception($"DB error: {ex.Message}");
                }
            }
            _logger.Error($"input Data must be not null");
        }
        public IEnumerable<Permission> GetAllPermissions()
        {
            var permissions = new List<Permission>();
            //добавляем RequestRights
            try
            {
                _dbContext.RequestRights.ToList().ForEach(right =>
                {
                    permissions.Add(new Permission(right.Id.ToString(), right.Name, string.Empty));
                });
                //добавляем ITRole
                _dbContext.ITRoles.ToList().ForEach(role =>
                {
                    permissions.Add(new Permission(role.Id.ToString(), role.Name, role.CorporatePhoneNumber));
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"DB error: {ex.Message}");
                throw new Exception($"DB error: {ex.Message}");
            }
            return permissions;
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var userPermissions = new List<string>();
            try
            {
                _dbContext.UserRequestRights
                .ToList().ForEach(userRight =>
                {
                    if (userLogin == userRight.UserId)
                    {
                        var right = _dbContext.RequestRights.FirstOrDefault(right => right.Id == userRight.RightId);
                        userPermissions.Add(right.ToString());
                    }
                });

                _dbContext.UsersITRoles
                    .ToList().ForEach(userItRole =>
                    {
                        if (userLogin == userItRole.UserId)
                        {
                            var role = _dbContext.ITRoles.FirstOrDefault(role => role.Id == userItRole.RoleId);
                            userPermissions.Add(role.ToString());
                        }
                    });
            }
            catch (Exception ex)
            {
                _logger.Error($"DB error: {ex.Message}");
                throw new Exception($"DB error: {ex.Message}");
            }
            return userPermissions;
        }

    }
}
