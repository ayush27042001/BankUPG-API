using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class SigningAuthorityDetail
{
    public int SigningAuthorityDetailId { get; set; }

    public int Mid { get; set; }

    public string SigningAuthorityName { get; set; } = null!;

    public string SigningAuthorityEmail { get; set; } = null!;

    public string SigningAuthorityPan { get; set; } = null!;

    public int PepstatusId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual Pepstatus Pepstatus { get; set; } = null!;
}
