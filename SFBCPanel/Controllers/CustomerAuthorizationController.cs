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
    public class CustomerAuthorizationController : Controller
    {
        DataSource ds = new DataSource();
        //
        // GET: /CustomerAuth/
        public ActionResult CustomerAuthorization()
        {
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["result"] != null)
            {
                ViewBag.SuccessMessage = Session["result"].ToString();
                Session["result"] = null;
            }
            
            Session["bracode"] = "000";
            String branchcode = Session["bracode"].ToString();

            List<CustomerAuthorization> customer = new List<CustomerAuthorization>();
            customer = ds.PendingCustomer(branchcode);

            return View(customer);
        }
        public ActionResult Details(int id)
        {
            CustomerAuthorizationinfo model = new CustomerAuthorizationinfo();
            Session["user"] = id;
            List<CustomerAuthorizationinfo> customer = new List<CustomerAuthorizationinfo>();
            customer = ds.CustomerAuthorizationinfo(id.ToString());
            Session["customer"] = customer;
            foreach (var item in customer) {
            model.Branch = item.Branch;
            model.AccountType = item.AccountType;
            model.Customername = item.Customername;
                Session["customername"] = item.Customername;
            model.Currency = item.Currency;
            model.Customeraccount = item.Customeraccount;
            model.UserName = item.UserName;
            model.Address = item.Address;
            model.CustomerPhone = item.CustomerPhone;
            model.Email = item.Email;
            model.Profile = item.Profile;
            model.userid = item.userid;
              
            }
            model.authsts = "true";
            model.rjtsts = "false";
            Session["model"] = model;
            return View(model);
        }

        public ActionResult Authorize(int id, String status)
        {

            int response = ds.updatecustomer(id.ToString(), "U");



            if (response != -1)
            {
                CustomerAuthorizationinfo model = new CustomerAuthorizationinfo();
                model = (CustomerAuthorizationinfo)Session["model"];
                //string usershorthand = "11" + model.Branch + model.Customeraccount;
                string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Authorized customer registration", model.UserName + " - " + model.Customername, DateTime.Now.ToString());

                Session["result"] = "Customer Authorization Completed Successfuly";
                //return RedirectToAction("Index", "Home", new { area = "" });
                return RedirectToAction("CustomerAuthorization");
            }
            else{

                return RedirectToAction("Details", id);
            }
        }
        public ActionResult Reject(int id, String status)
        {

              int response = ds.updatecustomer(id.ToString(), "R");
              if (response != -1)
              {
                CustomerAuthorizationinfo model = new CustomerAuthorizationinfo();
                model = (CustomerAuthorizationinfo)Session["model"];
                //string usershorthand = "11" + model.Branch + model.Customeraccount;
                string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Rejected customer registration", model.UserName + " - " + model.Customername, DateTime.Now.ToString());

                Session["result"] = "Reject Completed Successfuly";
                 // return RedirectToAction("Index", "Home", new { area = "" });
                      return RedirectToAction("CustomerAuthorization");
              }
              else
              {

                  return RedirectToAction("Details", id);
              }
        }
	}
}