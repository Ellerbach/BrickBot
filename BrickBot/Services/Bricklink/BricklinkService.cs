﻿using BrickBot.Models;
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
    public class BricklinkService
    {

        public static ColorItem[] coloritems = GetColor();

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
                        //need to implement category names
                        retset.Theme = retobjdeser.data.category_id.ToString();
                        //get all the prices details
                        PriceGuideItem retPrice = GetPriceGuide(retobjdeser.data.no, typedesc, false);
                        double retail_price;
                        retset.New = new PriceDetails();
                        Double.TryParse(retPrice.avg_price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out retail_price);
                        retset.New.Average = retail_price;
                        Double.TryParse(retPrice.min_price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out retail_price);
                        retset.New.Min = retail_price;
                        Double.TryParse(retPrice.max_price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out retail_price);
                        retset.New.Max = retail_price;
                        // so far only USD, need to implement other currencies
                        retset.New.Currency = "USD";
                        retset.Used = new PriceDetails();
                        Double.TryParse(retPrice.avg_price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out retail_price);
                        retset.Used.Average = retail_price;
                        Double.TryParse(retPrice.min_price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out retail_price);
                        retset.Used.Min = retail_price;
                        Double.TryParse(retPrice.max_price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out retail_price);
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
    }
}