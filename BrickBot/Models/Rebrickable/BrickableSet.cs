using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrickBot.Models.Rebrickable
{
    public class BrickableSet
    {
        public string set_num { get; set; }
        public string name { get; set; }
        public int year { get; set; }
        public int theme_id { get; set; }
        public int num_parts { get; set; }
        public string set_img_url { get; set; }
        public string set_url { get; set; }
        public DateTime last_modified_dt { get; set; }
    }

}