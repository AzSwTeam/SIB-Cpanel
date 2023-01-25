using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SFBCPanel.Models
{
    public class CustSettreport
    {
        [Display(Name = "ID")]
        public string sbf_sett_id { get; set; }

        [Display(Name = "Create Date")]
        public string sbf_create_date { get; set; }
        [Display(Name = "Status")]
        public string sbf_sattus { get; set; }
        [Display(Name = "Number OF Record")]
        public string sbf_no_rec { get; set; }
        [Display(Name = "Number OF Settlement Record")]
        public string sbf_no_sett_rec { get; set; }
        [Display(Name = " Settlement File")]
        public string Settlement_File { get; set; }



        [Display(Name = "Advice Number")]
        public string sbt_advice_no { get; set; }
        [Display(Name = "Biller Name")]
        public string bil_name { get; set; }
        [Display(Name = "Transaction Type")]
        public string sbt_type { get; set; }
        [Display(Name = "Create Date")]
        public string sbt_create_date { get; set; }
        [Display(Name = "Settlement File Name")]
        public string sbt_sett_file { get; set; }

        [Display(Name = "Amount")]
        public string sbt_amount { get; set; }

        [Display(Name = "From Account")]
        public string sbt_source_account { get; set; }

        [Display(Name = "To Account")]
        public string sbt_destination_account { get; set; }

        [Display(Name = "CB Date")]
        public string sbt_destination_date { get; set; }
        [Display(Name = "CB FT")]
        public string sbt_authorization_number { get; set; }
        [Display(Name = "Status")]
        public string sbt_status { get; set; }
        [Display(Name = "Settlement Biller ID")]
        public string sbt_sett_biller_id { get; set; }
        [Display(Name = "Settlement Transaction Date")]
        public string sbt_sett_transaction_date { get; set; }
        [Display(Name = "Settlement Transaction ID")]
        public string sbt_sett_transaction_id { get; set; }
        [Display(Name = "Settlement Bank Refrence")]
        public string sbt_sett_bank_ref { get; set; }
        [Display(Name = "Settlement Trace Number")]
        public string sbt_sett_trace_number { get; set; }
        [Display(Name = "Settlement Transaction Refrence")]
        public string sbt_sett_transaction_ref { get; set; }
        [Display(Name = "Settlement Amount")]
        public string sbt_sett_amount { get; set; }
        [Display(Name = "Settlement Fees")]
        public string sbt_sett_fees { get; set; }
        [Display(Name = "Settlement Biller Response")]
        public string sbt_sett_biller_response { get; set; }
    }
}