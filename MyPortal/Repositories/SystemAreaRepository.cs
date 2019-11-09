﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyPortal.Interfaces;
using MyPortal.Models.Database;

namespace MyPortal.Repositories
{
    public class SystemAreaRepository : ReadOnlyRepository<SystemArea>, ISystemAreaRepository
    {
        public SystemAreaRepository(MyPortalDbContext context) : base(context)
        {

        }
    }
}