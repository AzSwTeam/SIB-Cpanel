using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SFBCPanel.Models
{
    public class resetpass
    {
 
   
        [Display(Name = "Customer Password")]
        public String pass { get; set; }

        [Display(Name = "Customer Name")]
        public String name { get; set; }
        [Required]
        [Display(Name = "Customer Account")]
        public String account { get; set; }

        [Display(Name = "Customer Branch")]
        public String branchname { get; set; }
        public String lblconfirm { get; set; }
    }
    public class Customerinfopass
    {
        public List<SelectListItem> Branches { get; set; }
        public List<SelectListItem> AccTypes { get; set; }
        public List<SelectListItem> Currencies { get; set; }
        public List<SelectListItem> catgories { get; set; }

        [Display(Name = "Customer Branch")]
        public string Branch { get; set; }
        //[Required]
        [Display(Name = "Account Currency")]
        public string Currency { get; set; }
        //[Required]
        [Display(Name = "Account Type")]
        public string AccountType { get; set; }
        [Required]
        [Display(Name = "Account Number")]

        public string AccountNumber { get; set; }

        [Display(Name = "SUBNO")]
        public String SUBNO { get; set; }

        [Display(Name = "SUBGL")]
        public String SUBGL { get; set; }

        [Required]
        public String BranchCode { get; set; }

        [Required]
        public String AccountTypecode { get; set; }

        [Required]
        public String CurrencyCode { get; set; }


        public string CustomerName { get; set; }

        public String CustomerID { get; set; }

        public String CategoryCode { get; set; }
        [Display(Name = "Category")]
        public String category { get; set; }
    }
   
}