using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreatePEPStatusMasterRequest
    {
        [Required]
        [StringLength(100)]
        public string StatusName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }
    }
}
