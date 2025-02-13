﻿using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface IDietaryRequirementRepository : IReadWriteRepository<DietaryRequirement>,
        IUpdateRepository<DietaryRequirement>
    {
    }
}