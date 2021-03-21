using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SFBCPanel.Models
{
    public class CustomerAuthorization
    {
        [Display(Name = "Customer Name")]
        public String Customername { get; set; }
        [Display(Name = "Customer Account")]
        public String Customeraccount { get; set; }
          [Display(Name = "Customer ID")]
        public String CustomerID { get; set; }
    }


    public class CustomerAuthorizationinfo
    {
        [Display(Name = "Customer Branch")]
        public string Branch { get; set; }
        //[Required]
        [Display(Name = "Account Currency")]
        public string Currency { get; set; }
        //[Required]
        [Display(Name = "Account Type")]
        public string AccountType { get; set; }
        [Display(Name = "Customer Name")]
        public String Customername { get; set; }
        [Display(Name = "Customer Account")]
        public String Customeraccount { get; set; }
        [Display(Name = "Customer ID")]
        public String CustomerID { get; set; }
        [Display(Name = "Customer Email")]
        public string Email { get; set; }
        [Display(Name = "Customer Profile")]
        public string Profile { get; set; }
        [Display(Name = "Customer Username")]
        public string UserName { get; set; }

        [Display(Name = "Customer Address")]
        public string Address { get; set; }
        [Display(Name = "Customer Phone")]
        public string CustomerPhone { get; set; }
        [Display(Name = "Reject Reason")]
        public string RejectReason { get; set; }
        public string userid { get; set; }
        public string authsts   { get; set; }
        public string rjtsts   { get; set; }
    }
}