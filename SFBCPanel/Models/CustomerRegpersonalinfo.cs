using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using System.Web.Mvc;
namespace SFBCPanel.Models
{
    public class CustomerRegpersonalinfo
    {
        public List<SelectListItem> Profiles { get; set; }
        [Required(ErrorMessage = "Please Enter User Name")]
        [Display(Name = "User name")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please Enter User Address")]
        [Display(Name = "User Address")]
        public string Address { get; set; }
        [RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
        [Display(Name = "User Email")]
        public string Email { get; set; }
        [Display(Name = "User Profile")]
        public string Profile { get; set; }
        [Required]
        public String profileCode { get; set; }
        [Required(ErrorMessage = "Phone number required")]
        [Display(Name = "User Phone number")]
        public string phonenumber { get; set; }
            
    }
}