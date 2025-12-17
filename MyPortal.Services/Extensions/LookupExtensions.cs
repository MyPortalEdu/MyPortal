using MyPortal.Contracts.Models;
using MyPortal.Core;

namespace MyPortal.Services.Extensions;

public static class LookupExtensions
{
    public static LookupResponse ToResponseModel(this LookupEntity entity)
    {
        return new LookupResponse
        {
            Id = entity.Id,
            Description = entity.Description,
        };
    }
}