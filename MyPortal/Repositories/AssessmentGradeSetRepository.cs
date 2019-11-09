﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyPortal.Interfaces;
using MyPortal.Models.Database;

namespace MyPortal.Repositories
{
    public class AssessmentGradeSetRepository : Repository<AssessmentGradeSet>, IAssessmentGradeSetRepository
    {
        public AssessmentGradeSetRepository(MyPortalDbContext context) : base(context)
        {

        }
    }
}