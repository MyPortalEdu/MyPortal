using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.StaffMembers
{
    public class TrainingCertificateModel : EntityModel
    {
        public TrainingCertificateModel(TrainingCertificate model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(TrainingCertificate model)
        {
            CourseId = model.CourseId;
            StaffId = model.StaffId;
            StatusId = model.StatusId;

            if (model.StaffMember != null)
            {
                StaffMember = new StaffMemberModel(model.StaffMember);
            }

            if (model.TrainingCourse != null)
            {
                TrainingCourse = new TrainingCourseModel(model.TrainingCourse);
            }

            if (model.Status != null)
            {
                Status = new TrainingCertificateStatusModel(model.Status);
            }
        }

        public Guid CourseId { get; set; }

        public Guid StaffId { get; set; }

        public Guid StatusId { get; set; }

        public virtual StaffMemberModel StaffMember { get; set; }

        public virtual TrainingCourseModel TrainingCourse { get; set; }

        public virtual TrainingCertificateStatusModel Status { get; set; }
    }
}