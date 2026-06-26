using MyPortal.Contracts.Models;
using MyPortal.Core;
using MyPortal.Core.Interfaces;

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

    /// <summary>
    /// Active rows, deliberate order (DisplayOrder then Description), mapped to response models.
    /// Use for lookups whose entity carries a meaningful <see cref="IOrderedLookupEntity.DisplayOrder"/>.
    /// </summary>
    public static List<LookupResponse> ToOrderedLookups<T>(this IEnumerable<T> source)
        where T : LookupEntity, IOrderedLookupEntity
    {
        return source
            .Where(x => x.Active)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Description)
            .Select(x => x.ToResponseModel())
            .ToList();
    }

    /// <summary>Active rows ordered alphabetically by Description — the default for lookups
    /// (e.g. languages, nationalities) that have no curated display order.</summary>
    public static List<LookupResponse> ToAlphabeticalLookups<T>(this IEnumerable<T> source)
        where T : LookupEntity
    {
        return source
            .Where(x => x.Active)
            .OrderBy(x => x.Description)
            .Select(x => x.ToResponseModel())
            .ToList();
    }
}
