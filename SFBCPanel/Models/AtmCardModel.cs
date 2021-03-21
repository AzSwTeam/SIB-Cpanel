using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SFBCPanel.Models
{
    public class AtmCardModel
    {
        [Display(Name = "Customer Name")]
        public string name { get; set; }
        [Display(Name = "Name on card")]
        public string name_on_card { get; set; }
        public string request_id { get; set; }
        [Required]
        [Display(Name = "Account Number")]
        public string account_number { get; set; }
        [Display(Name = "Request Date")]
        public string request_date { get; set; }
        [Display(Name = "Request Status")]
        public string request_status { get; set; }
        [Display(Name = "Request Reason")]
        public string request_reason{ get; set; }

    }
}