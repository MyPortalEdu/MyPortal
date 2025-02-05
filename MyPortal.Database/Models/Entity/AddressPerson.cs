﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Database.Models.Entity
{
    [Table("AddressPeople")]
    public class AddressPerson : BaseTypes.Entity
    {
        [Column(Order = 2)] public Guid AddressId { get; set; }

        [Column(Order = 3)] public Guid? PersonId { get; set; }

        [Column(Order = 4)] public Guid AddressTypeId { get; set; }

        [Column(Order = 5)] public bool Main { get; set; }

        public virtual Address Address { get; set; }
        public virtual Person Person { get; set; }
        public virtual AddressType AddressType { get; set; }
    }
}