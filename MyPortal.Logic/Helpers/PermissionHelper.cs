﻿using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using MyPortal.Database.Enums;
using MyPortal.Logic.Enums;
using MyPortal.Logic.Interfaces.Services;

namespace MyPortal.Logic.Helpers
{
    internal class PermissionHelper
    {
        internal static BitArray CreatePermissionArray()
        {
            var array = new BitArray(Enum.GetNames(typeof(PermissionValue)).Length);

            return array;
        }

        internal static async Task<bool> UserHasPermission(Guid userId, IUserService userService,
            PermissionRequirement requirement, params PermissionValue[] permissionValues)
        {
            if (!permissionValues.Any())
            {
                return true;
            }

            var roles = await userService.GetUserRoles(userId);

            foreach (var role in roles)
            {
                var rolePermissions = new BitArray(role.Permissions);

                foreach (var permissionValue in permissionValues)
                {
                    if (rolePermissions[(int)permissionValue])
                    {
                        if (requirement == PermissionRequirement.RequireAny)
                        {
                            return true;
                        }
                    }
                    else if (requirement == PermissionRequirement.RequireAll)
                    {
                        // User is missing one of the permissions
                        return false;
                    }
                }
            }

            // User has all permissions
            return requirement == PermissionRequirement.RequireAll;
        }
    }
}