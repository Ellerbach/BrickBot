using BrickBot.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace BrickBot.Services.Peeron
{
    [Serializable]
    public class PeeronService: IBrickService
    {
        public bool CanGetBookInfo()
        {
            return true;
        }

        public bool CanGetCatalogInfo()
        {
            return false;
        }

        public bool CanGetGearInfo()
        {
            return false;
        }

        public bool CanGetInstructionsInfo()
        {
            return true;
        }

        public bool CanGetMinifigInfo()
        {
            return true;
        }

        public bool CanGetMOCInfo()
        {
            return false;
        }

        public bool CanGetPartInfo()
        {
            return true;
        }

        public bool CanGetSetInfo()
        {
            return true;
        }

        public BrickItem GetBrickInfo(string number, ItemType typedesc)
        {
            switch (typedesc)
            {
                case ItemType.Minifig:
                case ItemType.Part:
                    //need to implement
                    return null;
                    break;
                case ItemType.Set:
                case ItemType.Book:
                case ItemType.Instruction:
                    return GetSetDetails(number);
                    break;
                case ItemType.Gear:
                case ItemType.Catalog:
                case ItemType.MOC:
                case ItemType.Other:
                default:
                    return null;
                    break;
            }
        }

        public ServiceProvider GetServiceProvider
        {
            get { return ServiceProvider.Peeron; }
        }
        public List<ItemType> GetSupportedInfo
        {
            get { return new List<ItemType>() { ItemType.Set, ItemType.Instruction, ItemType.Part, ItemType.Minifig, ItemType.Book }; }
        }

        public BrickItem GetSetDetails(string number)
        {
            string url = $"http://www.peeron.com/inv/sets/{number}";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            var pagecontent = reader.ReadToEnd();
            BrickItem retitem = new BrickItem();
            retitem.BrickURL = url;
            //search for MSRP
            const string magicstring = "MSRP:&nbsp;<b>$";
            var idx = pagecontent.IndexOf(magicstring);
            int endidx;
            if (idx > 0)
            {
                endidx = pagecontent.IndexOf("</b>", idx + magicstring.Length);
                var price = pagecontent.Substring(idx + magicstring.Length, endidx - idx - magicstring.Length);

                double pricedouble;
                if (Double.TryParse(price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out pricedouble))
                {
                    retitem.New = new PriceDetails();
                    retitem.New.Average = pricedouble;
                    retitem.New.Currency = "USD";
                }
            }
            //find theme
            string theme = pagecontent;
            theme = theme.Substring(theme.IndexOf(">Theme</a>"));
            const string magichref = "<a href=\"";
            for (int i = 0; i < 2; i++)
            {
                idx = theme.IndexOf(magichref);
                theme = theme.Substring(idx + magichref.Length);
            }
            idx = theme.IndexOf("\">");
            theme = theme.Substring(idx + 2);

            endidx = theme.IndexOf('<');
            theme = theme.Substring(0, endidx);
            retitem.Theme = theme;
            // Find year
            const string magicyear = "Year:";
            string yearstr = pagecontent;
            yearstr = yearstr.Substring(yearstr.IndexOf(magicyear));
            yearstr = yearstr.Substring(yearstr.IndexOf("\">") + 2);
            yearstr = yearstr.Substring(0, yearstr.IndexOf('<'));
            int year;
            int.TryParse(yearstr, out year);
            retitem.YearReleased = year;
            //find instructions
            string instruct = pagecontent;
            const string magicinstructions = "instructions?</strong>";
            const string magicend = "</ul>";
            string name;
            if (instruct.IndexOf(magicinstructions) > 0)
            {
                instruct = instruct.Substring(instruct.IndexOf(magicinstructions) + magicinstructions.Length);
                instruct = instruct.Substring(0, instruct.IndexOf(magicend));
                //find all the <a href
                name = instruct;
                int count = 0;
                do
                {
                    idx = name.IndexOf(magichref);
                    if (idx > 0)
                    {
                        count++;
                        name = name.Substring(idx + magichref.Length);
                    }
                } while (idx > 0);
                if (count > 0)
                {
                    retitem.Instructions = new SetInstruction[count];
                    for (int i = 0; i < count; i++)
                    {
                        retitem.Instructions[i] = new SetInstruction();
                        idx = instruct.IndexOf(magichref);
                        if (idx > 0)
                        {

                            instruct = instruct.Substring(idx + magichref.Length);
                            url = instruct.Substring(0, instruct.IndexOf('"'));
                            //check if it's a relative of abosulte
                            if (!url.ToLower().Contains("http"))
                            {
                                url = "http://www.peeron.com" + url;
                            }
                            retitem.Instructions[i].URL = url;
                            name = instruct.Substring(instruct.IndexOf('>') + 1);
                            name = name.Substring(0, name.IndexOf('<'));
                            retitem.Instructions[i].Name = name;
                        }

                    }
                }
            }
            //try to extract image thumbnail
            const string magicpicture = "id=\"setpic\"";
            url = pagecontent.Substring(pagecontent.IndexOf(magicpicture) + magicpicture.Length);
            url = url.Substring(url.IndexOf('"') + 1);
            url = url.Substring(0, url.IndexOf('"'));
            retitem.ThumbnailUrl = url;

            retitem.BrickService = ServiceProvider.Peeron;
            retitem.ItemType = ItemType.Set;
            retitem.Number = number;
            //try to extract the title
            const string magictitle = "id=\"settitle\">";
            name = pagecontent.Substring(pagecontent.IndexOf(magictitle) + magictitle.Length);
            name = name.Substring(name.IndexOf(':')+2);
            name = name.Substring(0, name.IndexOf('<'));
            retitem.Name = name;

            return retitem;
        }

        
    }
}