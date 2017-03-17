using BrickBot.BricksetService;
using BrickBot.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace BrickBot.Services.BricksetService
{
    public class BricksetServiceAPI
    {

        private string userHash;
        private string apikey = ConfigurationManager.AppSettings["bsapikey"];
        //private string username = ConfigurationManager.AppSettings["bsusername"];
        //private string password = ConfigurationManager.AppSettings["bspassword"];

        private BricksetAPIv2SoapClient bsAPI; // = new BricksetAPIv2SoapClient("http://brickset.com/api/v2.asmx");

        public bool isLogged { get; internal set; }

        public BricksetServiceAPI()
        {
            bsAPI = new BricksetAPIv2SoapClient();
            //userHash = bsAPI.login(apikey, username, password);
            //isLogged = true;
            //if (userHash.Contains("ERROR") || userHash.Contains("INVALIDKEY"))
            //{
            //    isLogged = false;
            //}
        }

        public BrickItem getSets(string number)
        {
            var ret = bsAPI.getSets(apikey, "", "", "", "", $"{number}", "", "", "", "", "", "", "");
            if (ret == null)
                return null;
            BrickItem retitem = new BrickItem();
            retitem.Number = number;
            retitem.Name = ret[0].name;
            retitem.BrickService = ServiceProvider.Brickset;
            retitem.ItemType = ItemType.Set;
            retitem.Theme = ret[0].theme;
            retitem.ThumbnailUrl = ret[0].thumbnailURL;
            retitem.BrickURL = ret[0].bricksetURL;
            int years;
            int.TryParse(ret[0].year, out years);
            retitem.YearReleased = years;
            return retitem;

        }

        public BrickItem getInstructions(string number)
        {
            var ret = bsAPI.getSets(apikey, "", "", "", "", $"{number}", "", "", "", "", "", "", "");
            if (ret == null)
                return null;
            BrickItem retitem = new BrickItem();
            retitem.Number = number;
            retitem.Name = ret[0].name;
            if (ret[0].instructionsCount > 0)
            {
                var instruct = bsAPI.getInstructions(apikey, ret[0].setID);
                retitem.Instructions = new SetInstruction[ret[0].instructionsCount];
                for (int i = 0; i < instruct.Length; i++)
                {
                    retitem.Instructions[i] = new SetInstruction();
                    retitem.Instructions[i].Name = instruct[i].description;
                    retitem.Instructions[i].URL = instruct[i].URL;
                }
            }
            retitem.BrickService = ServiceProvider.Brickset;
            retitem.ItemType = ItemType.Set;
            retitem.Theme = ret[0].theme;
            retitem.ThumbnailUrl = ret[0].thumbnailURL;
            int years;
            int.TryParse(ret[0].year, out years);
            retitem.YearReleased = years;
            retitem.BrickURL = ret[0].bricksetURL;
            return retitem;

        }
    }
}