﻿using MyPortal.Data.Models;

namespace MyPortal.BusinessLogic.Extensions
{
    public static class StaffMemberExtensions
    {
        public static string GetFullName(this StaffMember staffMember)
        {
            return $"{staffMember.Person.LastName}, {staffMember.Person.FirstName}";
        }

        public static string GetDisplayName(this StaffMember staffMember)
        {
            return staffMember == null
                ? null
                : $"{staffMember.Person.Title} {staffMember.Person.FirstName.Substring(0, 1)} {staffMember.Person.LastName}";
        }
    }
}