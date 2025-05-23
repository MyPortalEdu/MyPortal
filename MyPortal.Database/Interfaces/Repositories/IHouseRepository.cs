﻿using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface IHouseRepository : IBaseStudentGroupRepository<House>, IUpdateRepository<House>
    {
    }
}