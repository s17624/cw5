﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.DTOs.Requests
{
    public class Student
    {
        public string indexNumber { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime birthDate { get; set; }
        public string studies { get; set; }
    }
}
