﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace MyPortal.Database.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public virtual Person Person { get; set; }
    }
}