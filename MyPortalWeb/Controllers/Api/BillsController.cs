using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Enums;
using MyPortal.Logic.Attributes;
using MyPortal.Logic.Constants;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Finance;

namespace MyPortalWeb.Controllers.Api
{
    [Route("api/bills")]
    public class BillsController : ControllerBase
    {
        private readonly IBillService _billService;

        public BillsController(IBillService billService)
        {
            _billService = billService;
        }

        [HttpGet]
        [Route("drafts/{chargeBillingPeriodId}")]
        [Authorize(Policy = Policies.UserType.Staff)]
        [Permission(PermissionValue.FinanceEditBills)]
        [ProducesResponseType(typeof(IEnumerable<BillModel>), 200)]
        public async Task<IActionResult> GenerateChargeBills([FromRoute] Guid chargeBillingPeriodId)
        {
            var generatedBills = await _billService.GenerateChargeBills(chargeBillingPeriodId);

            return Ok(generatedBills);
        }
    }
}