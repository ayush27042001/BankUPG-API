using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreatePaymentLinkBulkUploadRequest
    {
        public string? BatchReferenceId { get; set; }

        [MaxLength(500)]
        public string? BatchDescription { get; set; }

        [MaxLength(500)]
        public string? FileName { get; set; }

        public bool SendEmail { get; set; }
        public bool SendSms { get; set; }

        public List<PaymentLinkDataCaptureFieldRequest>? CustomerDataCapture { get; set; }
    }

    public class ListPaymentLinkBulkUploadsRequest
    {
        public string? DateFilter { get; set; }    // today, yesterday, last7days, last30days, custom
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }        // All, Pending, Completed, Failed
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class CreatePaymentLinkBulkUploadFileRequest
    {
        [Required]
        [MaxLength(500)]
        public string FileName { get; set; } = null!;

        [MaxLength(1000)]
        public string? FilePath { get; set; }
    }
}
