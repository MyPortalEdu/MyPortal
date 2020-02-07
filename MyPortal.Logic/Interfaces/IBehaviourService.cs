﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MyPortal.Logic.Interfaces
{
    public interface IBehaviourService
    {
        Task AddIncidentToDetention(Guid incidentId, Guid detentionId);
        Task RemoveIncidentFromDetention(Guid incidentId, Guid detentionId);
    }
}