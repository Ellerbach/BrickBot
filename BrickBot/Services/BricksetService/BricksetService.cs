using BrickBot.BricksetService;
using BrickBot.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace BrickBot.Services.BricksetService
{
    [Serializable]
    public class BricksetServiceAPI : IBrickService
    {

        private string userHash;
        private string apikey = ConfigurationManager.AppSettings["bsapikey"];
        //private string username = ConfigurationManager.AppSettings["bsusername"];
        //private string password = ConfigurationManager.AppSettings["bspassword"];

        //private BricksetAPIv2SoapClient bsAPI;

        public bool isLogged { get; internal set; }

        public BricksetServiceAPI()
        {
            
            //userHash = bsAPI.login(apikey, username, password);
            //isLogged = true;
            //if (userHash.Contains("ERROR") || userHash.Contains("INVALIDKEY"))
            //{
            //    isLogged = false;
            //}
        }

        public BrickItem getSets(string number)
        {
            BricksetAPIv2SoapClient bsAPI = new BricksetAPIv2SoapClient();
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
            BricksetAPIv2SoapClient bsAPI = new BricksetAPIv2SoapClient();
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

        public bool CanGetSetInfo()
        {
            return true;
        }

        public bool CanGetPartInfo()
        {
            return false;
        }

        public bool CanGetInstructionsInfo()
        {
            return true;
        }

        public bool CanGetMinifigInfo()
        {
            return false;
        }

        public bool CanGetGearInfo()
        {
            return false;
        }

        public bool CanGetBookInfo()
        {
            return true;
        }

        public bool CanGetCatalogInfo()
        {
            return false;
        }

        public bool CanGetMOCInfo()
        {
            return false;
        }

        public BrickItem GetBrickInfo(string number, ItemType typedesc)
        {
            BrickItem ret = null;
            switch (typedesc)
            {
                case ItemType.Set:
                    ret = getSets(number);
                    if (ret == null)
                    {
                        number += "-1";
                        ret = getSets(number);

                    }
                    return ret;
                case ItemType.Instruction:
                    ret = getInstructions(number);
                    if (ret == null)
                    {
                        number += "-1";
                        ret = getInstructions(number);
                    }
                    return ret;
                case ItemType.Part:
                case ItemType.Book:
                case ItemType.Gear:
                case ItemType.Catalog:
                case ItemType.MOC:
                case ItemType.Minifig:
                default:
                    return null;
                    break;
            };
        }

        public ServiceProvider GetServiceProvider
        {
            get { return ServiceProvider.Brickset; }
        }
        public List<ItemType> GetSupportedInfo
        {
            get { return new List<ItemType>() { ItemType.Set, ItemType.Instruction }; }
        }
    }
}