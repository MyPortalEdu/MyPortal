﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Database.Models.Entity;

[Table("StudentIncidents")]
public class StudentIncident : BaseTypes.Entity
{
    [Column(Order = 1)]
    public Guid StudentId { get; set; }
    
    [Column(Order = 2)]
    public Guid IncidentId { get; set; }
    
    [Column(Order = 3)]
    public Guid RoleTypeId { get; set; }
    
    [Column(Order = 4)]
    public Guid? OutcomeId { get; set; }

    [Column(Order = 5)]
    public Guid StatusId { get; set; }
    
    [Column(Order = 6)]
    public int Points { get; set; }
    
    public virtual Student Student { get; set; }
    public virtual Incident Incident { get; set; }
    public virtual BehaviourRoleType RoleType { get; set; }
    public virtual BehaviourOutcome Outcome { get; set; }
    public virtual BehaviourStatus Status { get; set; }

    public virtual ICollection<StudentIncidentDetention> LinkedDetentions { get; set; }
}