using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public record RegisterOrUpdateUserRequestDTO(
           string FirstName,
           string LastName,
           //[Required] string Name,
           [Required] string Username,
           [DataType(DataType.EmailAddress)] string? Email,
           [Required][DataType(DataType.Password)] string Password,
           //string Phone,
           [Required] int UserId,
           int CanSignRpt,
           int ShowNameOnRpt,
           int RelevantStaffId,
           int DiscountLimit,
           int MinCash,
           int EmpNo,
           string? Qualification,
           string ReportNote,
           string? DoctorStamp,
           int RegLocId,
           [Required] List<int> LocationIDs,
           [Required] List<int> Roles
       )
    {
        // Parameterless constructor for serialization or other purposes
        public RegisterOrUpdateUserRequestDTO() : this(
            string.Empty, string.Empty, string.Empty, null, string.Empty, 0, 0, 0, 0, 0, 0, 0, string.Empty, string.Empty, string.Empty, 0, new List<int>(), new List<int>())
        {
        }
    }
}
