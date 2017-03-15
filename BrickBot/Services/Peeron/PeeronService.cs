using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace BrickBot.Services.Peeron
{
    public class PeeronService
    {
        static public double GetSetRetailPrice(string number)
        {
            string url = $"http://www.peeron.com/inv/sets/{number}";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            var pagecontent = reader.ReadToEnd();
            //search for MSRP
            const string magicstring = "MSRP:&nbsp;<b>$";
            var idx = pagecontent.IndexOf(magicstring);
            if (idx > 0)
            {
                int endidx = pagecontent.IndexOf("</b>", idx + magicstring.Length);
                var price = pagecontent.Substring(idx + magicstring.Length, endidx - idx - magicstring.Length);

                double pricedouble;
                if (Double.TryParse(price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out pricedouble))
                    return pricedouble;
            }
            return 0;
        }
    }
}