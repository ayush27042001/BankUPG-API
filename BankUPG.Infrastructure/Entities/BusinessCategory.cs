using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class BusinessCategory
{
    public int BusinessCategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string CategoryCode { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<BusinessSubCategory> BusinessSubCategories { get; set; } = new List<BusinessSubCategory>();

    public virtual ICollection<Merchant> Merchants { get; set; } = new List<Merchant>();
}
