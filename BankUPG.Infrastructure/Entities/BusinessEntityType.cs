using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class BusinessEntityType
{
    public int BusinessEntityTypeId { get; set; }

    public string EntityName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<Merchant> Merchants { get; set; } = new List<Merchant>();
}
