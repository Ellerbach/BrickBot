using BrickBot.Models.Bricklink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrickBot.Models
{
    public interface IBrickService
    {
        bool CanGetSetInfo();
        bool CanGetPartInfo();
        bool CanGetInstructionsInfo();
        bool CanGetMinifigInfo();
        bool CanGetGearInfo();
        bool CanGetBookInfo();
        bool CanGetCatalogInfo();
        bool CanGetMOCInfo();
        BrickItem GetBrickInfo(string number, ItemType typedesc);
        ServiceProvider GetServiceProvider { get; }
        List<ItemType> GetSupportedInfo { get; }
    }
}