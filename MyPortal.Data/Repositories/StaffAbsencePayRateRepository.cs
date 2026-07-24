using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class StaffAbsencePayRateRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffAbsencePayRate>(factory, authorizationService), IStaffAbsencePayRateRepository;
