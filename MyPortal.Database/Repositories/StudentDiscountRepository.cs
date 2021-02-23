﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models;
using MyPortal.Database.Models.Entity;
using MyPortal.Database.Repositories.Base;

namespace MyPortal.Database.Repositories
{
    public class StudentDiscountRepository : BaseReadWriteRepository<StudentDiscount>, IStudentDiscountRepository
    {
        public StudentDiscountRepository(ApplicationDbContext context) : base(context, "SD")
        {
        }

        public async Task<IEnumerable<StudentDiscount>> GetByStudent(Guid studentId)
        {
            throw new NotImplementedException();
        }
    }
}