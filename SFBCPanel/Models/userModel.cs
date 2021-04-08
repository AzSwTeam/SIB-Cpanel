using SFBCPanel.Context;
using SIBCPanel.Context;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SFBCPanel.Models
{


    public class userInsertModel
    {
        public List<SelectListItem> Roles { get; set; }
        public List<SelectListItem> Branches { get; set; }
        
        
        [Required]
        [Display(Name = "First Name:")]
        public string name { get; set; }
        [Required]
        [Display(Name = "User Name:")]
        public string user_name { get; set; }
        
        [Display(Name = "Role Name:")]
        public string rolename { get; set; }
        
        [Display(Name = "Branch Name:")]

        public string branch_name { get; set; }
        [Required]
        public string roleid { get; set; }
        [Required]
        public string BranchCode { get; set; }
        
       
    }

    public class userlist {


            public int user_id { get; set; }
            
            [Display(Name =  "Name")]
            public string name { get; set; }
             
            
            [Display(Name = "Role")]
            public string rolename { get; set; }
           
            [Display(Name = "Branch")]

            public string user_branch { get; set; }


        
    }

    public class userUpdateModel
    {
       
        public List<SelectListItem> Branches { get; set; }
        public List<SelectListItem> Roles { get; set; }
        DataSource ds = new DataSource();
           public int user_id { get; set; }
        [Required]
        [Display(Name = "First Name:")]
        public string name { get; set; }
         [Required]
        [Display(Name = "User Name:")]
        public string user_name { get; set; }
          
         [Display(Name = "Role Name:")]
        public string rolename {get;set;}
          
         [Display(Name = "Branch Name:")]
       
        public string branch_name {get;set;}
         [Required]
         public string roleid { get; set; }
         [Required]
         public string BranchCode { get; set; }
    }
}