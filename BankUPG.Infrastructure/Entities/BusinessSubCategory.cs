using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class BusinessSubCategory
{
    public int BusinessSubCategoryId { get; set; }

    public int BusinessCategoryId { get; set; }

    public string SubCategoryName { get; set; } = null!;

    public string SubCategoryCode { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual BusinessCategory BusinessCategory { get; set; } = null!;

    public virtual ICollection<Merchant> Merchants { get; set; } = new List<Merchant>();
}
