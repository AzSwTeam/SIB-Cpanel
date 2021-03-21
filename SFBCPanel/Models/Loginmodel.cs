using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SFBCPanel.Models
{
    public class Loginmodel
    {
        [Required]
        public string Username { get; set; }
         [Required]
        public string Password { get; set; }

        public string ReturnURL { get; set; }
        

    }
    public class changepassword
    {

       [Required]
        public string OldPassword { get; set; }
         [Required]
        public string newPassword { get; set; }
         [Required]
        public string confrimPassword { get; set; }

    }

    public class Loginmodelresult
    {
        public bool Login { get; set; }
        public string lblconfirm { get; set; }
        public string UserId { get; set; }
        public string user_name { get; set; }
        public string user_branch { get; set; }
        public string login { get; set; }
        public string user_roleid { get; set; }
        public string user_log { get; set; }
        public string USER_LASTLOGIN { get; set; }
        public string status { get; set; }
        
    }
}