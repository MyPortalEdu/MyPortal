﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyPortal.Models.Database;

namespace MyPortal.Models.Misc
{
    public class AttendanceRegister
    {
        public IEnumerable<AttendanceMark> Marks { get; set; }
    }
}