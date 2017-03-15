using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrickBot.Models
{
    public class BrickItem
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Theme { get; set; }
        public string ThumbnailUrl { get; set; }
        public int YearReleased { get; set; }
        public string InstructionUrl { get; set; }
        public PriceDetails New { get; set; }
        public PriceDetails Used { get; set; }
        public string Color { get; set; }
        public ItemType ItemType { get; set; }
        public ServiceProvider BrickService { get; set; }
    }
}