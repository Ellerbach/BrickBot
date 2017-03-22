using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrickBot.Models
{
    // to get the name as string use
    // Enum.GetName(typeof(TypeDescription), 3)
    // will return BOOK
    public enum ItemType
    {
        Minifig = 0, Part, Set, Book, Gear, Catalog, Instruction, MOC
    }
}