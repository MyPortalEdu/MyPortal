using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Helpers
{
    internal static class EntityHelper
    {
        private static readonly Dictionary<Type, string[]> _columnCache = new();

        private static string[] GetColumnNames<T>()
        {
        
            var type = typeof(T);
            if (_columnCache.TryGetValue(type, out var columns))
                return columns;

            columns = type.GetProperties()
                .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null)
                .Select(p => p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name)
                .ToArray();

            _columnCache[type] = columns;
            return columns;

        }
        
        internal static string[] GetColumns(Type t, string alias = null)
        {
            var columnNames = new List<string>();

            if (t == typeof(Role))
            {
                return GetRolePropertyNames(alias);
            }

            if (t == typeof(User))
            {
                return GetUserPropertyNames(alias);
            }

            if (t == typeof(UserRole))
            {
                return GetUserRolePropertyNames(alias);
            }

            var columns = GetColumnNames<Type>();

            foreach (var column in columns)
            {
                columnNames.Add($"{alias}.{column}");
            }

            return columnNames.ToArray();
        }

        internal static string[] GetRolePropertyNames(string alias = null)
        {
            string tblAlias = string.IsNullOrWhiteSpace(alias) ? "Roles" : alias;

            var propNames = new List<string>
            {
                $"{tblAlias}.Id",
                $"{tblAlias}.Name",
                $"{tblAlias}.NormalizedName",
                $"{tblAlias}.Description",
                $"{tblAlias}.ConcurrencyStamp",
                $"{tblAlias}.Permissions",
                $"{tblAlias}.System"
            };

            return propNames.ToArray();
        }

        internal static string[] GetUserPropertyNames(string alias = null)
        {
            string tblAlias = string.IsNullOrWhiteSpace(alias) ? "Users" : alias;

            var propNames = new List<string>
            {
                $"{tblAlias}.Id",
                $"{tblAlias}.UserName",
                $"{tblAlias}.NormalizedUserName",
                $"{tblAlias}.Email",
                $"{tblAlias}.NormalizedEmail",
                $"{tblAlias}.EmailConfirmed",
                $"{tblAlias}.PasswordHash",
                $"{tblAlias}.SecurityStamp",
                $"{tblAlias}.ConcurrencyStamp",
                $"{tblAlias}.PhoneNumber",
                $"{tblAlias}.PhoneNumberConfirmed",
                $"{tblAlias}.TwoFactorEnabled",
                $"{tblAlias}.LockoutEnd",
                $"{tblAlias}.LockoutEnabled",
                $"{tblAlias}.AccessFailedCount",
                $"{tblAlias}.CreatedDate",
                $"{tblAlias}.PersonId",
                $"{tblAlias}.UserType",
                $"{tblAlias}.Enabled"
            };

            return propNames.ToArray();
        }

        internal static string[] GetUserRolePropertyNames(string alias = null)
        {
            string tblAlias = string.IsNullOrWhiteSpace(alias) ? "UserRoles" : alias;

            var propNames = new List<string>
            {
                $"{tblAlias}.UserId",
                $"{tblAlias}.RoleId"
            };

            return propNames.ToArray();
        }
    }
}