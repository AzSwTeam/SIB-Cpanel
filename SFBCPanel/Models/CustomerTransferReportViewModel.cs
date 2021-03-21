using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SFBCPanel.Models
{
    public class CustomerTransferReportViewModel
    {
        public List<SelectListItem> Branches { get; set; }
        public List<SelectListItem> AccTypes { get; set; }
        public List<SelectListItem> Currencies { get; set; }
        public List<SelectListItem> catgories { get; set; }
        /*public List<channel> Channels { get; set; }*/

        public string SUBNO { get; set; }
        public string SUBGL { get; set; }
        [Display(Name = "Customer Branch")]
        public string Branch { get; set; }
        //[Required]
        [Display(Name = "Account Currency")]
        public string Currency { get; set; }
        //[Required]
        [Display(Name = "Account Type")]
        public string AccountType { get; set; }

        //[Required]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }

        [Display(Name = "Customer Card")]


        //[Required]
        public String BranchCode { get; set; }

        //[Required]
        public String AccountTypecode { get; set; }

        //[Required]
        public String CurrencyCode { get; set; }


        public string CustomerName { get; set; }

        public String CustomerID { get; set; }
        [Display(Name = "Transaction Date")]
        public String TranDate { get; set; }

        public String TranFullReq { get; set; }
        public String UserId { get; set; }

        public String TranFullResp { get; set; }
        public String TranName { get; set; }
        public String TranToken { get; set; }


        [Display(Name = "Transaction Response")]
        public String TranResult { get; set; }

        [Display(Name = "From Account")]
        public String TranFromAccount { get; set; }

        [Display(Name = "To Account")]
        public String TranToAccount { get; set; }

        [Display(Name = "Transfer Amount")]
        public String TranReqAmount { get; set; }


        [Display(Name = "Transfer Status")]
        public String TranStatus { get; set; }

        [Display(Name = "From Date")]
        public String FromDate { get; set; }
        [Display(Name = "To Date")]
        public String ToDate { get; set; }
        [Display(Name = "PAN")]
        public String PAN { get; set; }
        [Display(Name = "Customer Name")]
        public String Customername { get; set; }
        [Display(Name = "Response Status")]
        public String ResponseStatus { get; set; }
        [Display(Name = "RRN")]
        public String RRN { get; set; }
        [Display(Name = "FT")]
        public String FT { get; set; }

    }
}