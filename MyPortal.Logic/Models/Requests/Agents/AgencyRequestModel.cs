using System;
using System.ComponentModel.DataAnnotations;

namespace MyPortal.Logic.Models.Requests.Agents;

public class AgencyRequestModel
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; }
    public Guid AgencyTypeId { get; set; }
    
    [Url]
    [StringLength(100)]
    public string Website { get; set; }
}