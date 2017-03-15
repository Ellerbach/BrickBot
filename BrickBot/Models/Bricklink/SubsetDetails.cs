using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace BrickBot.Models.Bricklink
{

    public class SubSetMain
    {
        public Meta meta { get; set; }
        [DataMember(Name = "data")]
        public SubSet[] data { get; set; }
    }

    public class SubSet
    {
        public int match_no { get; set; }
        [DataMember(Name = "entries")]
        public SubSetDetail[] entries { get; set; }
    }

    public class SubSetDetail
    {
        public Item item { get; set; }
        public int color_id { get; set; }
        public int quantity { get; set; }
        public int extra_quantity { get; set; }
        public bool is_alternate { get; set; }
        public bool is_counterpart { get; set; }
    }
}