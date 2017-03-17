using BrickBot.Models;
using BrickBot.Models.Rebrickable;
using BrickBot.Services.Bricklink;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace BrickBot.Services.Rebrickable
{
    public class RebrickableService
    {
        public static string Apikey { get; set; }

        private static Result[] Categories;

        public RebrickableService(string apikey)
        {
            Apikey = apikey;
            if (Categories == null)
                Categories = GetThemes();
        }

        public RebrickableService()
        {
            Apikey = ConfigurationManager.AppSettings["rebrickableapikey"];
            if (Categories == null)
                Categories = GetThemes();
        }

        public BrickItem GetSetInfo(string number)
        {
            BrickItem retitem = new BrickItem();

            string url = $"https://rebrickable.com/api/v3/lego/sets/{number}/";
            var strret = ExecuteRequest(url);
            var retobjdeser = Newtonsoft.Json.JsonConvert.DeserializeObject<BrickableSet>(strret);
            if (retobjdeser == null)
                return null;
            retitem.Name = retobjdeser.name;
            retitem.Number = retobjdeser.set_num;
            retitem.ThumbnailUrl = retobjdeser.set_img_url;
            retitem.ItemType = ItemType.Set;
            retitem.YearReleased = retobjdeser.year;
            retitem.Theme = Categories.Where(x =>x.id ==retobjdeser.theme_id).First().name;
            retitem.BrickURL = retobjdeser.set_url;
            retitem.BrickService = ServiceProvider.Rebrickable;
            return retitem;

        }

        public BrickItem GetMOCInfo(string number)
        {
            BrickItem retitem = new BrickItem();

            string url = $"https://rebrickable.com/api/v3/lego/mocs/{number}/";
            var strret = ExecuteRequest(url);
            var retobjdeser = Newtonsoft.Json.JsonConvert.DeserializeObject<BrickableMOC>(strret);
            if (retobjdeser == null)
                return null;
            retitem.Name = retobjdeser.name;
            retitem.Number = retobjdeser.set_num;
            retitem.ThumbnailUrl = retobjdeser.moc_img_url;
            retitem.ItemType = ItemType.MOC;
            retitem.YearReleased = retobjdeser.year;
            retitem.Theme = Categories.Where(x => x.id == retobjdeser.theme_id).First().name;
            retitem.BrickURL = retobjdeser.moc_url;
            retitem.BrickService = ServiceProvider.Rebrickable;
            return retitem;
        }

        public BrickItem GetPartInfo(string number)
        {
            BrickItem retitem = new BrickItem();

            string url = $"https://rebrickable.com/api/v3/lego/parts/{number}/";
            var strret = ExecuteRequest(url);
            var retobjdeser = Newtonsoft.Json.JsonConvert.DeserializeObject<BrickablePart>(strret);
            if (retobjdeser == null)
                return null;
            retitem.Name = retobjdeser.name;
            retitem.Number = retobjdeser.part_num;
            retitem.ThumbnailUrl = retobjdeser.part_img_url;
            retitem.ItemType = ItemType.MOC;
            retitem.YearReleased = retobjdeser.year_from;
            retitem.Theme = Categories.Where(x => x.id == retobjdeser.part_cat_id).First().name;
            retitem.BrickURL = retobjdeser.part_url;
            retitem.BrickService = ServiceProvider.Rebrickable;
            return retitem;
        }

        public Result[] GetThemes()
        {
            string url = $"https://rebrickable.com/api/v3/lego/themes/";
            var strret = ExecuteRequest(url);
            var retobjdeser = Newtonsoft.Json.JsonConvert.DeserializeObject<BrickableCategories>(strret);
            return retobjdeser.results;
        }

        static private string ExecuteRequest(string url, WebParameterCollection param = null)
        {
            string req = url;
            req += "?key=" + Apikey;
            if (param != null)
            {
                req += '&' + OAuthHelpers.Concatenate(param, "=", "&");
            }
            Debug.WriteLine($"{req}");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(req);
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                return reader.ReadToEnd();
            }
            catch (Exception)
            {

                return "";
            }
            
        }
    }
}