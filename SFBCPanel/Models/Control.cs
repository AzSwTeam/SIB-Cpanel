using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SFBCPanel.Models
{
    public class Control
    {
    }

    public class UsersChartsModel
    {
        public string category { get; set; }
        public float userscount { get; set; }
    }

    public class TransactionsDetailsModel
    {
        public string transactiontype { get; set; }
        public int transactioncount { get; set; }
    }

    public class TransactionStatusesModel
    {
        public string status;
        public int count;
    }


    public class Servielist
    {
          [Display(Name = "Service ID")]
        public int service_id { get; set; }

        [Display(Name = "Service Name")]
        public string name { get; set; }
          [Display(Name = "Service Status")]
        public string service_status { get; set; }


   [Display(Name = "Service Code")]

        public string service_code { get; set; }
       



    }
    public class ServiceInsertModel
    {
       
        [Required]
        [Display(Name = "Service Name:")]
        public string service_name { get; set; }


        public string service_status { get; set; }



        public string service_id { get; set; }

        public string service_code { get; set; }
         


    }
    public class ServiceUpdateModel
    {
        public List<SelectListItem> statuses { get; set; }

        [Required]
        [Display(Name = "Service Name:")]
        public string service_name { get; set; }

        public string service_status_code { get; set; }
        public string service_status { get; set; }



        public string service_id { get; set; }

        public string service_code { get; set; }



    }
}