using SFBCPanel.Context;
using SFBCPanel.Models;
using SIBCPanel.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SFBCPanel.Controllers
{
    public class ChangePassController : Controller
    {
        DataSource ds = new DataSource();
        // GET: ChangePass
        public ActionResult ChangePass()
        {
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["Userchangepassmessage"] != null)
            {
                ViewBag.SuccessMessage = Session["Userchangepassmessage"].ToString();
                Session["Userchangepassmessage"] = null;
            }
            changepassword model = new changepassword();
            model.OldPassword = null;
            model.newPassword = null;
            model.confrimPassword = null;
            return View();
        }

        [HttpPost]
        public ActionResult ChangePass(changepassword model)
        {
            if (((model.OldPassword == null)
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

                Session["Userchangepassmessage"] = result;
                return RedirectToAction("ChangePass", "ChangePass");
            }
            else
            {

                ModelState.AddModelError("", result);
                return View();
            }

        }
    }
}