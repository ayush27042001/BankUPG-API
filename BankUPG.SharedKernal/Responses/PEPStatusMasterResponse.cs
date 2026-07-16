using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace BankUPG.SharedKernal.Responses
{
    public class PEPStatusMasterResponse
    {
        public int PepstatusId { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
