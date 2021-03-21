using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SFBCPanel.Models
{
    public class CustomerAccounts
    {
     


        [Display(Name = "Customer ID")]
        public string USER_ID { get; set; }
        [Display(Name = "Customer Name")]
        public string USER_NAME { get; set; }
        [Display(Name = "Customer Account")]
        public string DEF_ACC { get; set; }
        [Display(Name = "Customer New Account")]
        public string ACC_NO { get; set; }
        public string ACC_NO1 { get; set; }
    }
}