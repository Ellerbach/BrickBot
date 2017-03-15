using BrickBot.BricksetService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace BrickBot.Services.BricksetService
{
    public class BricksetService
    {

        private string userHash;
        private string apikey = ConfigurationManager.AppSettings["bsapikey"];
        //private string username = ConfigurationManager.AppSettings["bsusername"];
        //private string password = ConfigurationManager.AppSettings["bspassword"];

        private BricksetAPIv2SoapClient bsAPI; // = new BricksetAPIv2SoapClient("http://brickset.com/api/v2.asmx");

        public bool isLogged { get; internal set; }

        public BricksetService()
        {
            bsAPI = new BricksetAPIv2SoapClient();
            //userHash = bsAPI.login(apikey, username, password);
            //isLogged = true;
            //if (userHash.Contains("ERROR") || userHash.Contains("INVALIDKEY"))
            //{
            //    isLogged = false;
            //}
        }

        public sets[] getSets(string number)
        {
            return bsAPI.getSets(apikey, "", "", "", "", $"{number}", "", "", "", "", "", "", "");


        }
    }
}