using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SFBCPanel.Models
{
    public class UsersMangementViewModel
    {
        [Required(ErrorMessage = "Enter Username Please")]
        public String Username { get; set; }
        public String SuccessfulLogin { get; set; }
        public String FailedLogin { get; set; }

        public String IpAddress { get; set; }
        public String LoginTime { get; set; }
        public String UserPass { get; set; }
        public String UserLogin { get; set; }
        public String LoginStatus { get; set; }

        public String UserID { get; set; }
        public String userstatus { get; set; }
        public String category { get; set; }
    }
}