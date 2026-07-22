using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class DietaryRequirementRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<DietaryRequirement>(factory, authorizationService), IDietaryRequirementRepository;
