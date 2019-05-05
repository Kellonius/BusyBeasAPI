using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusyBeasAPI.Models
{
    public class RequestedServicesModel
    {

        public int id { get; set; }
        public string serviceName { get; set; }
        public DateTime date { get; set; }
        public bool? fulfilled { get; set; }
        public decimal? hoursTaken { get; set; }
        public decimal? pricePerHour { get; set; }

    }
}