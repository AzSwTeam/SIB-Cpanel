using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SFBCPanel.Models
{
    public class custinfo
    {
        public List<SelectListItem> Profiles { get; set; }
        public string profileCode { get; set; }
        public string user_id { get; set; }
        [Display(Name = "Customer Name")]
        public string user_name { get; set; }
        [Display(Name = "Customer Username")]
        public string user_log { get; set; }
        public string user_pwd { get; set; }
        [Display(Name = "Customer Email")]
        public string user_email { get; set; }
        [Display(Name = "Customer Mobile")]
        public string user_mobile { get; set; }
        [Display(Name = "Customer Address")]
        public string user_adrs { get; set; }
        [Display(Name = "Customer Profile")]
        public string name { get; set; }
        [Display(Name = "Customer Status")]
        public string status { get; set; }
        public string lblconfirm { get; set; }
        public string catgory { get; set; }
        public List<channel> Channels { get; set; }
        public String[] SelectedChannelsID { get; set; }
        [Display(Name = "Channel")]
        public String Channel { get; set; }
        public List<channel> SelectedChannels { get; set; }
    }
}