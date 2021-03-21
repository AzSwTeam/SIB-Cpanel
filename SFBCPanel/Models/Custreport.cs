using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SFBCPanel.Models
{
    public class Custreport
    {
        public List<SelectListItem> Branches { get; set; }
        /*public List<SelectListItem> AccTypes { get; set; }*/
        /*public List<SelectListItem> Currencies { get; set; }*/
        public List<SelectListItem> catgories { get; set; }
        public List<SelectListItem> CustomerStatus { get; set; }
        public List<SelectListItem> transactions_names { get; set; }
        public List<SelectListItem> transactions_statuses { get; set; }


        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Display(Name = "Customer ID")]
        public String CustomerID { get; set; }

        [Display(Name = "Customer Branch")]
        public string Branch { get; set; }

        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; }

        [Display(Name = "Customer Status")]
        public string CustStatus { get; set; }

        public String BranchCode { get; set; }
        public String CategoryCode { get; set; }
        public String TranName { get; set; }

        public String StatusCode { get; set; }
        [Display(Name = "Account Currency")]
        public string Currency { get; set; }

        [Display(Name = "Account Type")]
        public string AccountType { get; set; }


        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }

        public string customercreator { get; set; }
        public string customerfullname { get; set; }
        public string customeremail { get; set; }
        public string phonenumber { get; set; }
        public string address { get; set; }
        public string lastlogin { get; set; }
        public string lastip { get; set; }
        public string faildlogincount { get; set; }
        public string username { get; set; }
        public string category { get; set; }
        public string createdby { get; set; }
        public string user_email { get; set; }
        public string wrong_passwords { get; set; }
        public string channels { get; set; }
        public List<string> linkedAccounts { get; set; }
    }
}