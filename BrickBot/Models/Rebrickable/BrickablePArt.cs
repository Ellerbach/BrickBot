using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrickBot.Models.Rebrickable
{
    public class BrickablePart
    {
        public string part_num { get; set; }
        public string name { get; set; }
        public int part_cat_id { get; set; }
        public int year_from { get; set; }
        public int year_to { get; set; }
        public string part_url { get; set; }
        public string part_img_url { get; set; }
        public object[] prints { get; set; }
        public object[] molds { get; set; }
        public object[] alternates { get; set; }
        public External_Ids external_ids { get; set; }
    }

    public class External_Ids
    {
        public string[] BrickOwl { get; set; }
    }

}