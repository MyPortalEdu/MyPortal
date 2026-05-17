using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.People;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.People
{
    public class PersonService : BaseService, IPersonService
    {
        private readonly IPersonRepository _personRepository;

        public PersonService(IAuthorizationService authorizationService, ILogger<PersonService> logger,
            IPersonRepository personRepository) : base(authorizationService, logger)
        {
            _personRepository = personRepository;
        }

        public async Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
            SortOptions? sort = null, PageOptions? paging = null,
            CancellationToken cancellationToken = default)
        {
            // Same gate as the school-details edit screen the picker is consumed by; if
            // we add other staff-pickers in different contexts later, broaden the gate
            // (or move it onto the controller) at that point.
            await AuthorizationService.RequirePermissionAsync(Permissions.Agencies.ViewAgencies, cancellationToken);

            return await _personRepository.GetStaffMembersAsync(filter, sort, paging, cancellationToken);
        }
    }
}
