using System;
using System.Collections.Generic;

namespace BankUPG.SharedKernal.Responses
{
    public class IpWhitelistResponse
    {
        public int IpWhitelistId { get; set; }
        public int Mid { get; set; }
        public string? IpAddress { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class IpWhitelistStatusResponse
    {
        public bool IpWhitelistEnabled { get; set; }
        public int TotalIps { get; set; }
        public List<IpWhitelistResponse> IpAddresses { get; set; } = new();
    }
}
