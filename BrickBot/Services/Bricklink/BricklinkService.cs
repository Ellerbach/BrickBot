using BrickBot.Models;
using BrickBot.Models.Bricklink;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace BrickBot.Services.Bricklink
{
    [Serializable]
    public class BricklinkService : IBrickService
    {

        public static ColorItem[] coloritems = GetColor();
        public static Category[] categories = GetCategories();

        public bool isAuthenticated { get; internal set; }

        private const int retry = 3;


        public BrickItem GetCatalogItem(string number, TypeDescription typedesc = TypeDescription.SET)
        {
            try
            {
                string url = $"https://api.bricklink.com/api/store/v1/items/{typedesc.ToString()}/{number}";
                for (int i = 0; i < retry; i++)
                {
                    var strret = ExecuteRequest(url);
                    var retobjdeser = Newtonsoft.Json.JsonConvert.DeserializeObject<CatalogMain>(strret);
                    Debug.WriteLine($"{retobjdeser.meta.message}, {retobjdeser.meta.description}, {retobjdeser.meta.code}");
                    if (retobjdeser.meta.message == "OK")
                    {
                        BrickItem retset = new BrickItem();
                        //crap but ok as enum are aligned and not searching for other elements than the one which can match
                        retset.ItemType = (ItemType)typedesc;
                        retset.Number = retobjdeser.data.no;
                        retset.Name = retobjdeser.data.name;
                        retset.ThumbnailUrl = retobjdeser.data.thumbnail_url;
                        switch (typedesc)
                        {
                            case TypeDescription.MINIFIG:
                                retset.BrickURL = "https://www.bricklink.com/v2/catalog/catalogitem.page?M=" + retset.Number;
                                break;
                            case TypeDescription.PART:
                                retset.BrickURL = "https://www.bricklink.com/v2/catalog/catalogitem.page?P=" + retset.Number;
                                break;
                            case TypeDescription.SET:
                                retset.BrickURL = "https://www.bricklink.com/v2/catalog/catalogitem.page?S=" + retset.Number;
                                break;
                            case TypeDescription.BOOK:
                                retset.BrickURL = "https://www.bricklink.com/v2/catalog/catalogitem.page?B=" + retset.Number;
                                break;
                            case TypeDescription.GEAR:
                                retset.BrickURL = "https://www.bricklink.com/v2/catalog/catalogitem.page?G=" + retset.Number;
                                break;
                            case TypeDescription.CATALOG:
                                retset.BrickURL = "https://www.bricklink.com/v2/catalog/catalogitem.page?C=" + retset.Number;
                                break;
                            case TypeDescription.INSTRUCTION:
                            case TypeDescription.UNSORTED_LOT:
                            case TypeDescription.ORIGINAL_BOX:
                            default:
                                retset.BrickURL = "https://www.bricklink.com/v2/search.page?q=" + retset.Number;
                                break;
                        }
                        //clean URL, sometimes it comes without the http
                        if (retset.ThumbnailUrl.Length > 0)
                            if (retset.ThumbnailUrl.IndexOf("http", StringComparison.CurrentCultureIgnoreCase) < 0)
                            {
                                retset.ThumbnailUrl = "http:" + retset.ThumbnailUrl;
                            }
                        //need to implement category names
                        try
                        {
                            retset.Theme = categories.Where(x => x.category_id == retobjdeser.data.category_id).First().category_name;

                        }
                        catch (Exception)
                        {
                            retset.Theme = "";
                        }
                        //get all the prices details
                        PriceGuideItem retPrice = GetPriceGuide(retobjdeser.data.no, typedesc, false);
                        double retail_price;
                        retset.New = new PriceDetails();
                        Double.TryParse(retPrice?.avg_price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out retail_price);
                        retset.New.Average = retail_price;
                        Double.TryParse(retPrice?.min_price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out retail_price);
                        retset.New.Min = retail_price;
                        Double.TryParse(retPrice?.max_price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out retail_price);
                        retset.New.Max = retail_price;
                        // so far only USD, need to implement other currencies
                        retset.New.Currency = "USD";
                        retPrice = GetPriceGuide(retobjdeser.data.no, typedesc, true);
                        retset.Used = new PriceDetails();
                        Double.TryParse(retPrice?.avg_price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out retail_price);
                        retset.Used.Average = retail_price;
                        Double.TryParse(retPrice?.min_price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out retail_price);
                        retset.Used.Min = retail_price;
                        Double.TryParse(retPrice?.max_price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out retail_price);
                        retset.Used.Max = retail_price;
                        retset.Used.Currency = "USD";
                        retset.YearReleased = retobjdeser.data.year_released;
                        retset.BrickService = ServiceProvider.Bricklink;
                        return retset;
                    }
                }
                return null;
            }
            catch (Exception)
            { return null; }
        }

        public SubSetDetail[] GetSubSets(string number, TypeDescription typedesc = TypeDescription.SET)
        {
            try
            {
                // / items /{ type}/{ no}/ subsets
                string url = $"https://api.bricklink.com/api/store/v1/items/{typedesc.ToString()}/{number}/subsets";
                for (int i = 0; i < retry; i++)
                {
                    var strret = ExecuteRequest(url);
                    var retobjdeser = Newtonsoft.Json.JsonConvert.DeserializeObject<SubSetMain>(strret);
                    Debug.WriteLine($"{retobjdeser.meta.message}, {retobjdeser.meta.description}, {retobjdeser.meta.code}");
                    if (retobjdeser.meta.message == "OK")
                    {
                        List<SubSetDetail> subdt = new List<SubSetDetail>();
                        foreach (var dt in retobjdeser.data)
                            try
                            {
                                //subdt.Add(dt.entries[dt.match_no]);
                                subdt.AddRange(dt.entries);
                            }
                            catch (Exception)
                            { }
                        return subdt.ToArray();
                    }
                }
                return null;
            }
            catch (Exception)
            { return null; }
        }

        public PriceGuideItem GetPriceGuide(string number, TypeDescription typedesc = TypeDescription.SET, bool New_Used = true)
        {
            try
            {
                string url = $"https://api.bricklink.com/api/store/v1/items/{typedesc.ToString()}/{number}/price";
                WebParameterCollection param = new WebParameterCollection();
                param.Add("currency_code", "EUR");
                if (New_Used)
                    param.Add("new_or_used", "N");
                else
                    param.Add("new_or_used", "U");
                for (int i = 0; i < retry; i++)
                {
                    var strret = ExecuteRequest(url, param);
                    var retobjdeser = Newtonsoft.Json.JsonConvert.DeserializeObject<PriceMain>(strret);
                    Debug.WriteLine($"{retobjdeser.meta.message}, {retobjdeser.meta.description}, {retobjdeser.meta.code}");
                    if (retobjdeser.meta.message == "OK")
                        return retobjdeser.data;
                }
                return null;
            }
            catch (Exception)
            { return null; }
        }

        static public ColorItem[] GetColor(int color = -1)
        {
            try
            {
                string url = $"https://api.bricklink.com/api/store/v1/colors";
                if (color >= 0)
                    url += "/" + color.ToString();
                //WebParameterCollection param = new WebParameterCollection();
                for (int i = 0; i < retry; i++)
                {
                    var strret = ExecuteRequest(url);
                    var retobjdeser = Newtonsoft.Json.JsonConvert.DeserializeObject<ColorItemMain>(strret);
                    Debug.WriteLine($"{retobjdeser.meta.message}, {retobjdeser.meta.description}, {retobjdeser.meta.code}");
                    if (retobjdeser.meta.message == "OK")
                        return retobjdeser.data;
                    //System.Threading.Thread.Sleep(1000);
                }
                return null;
            }
            catch (Exception)
            { return null; }
        }

        static public Category[] GetCategories(int categorie = -1)
        {
            try
            {
                string url = $"https://api.bricklink.com/api/store/v1/categories";
                if (categorie >= 0)
                    url += "/" + categorie.ToString();
                //WebParameterCollection param = new WebParameterCollection();
                for (int i = 0; i < retry; i++)
                {
                    var strret = ExecuteRequest(url);
                    var retobjdeser = Newtonsoft.Json.JsonConvert.DeserializeObject<CategoryMain>(strret);
                    Debug.WriteLine($"{retobjdeser.meta.message}, {retobjdeser.meta.description}, {retobjdeser.meta.code}");
                    if (retobjdeser.meta.message == "OK")
                        return retobjdeser.data;
                }
                return null;
            }
            catch (Exception)
            { return null; }
        }

        static private string ExecuteRequest(string url, WebParameterCollection param = null)
        {
            string req = url;
            if (param != null)
            {
                req += '?' + OAuthHelpers.Concatenate(param, "=", "&");
            }
            Debug.WriteLine($"{req}");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(req);
            request.Headers.Add("Authorization", OAuthHelpers.GetAuthorizationToken(url, param));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            return reader.ReadToEnd();
        }

        public bool CanGetSetInfo()
        {
            return true;
        }

        public bool CanGetPartInfo()
        {
            return true;
        }

        public bool CanGetInstructionsInfo()
        {
            return false;
        }

        public bool CanGetMinifigInfo()
        {
            return true;
        }

        public bool CanGetGearInfo()
        {
            return true;
        }

        public bool CanGetBookInfo()
        {
            return true;
        }

        public bool CanGetCatalogInfo()
        {
            return true;
        }

        public bool CanGetMOCInfo()
        {
            return false;
        }

        public BrickItem GetBrickInfo(string number, ItemType typedesc)
        {
            switch (typedesc)
            {
                case ItemType.Set:
                    var ret = GetCatalogItem(number, TypeDescription.SET);
                    if (ret == null)
                    {
                        number += "-1";
                        ret = GetCatalogItem(number, TypeDescription.SET);
                    }
                    return ret;
                case ItemType.Minifig:
                case ItemType.Part:
                case ItemType.Book:
                case ItemType.Gear:
                case ItemType.Catalog:
                    return GetCatalogItem(number, (TypeDescription)typedesc);
                case ItemType.Instruction:
                case ItemType.MOC:
                default:
                    return null;
                    break;
            }
        }

        public ServiceProvider GetServiceProvider
        {
            get { return ServiceProvider.Bricklink; }
        }

        public List<ItemType> GetSupportedInfo
        {
            get { return new List<ItemType>() { ItemType.Set, ItemType.Part, ItemType.Book, ItemType.Gear, ItemType.Minifig, ItemType.Catalog }; }
        }
    }
}