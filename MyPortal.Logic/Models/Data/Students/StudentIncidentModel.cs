using System;
using System.Threading.Tasks;
using MyPortal.Database.Interfaces;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.Behaviour.Incidents;
using MyPortal.Logic.Models.Structures;
using MyPortal.Logic.Models.Summary;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Logic.Models.Data.Students;

public class StudentIncidentModel : EntityModel
{
    public StudentIncidentModel(StudentIncident model) : base(model)
    {
        LoadFromModel(model);
    }

    private void LoadFromModel(StudentIncident model)
    {
        StudentId = model.StudentId;
        IncidentId = model.IncidentId;
        RoleTypeId = model.RoleTypeId;
        OutcomeId = model.OutcomeId;
        StatusId = model.StatusId;
        Points = model.Points;

        if (model.Student != null)
        {
            Student = new StudentModel(model.Student);
        }

        if (model.Incident != null)
        {
            Incident = new IncidentModel(model.Incident);
        }

        if (model.RoleType != null)
        {
            RoleType = new BehaviourRoleTypeModel(model.RoleType);
        }

        if (model.Outcome != null)
        {
            Outcome = new BehaviourOutcomeModel(model.Outcome);
        }

        if (model.Status != null)
        {
            Status = new BehaviourStatusModel(model.Status);
        }
    }

    public Guid StudentId { get; set; }

    public Guid IncidentId { get; set; }

    public Guid RoleTypeId { get; set; }

    public Guid? OutcomeId { get; set; }

    public Guid StatusId { get; set; }

    public int Points { get; set; }

    public StudentModel Student { get; set; }
    public IncidentModel Incident { get; set; }
    public BehaviourRoleTypeModel RoleType { get; set; }
    public BehaviourOutcomeModel Outcome { get; set; }
    public BehaviourStatusModel Status { get; set; }

    public BehaviourInvolvedStudentSummaryModel[] InvolvedStudents { get; set; }

    internal StudentIncidentSummaryModel ToListModel(IUnitOfWork unitOfWork)
    {
        return new StudentIncidentSummaryModel(this);
    }
}