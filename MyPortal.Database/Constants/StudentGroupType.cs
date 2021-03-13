﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MyPortal.Database.Constants
{
    public class StudentGroupType
    {
        public static Guid Course { get; } = Guid.Parse("5E37BCFF-6C32-4FC9-B2CB-D45E44C7A3D6");
        public static Guid RegGroup { get; } = Guid.Parse("5E37BCFF-6C32-4FC9-B2CB-D45E44C7A3D7");
        public static Guid YearGroup { get; } = Guid.Parse("5E37BCFF-6C32-4FC9-B2CB-D45E44C7A3D8");
        public static Guid CurriculumYearGroup { get; } = Guid.Parse("5E37BCFF-6C32-4FC9-B2CB-D45E44C7A3D9");
        public static Guid ExamAwardEnrolments { get; } = Guid.Parse("5E37BCFF-6C32-4FC9-B2CB-D45E44C7A3DA");
        public static Guid House { get; } = Guid.Parse("5E37BCFF-6C32-4FC9-B2CB-D45E44C7A3DB");
        public static Guid CurriculumGroup { get; } = Guid.Parse("5E37BCFF-6C32-4FC9-B2CB-D45E44C7A3DC");
        public static Guid Session { get; } = Guid.Parse("5E37BCFF-6C32-4FC9-B2CB-D45E44C7A3DD");
    }
}