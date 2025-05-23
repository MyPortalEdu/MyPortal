﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyPortal.Database.Enums;
using MyPortal.Logic.Enums;
using MyPortal.Logic.Exceptions;
using MyPortal.Logic.Extensions;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Settings;
using MyPortal.Logic.Models.Response;

namespace MyPortalWeb.Controllers.BaseControllers
{
    [Authorize]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected BaseApiController()
        {
            
        }
        
        protected IActionResult Error(int statusCode, string errorMessage)
        {
            var error = new ErrorResponseModel(errorMessage);

            return StatusCode(statusCode, error);
        }

        protected IActionResult Error(HttpStatusCode statusCode, string errorMessage)
        {
            return Error((int)statusCode, errorMessage);
        }
    }
}