using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusyBeasAPI.Models
{
    public class ServiceModel
    {
        public int id { get; set; }
        public string serviceName { get; set; }
        public string serviceDescription { get; set; }
        public decimal pricePerHour { get; set; }
    }
}