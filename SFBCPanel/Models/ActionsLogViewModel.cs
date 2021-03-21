using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SFBCPanel.Models
{
    public class ActionsLogViewModel
    {
        [Display(Name = "User id")]
        public string user_id { get; set; }
        [Display(Name = "Username")]
        public string user_name { get; set; }
        [Display(Name = "User Role")]
        public string user_role { get; set; }
        [Display(Name = "User Status")]
        public string user_status { get; set; }
        [Display(Name = "User Branch")]
        public string user_branch { get; set; }
        [Display(Name = "Action")]
        public string action { get; set; }
        [Display(Name = "Affected User")]
        public string action_on_user { get; set; }
        [Display(Name = "Date - Time")]
        public string timedate { get; set; }
    }
}