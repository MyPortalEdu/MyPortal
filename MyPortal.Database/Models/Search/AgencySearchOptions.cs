using System;

namespace MyPortal.Database.Models.Search;

public class AgencySearchOptions
{
    public Guid? AgencyTypeId { get; set; }
    public string Name { get; set; }
}