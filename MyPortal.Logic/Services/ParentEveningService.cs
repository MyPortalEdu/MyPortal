﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MyPortal.Logic.Enums;
using MyPortal.Logic.Exceptions;
using MyPortal.Logic.Helpers;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data;
using MyPortal.Logic.Models.Response.Contacts;

namespace MyPortal.Logic.Services
{
    public class ParentEveningService : BaseService, IParentEveningService
    {
        public async Task<IEnumerable<ParentEveningAppointmentTemplateModel>> GetAppointmentTemplatesByStaffMember(
            Guid parentEveningId, Guid staffMemberId)
        {
            var templates = new List<ParentEveningAppointmentTemplateModel>();
            
            using (var unitOfWork = await DataConnectionFactory.CreateUnitOfWork())
            {
                var parentEvening = await unitOfWork.ParentEvenings.GetById(parentEveningId);

                if (parentEvening == null)
                {
                    throw new NotFoundException("Parent evening not found.");
                }

                var pesm = await unitOfWork.ParentEveningStaffMembers.GetInstanceByStaffMember(parentEveningId,
                    staffMemberId);

                if (pesm == null)
                {
                    throw new NotFoundException("Parent evening staff member not found.");
                }

                var appointments =
                    (await unitOfWork.ParentEveningAppointments.GetAppointmentsByStaffMember(parentEveningId,
                        staffMemberId))?.ToArray();

                var breaks =
                    (await unitOfWork.ParentEveningBreaks.GetBreaksByStaffMember(parentEveningId, staffMemberId))?
                    .ToArray();

                var from = pesm.AvailableFrom.HasValue ? pesm.AvailableFrom.Value : parentEvening.Event.StartTime;

                var to = pesm.AvailableTo.HasValue ? pesm.AvailableTo.Value : parentEvening.Event.EndTime;
                
                var allStartTimes = DateTimeHelper.GetAllInstances(from, to, DateTimeDivision.Minute, pesm.AppointmentLength);

                foreach (var startTime in allStartTimes)
                {
                    var template = new ParentEveningAppointmentTemplateModel(pesm.ParentEveningId, pesm.StaffMemberId, startTime,
                        startTime.AddMinutes(pesm.AppointmentLength));

                    var templateRange = template.GetDateRange();

                    if (appointments != null &&
                        appointments.Any(a => templateRange.Overlaps(new DateRange(a.Start, a.End))))
                    {
                        template.Available = false;
                    }

                    if (template.Available && breaks != null &&
                        breaks.Any(b => templateRange.Overlaps(new DateRange(b.Start, b.End))))
                    {
                        template.Available = false;
                    }

                    templates.Add(template);
                }
            }

            return templates;
        }

        public ParentEveningService(ClaimsPrincipal user) : base(user)
        {
        }
    }
}