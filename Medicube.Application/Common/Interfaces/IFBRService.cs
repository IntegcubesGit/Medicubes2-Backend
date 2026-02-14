using Domain.DTOs;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IFBRService
    {
        Task<object> SubmitInvoiceToFBR(FBRInvoiceSubmissionDTO dto);
    }
}
