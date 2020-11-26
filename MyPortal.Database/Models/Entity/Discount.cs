﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using MyPortal.Database.BaseTypes;

namespace MyPortal.Database.Models.Entity
{
    [Table("Discounts")]
    public class Discount : LookupItem
    {
        [Column(Order = 3, TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(Order = 4)]
        public bool Percentage { get; set; }

        public virtual ICollection<BillDiscount> BillDiscounts { get; set; }
        public virtual ICollection<ChargeDiscount> ChargeDiscounts { get; set; }
    }
}