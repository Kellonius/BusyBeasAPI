using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusyBeasAPI.Models
{
    public class AddressModel
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string streetOne { get; set; }
        public string streetTwo { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
    }
}