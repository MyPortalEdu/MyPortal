﻿using MyPortal.Database.Models;

namespace MyPortal.Logic.Models.Requests.Person
{
    public class PersonAttachmentUpload
    {
        public Document Document { get; set; }
        public int PersonId { get; set; }
    }
}