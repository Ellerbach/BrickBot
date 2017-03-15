using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace BrickBot.Models.Bricklink
{
    public class PriceMain
    {
        public Meta meta { get; set; }
        [DataMember(Name = "data")]
        public PriceGuideItem data { get; set; }
    }

    public class PriceGuideItem
    {
        public Item item { get; set; }
        public string new_or_used { get; set; }
        public string currency_code { get; set; }
        public string min_price { get; set; }
        public string max_price { get; set; }
        public string avg_price { get; set; }
        public string qty_avg_price { get; set; }
        public int unit_quantity { get; set; }
        public int total_quantity { get; set; }
        public Price_Detail[] price_detail { get; set; }
    }

    public class Item
    {
        public string no { get; set; }
        public string type { get; set; }
    }

    public class Price_Detail
    {
        public int quantity { get; set; }
        public string unit_price { get; set; }
        public bool shipping_available { get; set; }
        public int qunatity { get; set; }
    }


}