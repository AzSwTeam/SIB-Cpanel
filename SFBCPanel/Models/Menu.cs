using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SFBCPanel.Models
{
    public class Menu
    {
        public int MID;
        public string MenuName;
        public string MenuURL;
        public int MenuParentID;
        public int subMenuParentID;
        public string MenuIMG;
    }
}