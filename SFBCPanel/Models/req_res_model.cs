using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SFBCPanel.Models
{
   

    public class req_res_model
    {
        [Display(Name = "ID")]
        public string ID { get; set; }
        [Display(Name = "Request Data")]
        public string Request_Data { get; set; }
        [Display(Name = "Response Data")]
        public string Response_Data { get; set; }
        [Display(Name = "Connection Response")]
        public string CONNECTION_RESPONSE { get; set; }
        [Display(Name = "Request Date")]
        public string REQUEST_DATE { get; set; }
        [Display(Name = "REsponse Date")]
        public string RESPONSE_DATE { get; set; }

        public string TRAN_Data { get; set; }
        [Display(Name = "Biller Id")]
        public string Biller_ID { get; set; }
        [Display(Name = "Biller Name")]
        public string Biller_Name { get; set; }
        [Display(Name = "Biller Voucher Date")]
        public string BILLER_VOUCHER { get; set; }
        [Display(Name = " Biller Amount")]
        public string BILL_AMOUNT { get; set; }
        [Display(Name = "Biller Response")]
        public string BBL_BILLERRESPONSE { get; set; }
        [Display(Name = "Bank Reference")]
        public string BBL_BNKREFRENCE { get; set; }
        [Display(Name = "Biller Trace No")]
        public string BBL_SYS_TRACENO { get; set; }
        public String Biller { get; set; }

        public List<string> Billers { get; set; }

        [Display(Name = "Biller id")]
        public String bbl_id { get; set; }
        [Display(Name = "Transaction date")]
        public String bbl_trandate { get; set; }
        [Display(Name = "Biller name")]
        public String bil_name { get; set; }
        [Display(Name = "Voucher")]
        public String bbl_billervoucher { get; set; }
        [Display(Name = "Bill amount")]
        public String bbl_billamount { get; set; }
        [Display(Name = "Bank response")]
        public String bbl_bnkresponse { get; set; }
        [Display(Name = "Reversal status")]
        public String bbl_reversalstatus { get; set; }
        [Display(Name = "Trace number")]
        public String bbl_sys_traceno { get; set; }
        [Display(Name = "Customer name")]
        public String bbl_customername { get; set; }
        [Display(Name = "Biller response")]
        public String bbl_response { get; set; }

    }
}