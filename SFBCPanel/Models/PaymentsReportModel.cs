
using System.Collections.Generic;
using System.Web.Mvc;

namespace SFBCPanel.Models
{
    public class PaymentsReportModel
    {
        public List<SelectListItem> transactions_statuses { get; set; }
        public List<SelectListItem> Billers { get; set; }
        public string BillerId{ get; set; } = "0";
        public string transactions_statusesId { get; set; } = "0";
        public string SubBillersId { get; set; } = "0";
        public List<SelectListItem> SubBillers { get; set; }
        public string fromDate { get; set; }    
        public string toDate { get; set; }

        public string status { get; set; }
        public string message { get; set; }
        public string bil_message { get; set; }
        public string service_code { get; set; }

        public string account_no { get; set; }
        public string BILL_SUB_IB { get; set; }
        public string channelid { get; set; }
        public string VoucherRes { get; set; }

        public string tran_id { get; set; }

        public string TRAN_Data { get; set; }
        public string sub_tran_name { get; set; }
        public string BILL_AMOUNT { get; set; }
        public string BBL_BNKREFRENCE { get; set; }
        public string bnk_response { get; set; }
        public string biller_response { get; set; }



        public List<req_res_model> transactions { get; set; } = new List<req_res_model>();

    }
}
