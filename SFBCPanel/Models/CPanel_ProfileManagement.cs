using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SFBCPanel.Models
{
    //public class pageparameter
    //{
    //    public String menuid { get; set; }
    //    [Display(Name = "MenuItems EN-Name")]
    //    public String menuname { get; set; }
    //    [Display(Name = "MenuItem AR-Name")]
    //    public String menuname_ar { get; set; }
    //    [Display(Name = "MainMenu EN-Name")]
    //    public String Parent_menuname { get; set; }

    //    [Display(Name = "MainMenu AR-Name")]
    //    public String Parent_menuname_ar { get; set; }

    //    public String menuparentid { get; set; }

    //    public bool IsSelected { get; set; }
    //}

    public class CPanel_ProfileManagement
    {//select menuid,menuname ,menuparentid from tbl_menumaster where menu_category='1'
        public List<pageparameter> pages { get; set; }
        public int[] pagesid { get; set; }
        public List<SelectListItem> catgories { get; set; }

        [Required]
        public String menu_category { get; set; }

        public List<profilesparameter> profiles { get; set; }
        public int[] profilesid { get; set; }

        [Required]
        [Display(Name = "Profile Name  ")]
        public String profilename { get; set; }


        [Display(Name = "Category")]
        public String category { get; set; }

    }

    //public class profilesparameter
    //{

    //    [Display(Name = "Profile Name  ")]
    //    public String profilename { get; set; }
    //    [Display(Name = "Profile Status  ")]
    //    public String profilestatus { get; set; }
    //    public String profielid { get; set; }
    //    public bool IsSelected { get; set; }

    //}
}