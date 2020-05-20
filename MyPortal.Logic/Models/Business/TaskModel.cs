﻿using System;
using System.ComponentModel.DataAnnotations;

namespace MyPortal.Logic.Models.Business
{
    public class TaskModel
    {
        public Guid Id { get; set; }
        public Guid AssignedToId { get; set; }
        public Guid? AssignedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }

        [Required]
        [StringLength(128)]
        public string Title { get; set; }

        [StringLength(256)]
        public string Description { get; set; }

        public bool Personal { get; set; }
        public bool Completed { get; set; }
        public virtual HomeworkSubmissionModel HomeworkSubmission { get; set; }
        public virtual PersonModel AssignedTo { get; set; }
        public virtual PersonModel AssignedBy { get; set; }
        
        #region Business Logic
        
        public bool Overdue
        {
            get { return !Completed && DueDate <= DateTime.Today; }
        }
        
        #endregion
    }
}