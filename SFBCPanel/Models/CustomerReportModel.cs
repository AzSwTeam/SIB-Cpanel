using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SFBCPanel.Models
{
    public class CustomerReportModel
    {
        public List<SelectListItem> Branches { get; set; }
        /*public List<SelectListItem> AccTypes { get; set; }*/
        /*public List<SelectListItem> Currencies { get; set; }*/
        public List<SelectListItem> catgories { get; set; }
        public List<SelectListItem> CustomerStatus { get; set; }

        public List<SelectListItem> admins { get; set; }

        public string CustomerName { get; set; }

        public string CustomerLog { get; set; }
        public string Email { get; set; }
        public string mobile { get; set; }

        public string address { get; set; }
        public string created_by { get; set; }

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

        public String StatusCode { get; set; }
        [Display(Name = "Account Currency")]
        public string Currency { get; set; }


        [Display(Name = "Account Type")]
        public string AccountType { get; set; }


        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }
    }
}