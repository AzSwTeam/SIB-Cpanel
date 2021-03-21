using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SFBCpanel.Models
{
    public class UserDetailsModel
    {
        public int user_id { set; get; }
        [Required(ErrorMessage = "Enter Username Please")]
        public string user_name { set; get; }
        public string user_log { set; get; }
        public string user_pwd { set; get; }
        public string user_email { set; get; }
        public string user_mobile { set; get; }
        public string user_fax { set; get; }
        public string user_address { set; get; }
        public string user_status { set; get; }
        public string defult_account { set; get; }
        public string last_login { set; get; }
        public string last_login_ip { set; get; }
        public int faild_login { set; get; }
        public string user_custid { set; get; }
        public string first_login { set; get; }
        public int category { set; get; }
        public string user_transfer { set; get; }
        public int role_id { set; get; }
        public string account { set; get; }
        public string active { set; get; }
        public string login_status { set; get; }
        public int wrong_password { set; get; }
        public string last_unssessful_login { set; get; }
        public string company_name { set; get; }
    }
}
