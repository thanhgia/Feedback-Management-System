using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FMSWebAdmin.Models
{
    public class ResponseModel
    {
        public int id { get; set; }
        public DateTime responseTime { get; set; }
        public string questionId { get; set; }
        public string responseDetail { get; set; }
        public string userAgent { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string status { get; set; }
        public string equipmentId { get; set; }
        public string typeOfQuestion { get; set; }
    }
}
