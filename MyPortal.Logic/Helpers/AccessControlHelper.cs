﻿using System;
using System.Linq;
using System.Threading.Tasks;
using MyPortal.Database.Constants;
using MyPortal.Database.Enums;
using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Permissions;
using MyPortal.Logic.Services;

namespace MyPortal.Logic.Helpers;

public class AccessControlHelper
{
    internal static async Task<bool> CanAccessPerson(ISessionUser user, IUserService userService, IPersonService personService,
        IStudentService studentService, Guid requestedPersonId)
    {
        var person = await personService.GetPersonWithTypesById(requestedPersonId);

            if (person == null)
            {
                return false;
            }

            if (user.IsType(UserTypes.Student))
            {
                // Students can only access resources involving themselves
                var userId = user.GetUserId();

                if (userId != null)
                {
                    var student = await studentService.GetStudentByUserId(userId.Value);

                    if (student.PersonId == requestedPersonId)
                    {
                        return true;
                    }
                }
            }
            else if (user.IsType(UserTypes.Staff))
            {
                if (person.PersonTypes.StudentId.HasValue)
                {
                    // Staff members can access resources for all students if they have ViewStudentDetails permission
                    return await user.HasPermission(userService, PermissionValue.StudentViewStudentDetails);
                }

                if (person.PersonTypes.StaffId.HasValue)
                {
                    // Staff members can access other basic staff information if they have the ViewBasicDetails permission
                    // Non basic details (e.g employment details) should require further permission checks
                    return await user.HasPermission(userService, PermissionValue.PeopleViewStaffBasicDetails);
                }

                if (person.PersonTypes.ContactId.HasValue)
                {
                    // Staff members can access all contacts if they have ViewContactDetails permission
                    return await user.HasPermission(userService, PermissionValue.PeopleViewContactDetails);
                }
            }
            else if (user.IsType(UserTypes.Parent))
            {
                // Parents can only access resources involving students that they have parental responsibility for
                var userId = user.GetUserId();

                if (userId != null)
                {
                    var userPerson = await personService.GetPersonWithTypesByUser(userId.Value);
                    if (userPerson.PersonTypes.ContactId.HasValue)
                    {
                        var students =
                            await studentService.GetStudentsByContact(userPerson.PersonTypes.ContactId.Value, true);

                        return students.Any(s =>
                        {
                            if (s.Person.Id.HasValue && person.Person.Id.HasValue)
                            {
                                return s.Person.Id.Value == person.Person.Id.Value;
                            }

                            return false;
                        });
                    }
                }
            }

            return false;
    }
}