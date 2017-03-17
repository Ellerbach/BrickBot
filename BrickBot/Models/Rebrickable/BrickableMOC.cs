using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrickBot.Models.Rebrickable
{
    public class BrickableMOC
    {
        public string set_num { get; set; }
        public string name { get; set; }
        public int year { get; set; }
        public int theme_id { get; set; }
        public int num_parts { get; set; }
        public string moc_img_url { get; set; }
        public string moc_url { get; set; }
        public string designer_name { get; set; }
        public string designer_url { get; set; }
    }

}