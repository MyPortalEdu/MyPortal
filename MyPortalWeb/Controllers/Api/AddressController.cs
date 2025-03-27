using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Enums;
using MyPortal.Logic.Attributes;
using MyPortal.Logic.Enums;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Addresses;
using MyPortal.Logic.Models.Requests.Addresses;

namespace MyPortalWeb.Controllers.Api
{
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        [Route("api/addresses")]
        [Permission(PermissionRequirement.RequireAny, PermissionValue.PeopleEditContactDetails,
            PermissionValue.StudentEditStudentDetails, PermissionValue.PeopleEditAgentDetails,
            PermissionValue.PeopleEditStaffBasicDetails, PermissionValue.AgencyEditAgencies)]
        public async Task<IActionResult> GetExistingAddresses([FromQuery] AddressSearchRequestModel searchModel)
        {
            var addresses = await _addressService.GetMatchingAddresses(searchModel);

            return Ok(addresses);
        }

        [HttpPut]
        [Route("api/addresses/{addressId}")]
        [Permission(PermissionRequirement.RequireAny, PermissionValue.PeopleEditContactDetails,
            PermissionValue.StudentEditStudentDetails, PermissionValue.PeopleEditAgentDetails,
            PermissionValue.PeopleEditStaffBasicDetails, PermissionValue.AgencyEditAgencies)]
        public async Task<IActionResult> UpdateAddress([FromRoute] Guid addressId, AddressRequestModel model)
        {
            await _addressService.UpdateAddress(addressId, model);

            return Ok();
        }

        [HttpPost]
        [Route("api/people/{personId}/addresses")]
        [Permission(PermissionRequirement.RequireAny, PermissionValue.PeopleEditContactDetails,
            PermissionValue.StudentEditStudentDetails, PermissionValue.PeopleEditAgentDetails,
            PermissionValue.PeopleEditStaffBasicDetails)]
        public async Task<IActionResult> CreatePersonAddress([FromRoute] Guid personId,
            [FromBody] EntityAddressRequestModel personAddress)
        {
            await _addressService.CreateAddressForPerson(personId, personAddress);

            return Ok();
        }

        [HttpPut]
        [Route("api/people/{personId}/addresses/{addressLinkId}")]
        [Permission(PermissionRequirement.RequireAny, PermissionValue.PeopleEditContactDetails,
            PermissionValue.StudentEditStudentDetails, PermissionValue.PeopleEditAgentDetails,
            PermissionValue.PeopleEditStaffBasicDetails)]
        public async Task<IActionResult> UpdatePersonAddressLink([FromRoute] Guid addressPersonId,
            [FromBody] LinkAddressRequestModel addressLink)
        {
            await _addressService.UpdateAddressLinkForPerson(addressPersonId, addressLink);

            return Ok();
        }

        [HttpGet]
        [Route("api/people/{personId}/addresses")]
        [Permission(PermissionRequirement.RequireAny, PermissionValue.PeopleViewContactDetails,
            PermissionValue.StudentViewStudentDetails, PermissionValue.PeopleViewAgentDetails,
            PermissionValue.PeopleViewStaffBasicDetails)]
        [ProducesResponseType(typeof(IEnumerable<AddressLinkDataModel>), 200)]
        public async Task<IActionResult> GetAddressesByPerson([FromRoute] Guid personId)
        {
            var addresses = await _addressService.GetAddressLinksByPerson(personId);

            return Ok(addresses);
        }
    }
}