using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace BrickBot.Models.Bricklink
{

    public class CategoryMain
    {
        public Meta meta { get; set; }
        [DataMember(Name = "data")]
        public Category[] data { get; set; }
    }

    public class Category
    {
        public int category_id { get; set; }
        public string category_name { get; set; }
        public int parent_id { get; set; }
    }

}