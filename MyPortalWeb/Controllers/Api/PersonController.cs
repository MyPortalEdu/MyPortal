using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Models.Search;
using MyPortal.Logic.Constants;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.People;

namespace MyPortalWeb.Controllers.Api
{
    [Authorize]
    public class PersonController : ControllerBase
    {
        private readonly IPersonService _personService;
        
        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }

        [HttpGet]
        [Authorize(Policy = Policies.UserType.Staff)]
        [Route("api/people")]
        [ProducesResponseType(typeof(IEnumerable<PersonSearchResultModel>), 200)]
        public async Task<IActionResult> SearchPeople([FromQuery] PersonSearchOptions searchModel)
        {
            var people = await _personService.GetPeopleWithTypes(searchModel);

            return Ok(people);
        }

        [HttpGet]
        [Route("api/users/{userId}/person")]
        [ProducesResponseType(typeof(PersonModel), 200)]
        public async Task<IActionResult> GetPersonByUser([FromRoute] Guid userId)
        {
            var person = await _personService.GetPersonByUserId(userId, false);

            return Ok(person);
        }
    }
}