using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SFBCPanel.Models
{
    public class ChangePasswordViewModel
    {
        [DataType(DataType.Password)]
        //[Display(Name = "Current Password")]
        [Display(Name = "Current Passowrd")]
        [Required (
            ErrorMessageResourceName = "Please Enter Current Password")]
        public string CurrentPassword { get; set; }


        [DataType(DataType.Password)]
        //[Display(Name = "New Password")]
        [Display(Name = "New Passowrd" )]
        [Required(
            ErrorMessageResourceName = "Please Enter New Password")]
        public string NewPassword { get; set; }


        [DataType(DataType.Password)]
        //[Display(Name = "Confirm New Password")]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "Please make sure to Match written Password correctly")]
        public string ReNewPassword { get; set; }
    }
}