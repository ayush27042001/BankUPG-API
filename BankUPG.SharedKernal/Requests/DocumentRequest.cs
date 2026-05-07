using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class UploadDocumentRequest
    {
        [Required(ErrorMessage = "Document type is required")]
        public int DocumentTypeId { get; set; }

        public int? BusinessProofTypeId { get; set; }

        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; } = null!;
    }

    public class UpdateDocumentRequest
    {
        [Required(ErrorMessage = "Document upload ID is required")]
        public int DocumentUploadId { get; set; }

        [Required(ErrorMessage = "Document type is required")]
        public int DocumentTypeId { get; set; }

        public int? BusinessProofTypeId { get; set; }

        public IFormFile? File { get; set; }
    }

    public class GetDocumentRequest
    {
        [Required(ErrorMessage = "Document upload ID is required")]
        public int DocumentUploadId { get; set; }
    }

    public class GetDocumentsByTypeRequest
    {
        [Required(ErrorMessage = "Document type is required")]
        public int DocumentTypeId { get; set; }
    }
}
