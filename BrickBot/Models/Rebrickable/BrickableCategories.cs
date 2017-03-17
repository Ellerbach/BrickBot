using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrickBot.Models.Rebrickable
{
    public class BrickableCategories
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public Result[] results { get; set; }
    }

    public class Result
    {
        public int id { get; set; }
        public int? parent_id { get; set; }
        public string name { get; set; }
    }

}