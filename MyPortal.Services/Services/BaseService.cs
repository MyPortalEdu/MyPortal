﻿using MyPortal.Auth.Interfaces;

namespace MyPortal.Services.Services;

public class BaseService
{
    protected readonly IAuthorizationService _authorizationService;

    public BaseService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }
}