using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace BrickBot.Models.Bricklink
{
    // to get the name as string use
    // Enum.GetName(typeof(TypeDescription), 3)
    // will return BOOK
    public enum TypeDescription
    {
        MINIFIG = 0, PART, SET, BOOK, GEAR, CATALOG, INSTRUCTION, UNSORTED_LOT, ORIGINAL_BOX
    }


    public class CatalogMain
    {
        public Meta meta { get; set; }
        [DataMember(Name = "data")]
        public CatalogItem data { get; set; }
    }

    public class Meta
    {
        public string description { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }
    //autre façon de faire    
    //[DataContract(Name = "data")]
    public class CatalogItem
    {
        public string no { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int category_id { get; set; }
        public string image_url { get; set; }
        public string thumbnail_url { get; set; }
        public string weight { get; set; }
        public string dim_x { get; set; }
        public string dim_y { get; set; }
        public string dim_z { get; set; }
        public int year_released { get; set; }
        public bool is_obsolete { get; set; }
    }



}