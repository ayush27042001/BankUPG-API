using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class BusinessAddressDetail
{
    public int BusinessAddressDetailId { get; set; }

    public int Mid { get; set; }

    public string AddressLine1 { get; set; } = null!;

    public string? AddressLine2 { get; set; }

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string? Country { get; set; }

    public bool? HasDifferentOperatingAddress { get; set; }

    public string? OperatingAddressLine1 { get; set; }

    public string? OperatingAddressLine2 { get; set; }

    public string? OperatingCity { get; set; }

    public string? OperatingState { get; set; }

    public string? OperatingPostalCode { get; set; }

    public string? OperatingCountry { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
