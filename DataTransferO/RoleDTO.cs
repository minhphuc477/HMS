using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DataTransferO
{
    public class RoleDTO
    {
        public Guid RoleId { get; set; }
        public required string RoleName { get; set; }
    }
}
