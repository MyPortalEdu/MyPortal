﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using MyPortal.Models;
using MyPortal.Models.Database;
using MyPortal.Models.Misc;

namespace MyPortal.Processes
{
    public static class AdminProcesses
    {
        public static async Task<ProcessResponse<object>> AddUserToRole(UserRoleModel roleModel, UserManager<ApplicationUser> userManager, IdentityContext identity)
        {
            var userInDb = identity.Users.SingleOrDefault(u => u.Id == roleModel.UserId);
            var roleInDb = identity.Roles.SingleOrDefault(r => r.Name == roleModel.RoleName);

            if (userInDb == null)
            {
                return new ProcessResponse<object>(ResponseType.NotFound, "User not found", null);
            }

            if (roleInDb == null)
            {
                return new ProcessResponse<object>(ResponseType.NotFound, "Role not found", null);
            }

            switch (roleModel.RoleName)
            {                
                case "Admin":
                case "Finance":
                case "SeniorStaff":
                case "Staff":

                    if (await userManager.IsInRoleAsync(roleModel.UserId, "Student"))
                    {
                        return new ProcessResponse<object>(ResponseType.BadRequest, "Students cannot be added to staff roles", null);
                    }
                    break;

                case "Student":

                    if (await userManager.IsInRoleAsync(roleModel.UserId, "Staff"))
                    {
                        return new ProcessResponse<object>(ResponseType.BadRequest, "Staff members cannot be added to student roles", null);
                    }

                    break;

            }

            if (await userManager.IsInRoleAsync(roleModel.UserId, roleModel.RoleName))
            {
                return new ProcessResponse<object>(ResponseType.BadRequest, "User is already in role", null);
            }

            var result = await userManager.AddToRoleAsync(roleModel.UserId, roleModel.RoleName);

            if (result.Succeeded)
            {
                return new ProcessResponse<object>(ResponseType.Ok, "User added to role", null);
            }

            return new ProcessResponse<object>(ResponseType.BadRequest, "An error occurred", null);
        }

        public static async Task<ProcessResponse<object>> AttachPersonToUser(UserProfile userProfile,
            UserManager<ApplicationUser> userManager, IdentityContext identity, MyPortalDbContext context)
        {
            var userInDb = identity.Users.FirstOrDefault(u => u.Id == userProfile.UserId);
            var roleInDb = identity.Roles.FirstOrDefault(r => r.Name == userProfile.RoleName);

            var userIsAttached = context.Students.Any(x => x.Person.UserId == userProfile.UserId) ||
                                 context.StaffMembers.Any(x => x.Person.UserId == userProfile.UserId);                       

            if (userInDb == null)
            {
                return new ProcessResponse<object>(ResponseType.NotFound, "User not found", null);
            }

            if (roleInDb == null)
            {
                return new ProcessResponse<object>(ResponseType.NotFound, "Role not found", null);
            }

            if (userIsAttached)
            {
                return new ProcessResponse<object>(ResponseType.BadRequest, "A person is already attached to this user", null);
            }

            if (userProfile.RoleName == "Staff")
            {
                var roles = await userManager.GetRolesAsync(userProfile.UserId);
                await userManager.RemoveFromRolesAsync(userProfile.UserId, roles.ToArray());
                await userManager.AddToRoleAsync(userProfile.UserId, "Staff");
                var personInDb = context.StaffMembers.Single(x => x.Id == userProfile.PersonId);
                if (personInDb.Person.UserId != null)
                {
                    return new ProcessResponse<object>(ResponseType.BadRequest, "This person is attached to another user", null);
                }

                personInDb.Person.UserId = userInDb.Id;
                context.SaveChanges();
                return new ProcessResponse<object>(ResponseType.Ok, "Person attached", null);
            }

            if (userProfile.RoleName == "Student")
            {
                var roles = await userManager.GetRolesAsync(userProfile.UserId);
                await userManager.RemoveFromRolesAsync(userProfile.UserId, roles.ToArray());
                await userManager.AddToRoleAsync(userProfile.UserId, "Student");
                var personInDb = context.Students.Single(x => x.Id == userProfile.PersonId);
                if (personInDb.Person.UserId != null)
                {
                    return new ProcessResponse<object>(ResponseType.BadRequest, "This person is attached to another user", null);
                }

                personInDb.Person.UserId = userInDb.Id;
                context.SaveChanges();
                return new ProcessResponse<object>(ResponseType.Ok, "Person attached", null);
            }

            return new ProcessResponse<object>(ResponseType.BadRequest, "An error occurred", null);
        }
    }
}