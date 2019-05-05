﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusyBeasAPI.Models
{
    public class UserModel
    {
        public int id { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public bool? isAdmin { get; set; }

    }
}