using System;

namespace BankUPG.SharedKernal.Responses
{
    public class PaymentLinkBulkUploadResponse
    {
        public DateTime? CreatedOn => CreatedDate;
        public long BulkUploadId { get; set; }
        public string? BatchID => BatchReferenceId;
        public string? BatchReferenceId { get; set; }
        public string? CreatorEmail { get; set; }
        public string? BatchDescription { get; set; }
        public int LinkCreated { get; set; }
        public int ActiveCount { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class PaymentLinkBulkUploadFileResponse
    {
        public long ID => FileId;
        public long FileId { get; set; }
        public string? FileName { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedOn => CreatedDate;
        public DateTime? CreatedDate { get; set; }
    }
}
