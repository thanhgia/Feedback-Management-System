using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FMSWebAdmin.Models.Dataset
{
    public class FMSMessage
    {
        public string[] registration_ids { get; set; }
        public FMSNoti notification { get; set; }
        public object data { get; set; }
    }
}
