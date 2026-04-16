using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class Pepstatus
{
    public int PepstatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<SigningAuthorityDetail> SigningAuthorityDetails { get; set; } = new List<SigningAuthorityDetail>();
}
