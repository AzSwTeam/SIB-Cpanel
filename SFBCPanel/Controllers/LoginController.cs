using SFBCPanel.Models;
using SFBCPanel.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SIBCPanel.Context;

namespace SFBCPanel.Controllers
{
    public class LoginController : Controller
    {
        DataSource ds = new DataSource();
        //
        // GET: /Login/
        public ActionResult Login()
        {
            Session["cpanelLogin"] = "false";
            return View();
        }
        [HttpPost]
        public ActionResult Login(Loginmodel model)
        {
            Loginmodelresult result= new Loginmodelresult();
            try
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                // We do not want to use any existing identity information

                result = ds.checkuserlogin(model.Username, model.Password, ipAddress);

                if ( result.lblconfirm.Equals("home"))
                {
                    Session["cpanelLogin"] = "true";
                    Session["username"] = model.Username;//added
                    Session["user_log"] = model.Username;
                    Session["UserId"] = result.UserId;
                    Session["user_name"] = result.user_name;
                    Session["user_branch"] = result.user_branch;
                    Session["user_roleid"] = result.user_roleid;
                    Session["user_status"] = result.status;
                    //model = ds.GetOnlineOfflineUsers(result.user_branch);
                    //Session["onlineofflineusers"] = list;
                    return RedirectToAction("Index","Home");
                }
                else
                if (result.lblconfirm.Equals("change_pass"))
                {
                    Session["cpanelLogin"] = "changepass";
                    Session["user_log"] = model.Username;
                    Session["UserId"] = result.UserId;
                    Session["user_name"] = result.user_name;
                    Session["user_branch"] = result.user_branch;
                    Session["user_roleid"] = result.user_roleid;
                    List<int> list = ds.GetOnlineOfflineUsers(result.user_branch);
                    Session["onlineofflineusers"] = list;
                    return RedirectToAction("Changepassword");
                }
                else
                {
                    ModelState.AddModelError("",result.lblconfirm);
                    return View(model);
                }
            }
            catch (Exception e) 
            {
            }
            return View();
        }

        public ActionResult Changepassword()
        {
            Session["cpanelLogin"] = "changepass";
            changepassword model = new changepassword();
            model.OldPassword = null;
            model.newPassword = null;
            model.confrimPassword = null;
            return View();
        }
        [HttpPost]
        public ActionResult Changepassword(changepassword model)
        {
            if (((model.OldPassword== null)
            || ((model.newPassword == null)
            || (model.confrimPassword == null))))
            {
            ModelState.AddModelError("", "Please Check Your Information ");
                return View();
            }

           
            if ((model.newPassword != model.confrimPassword))
            {
                 
                ModelState.AddModelError("", "Please Check Your New Password ");
                model.OldPassword = null; 
                model.newPassword = null; 
                model.confrimPassword = null;
                return View();

                 
            }
            String username = Session["user_log"].ToString();
            String result = ds.changepass(username, model.OldPassword, model.newPassword);
            if (result.Equals("Your Password was Changed Successfully"))
            {
                Session["cpanelLogin"] = "true";
                Session["Homemessage"] = result;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Session["cpanelLogin"] = "changepass";
                ModelState.AddModelError("", result);
                return View();
            }
           
        }
    
    }
}