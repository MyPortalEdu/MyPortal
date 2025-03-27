using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Requests.Calendar;
using MyPortal.Logic.Models.Structures;

namespace MyPortalWeb.Controllers.Api
{
    [Authorize]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpGet]
        [Route("api/people/{personId}/calendar")]
        [ProducesResponseType(typeof(IEnumerable<CalendarEventModel>), 200)]
        public async Task<IActionResult> GetCalendarEventsByPerson([FromRoute] Guid personId,
            [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            DateRange dateRange;

            if (dateFrom == null || dateTo == null)
            {
                dateRange = DateRange.CurrentWeek;
            }
            else
            {
                dateRange = new DateRange(dateFrom.Value, dateTo.Value);
            }

            var events =
                await _calendarService.GetCalendarEventsByPerson(personId, dateRange.Start, dateRange.End);

            return Ok(events);
        }

        [HttpPost]
        [Route("events")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CreateEvent([FromBody] EventRequestModel model)
        {
            await _calendarService.CreateEvent(model);

            return Ok();
        }

        [HttpPut]
        [Route("events/{eventId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateEvent([FromRoute] Guid eventId, [FromBody] EventRequestModel model)
        {
            await _calendarService.UpdateEvent(eventId, model);

            return Ok();
        }

        [HttpDelete]
        [Route("events/{eventId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteEvent([FromRoute] Guid eventId)
        {
            await _calendarService.DeleteEvent(eventId);

            return Ok();
        }

        [HttpPut]
        [Route("events/{eventId}/attendees")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CreateOrUpdateAttendees([FromRoute] Guid eventId,
            [FromBody] EventAttendeesRequestModel model)
        {
            await _calendarService.CreateOrUpdateEventAttendees(eventId, model);

            return Ok();
        }

        [HttpDelete]
        [Route("events/{eventId}/attendees/{personId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteAttendee([FromRoute] Guid eventId, [FromRoute] Guid personId)
        {
            await _calendarService.DeleteEventAttendee(eventId, personId);

            return Ok();
        }
    }
}