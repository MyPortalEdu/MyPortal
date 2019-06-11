﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyPortal.Models.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string message) : base(message)
        {
        }
    }
}