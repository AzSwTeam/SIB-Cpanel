using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cpanel.Models
{
    public class AllTransfersViewModel
    {
        public int TranId { get; set; }
        public string TranName { get; set; }
        public string TranStatus { get; set; }

        public string TranResult { get; set; }
    }
}