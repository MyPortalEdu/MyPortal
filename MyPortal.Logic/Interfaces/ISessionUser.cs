﻿using System;
using System.Threading.Tasks;
using MyPortal.Database.Constants;
using MyPortal.Database.Enums;
using MyPortal.Database.Interfaces;
using MyPortal.Logic.Enums;
using MyPortal.Logic.Interfaces.Services;

namespace MyPortal.Logic.Interfaces;

public interface ISessionUser
{
    Task<IUnitOfWork> GetConnection();

    Task<bool> HasPermission(IUserService userService, PermissionRequirement requirement,
        params PermissionValue[] permissionValues);
    
    Task<bool> HasPermission(IUserService userService,
        params PermissionValue[] permissionValues);

    bool IsType(int userType);

    Guid? GetUserId();
}