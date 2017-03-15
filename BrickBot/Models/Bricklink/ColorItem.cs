using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace BrickBot.Models.Bricklink
{
    public class ColorItemMain
    {
        public Meta meta { get; set; }
        [DataMember(Name = "data")]
        public ColorItem[] data { get; set; }
    }

    public class ColorItem
    {
        public int color_id { get; set; }
        public string color_name { get; set; }
        public string color_code { get; set; }
        public string color_type { get; set; }
    }

}