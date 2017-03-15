using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrickBot.Models
{
    public class PriceDetails
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Average { get; set; }
        public string Currency { get; set; }
    }
}