using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.Attendance;

public class SubmitRegisterRequest
{
    [Required]
    public IList<SubmitMarkRequest> Marks { get; set; } = new List<SubmitMarkRequest>();
}
