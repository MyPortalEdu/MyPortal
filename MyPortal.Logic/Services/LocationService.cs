﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.School;

namespace MyPortal.Logic.Services
{
    public sealed class LocationService : BaseService, ILocationService
    {
        public LocationService(ISessionUser user) : base(user)
        {
        }

        public async Task<IEnumerable<LocationModel>> GetLocations()
        {
            await using var unitOfWork = await User.GetConnection();

            var locations = await unitOfWork.Locations.GetAll();

            return locations.Select(l => new LocationModel(l));
        }
    }
}