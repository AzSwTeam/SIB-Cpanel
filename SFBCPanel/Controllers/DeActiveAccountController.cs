using SFBCpanel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SFBCPanel.Context;
using SFBCPanel.Models;
using SIBCPanel.Context;

namespace SFBCpanel.Controllers
{
    public class DeActiveAccountController : Controller
    {
        //
        // GET: /DeActiveAccount/

        DataSource ds = new DataSource();
        //
        // GET: /ActiveAccount/
        public ActionResult DeActiveCustomer()
        {
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["deresult"] != null)
            {
                ViewBag.SuccessMessage = Session["deresult"].ToString();
                Session["deresult"] = null;
            }
            CustomerRegBankinfo model = new CustomerRegBankinfo();
            String userbranch = Session["user_branch"].ToString();


            model.Branches = ds.PopulateBranchs(userbranch);
            model.AccTypes = ds.PopulateAccountTypes();
            model.Currencies = ds.PopulateCurrencies();
            model.catgories = ds.GetGatgories();

            Session["regmodel"] = model;
            return View(model);

        }

        [HttpPost]
        public ActionResult DeActiveCustomer(CustomerRegBankinfo model)
        {
            string message = "";
            try
            {
                String userbranch = Session["user_branch"].ToString();


                model.Branches = ds.PopulateBranchs(userbranch);
                model.AccTypes = ds.PopulateAccountTypes();
                model.Currencies = ds.PopulateCurrencies();
                model.catgories = ds.GetGatgories();
                //model.catgories.RemoveAt(0);
                var selectedBranch = model.Branches.Find(p => p.Value == model.BranchCode.ToString());
                var selectedAccType = model.AccTypes.Find(p => p.Value == model.AccountTypecode.ToString());
                var selectedCurrency = model.Currencies.Find(p => p.Value == model.CurrencyCode.ToString());
                var selectedcategory = model.catgories.Find(p => p.Value == model.CategoryCode.ToString());

                if (selectedBranch != null)
                {
                    selectedBranch.Selected = true;

                }
                if (selectedAccType != null)
                {
                    selectedAccType.Selected = true;

                }
                if (selectedCurrency != null)
                {
                    selectedCurrency.Selected = true;

                }
                if (selectedcategory != null)
                {
                    selectedcategory.Selected = true;

                }


                if (ModelState.IsValidField(model.BranchCode) && ModelState.IsValidField(model.AccountNumber) &&
                    ModelState.IsValidField(model.AccountTypecode) && ModelState.IsValidField(model.CurrencyCode))
                {
                    custinfo infomodel = new custinfo();

                    String response;
                    String fullnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    infomodel = ds.getcustinfo(model.BranchCode, model.AccountTypecode, model.AccountNumber, model.CurrencyCode, model.CategoryCode,fullnumber);
                    response = infomodel.lblconfirm;
                    if (response.Equals("This Account is Already exist"))
                    {
                        //String act = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                        String act = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                        Session["Account"] = act;
                        if (infomodel.status.ToString().Equals("A"))
                        {
                            int result = ds.updatecustomerusingact(act, "D");
                            if (result != -1)
                            {
                                string custname = ds.getcustomerfullname(act);
                                string customeraccount = custname;
                                string usershorSthand = "18" + act.Substring(2, 3) + act.Substring(11, 7).ToString();
                                string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());

                                ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Deactivated customer", usershorSthand + " - " + custname, DateTime.Now.ToString());
                                String s = ds.getbranchnameenglish(act.Substring(2, 3)).ToString() + "-" + ds.getaccounttype(act.Substring(5, 5)).ToString() + "-" + ds.getcurrencyname(act.Substring(10, 3)).ToString() + "-" + act.Substring(13).ToString();


                                Session["deresult"] = "The Customer Account " + s + " DeActivated Successfully";
                                //return RedirectToAction("Index", "Home", new { area = "" });
                                return RedirectToAction("DeActiveCustomer");
                            }
                        }
                        else if (infomodel.status.ToString().Equals("P"))
                        {

                            message = "This Customer Account Is Not Authorized";
                            Session["deresult"] = "This Customer Account Is Not Authorized";
                            ModelState.AddModelError("", message);
                            //return View(model);
                            return RedirectToAction("DeActiveCustomer");
                        }

                        else if (infomodel.status.ToString().Equals("R"))
                        {
                            message = "This Customer Account Is Rejected";
                            Session["deresult"] = "This Customer Account Is Rejected";
                            ModelState.AddModelError("", message);
                            // return View(model);
                            return RedirectToAction("DeActiveCustomer");

                        }

                        else if (infomodel.status.ToString().Equals("D"))
                        {
                            message = "This Customer Account Is Deleted or Deactivated";
                            Session["deresult"] = "This Customer Account Is Deleted or Deactivated";
                            ModelState.AddModelError("", message);
                            //return View(model);
                            return RedirectToAction("DeActiveCustomer");


                        }
                        else if (infomodel.status.ToString().Equals("S"))
                        {
                            message = "This Customer Account Is Stoped";
                            Session["deresult"] = "This Customer Account Is Stoped";
                            ModelState.AddModelError("", message);
                            //return View(model);
                            return RedirectToAction("DeActiveCustomer");




                        }


                    }
                    else
                    {
                        message = "Sorry this account Not Registered ";
                        Session["deresult"] = "Sorry this account Not Registered ";
                        ModelState.AddModelError("", message);
                        //return View(model);
                        return RedirectToAction("DeActiveCustomer");
                    }
                }
                else
                {
                    message = "All Fields are required ";
                    ModelState.AddModelError("", "Something is missing" + message);

                }
            }
            catch (Exception ex)
            {
                message = "Please Contact for Support";
                ModelState.AddModelError("", "Something is missing" + message);

            }
            return View(model);
        }

        [HttpPost]
        public ActionResult DeActiveCustomerprocess(CustomerRegBankinfo passedmodel)
        {
            CustomerRegBankinfo model = new CustomerRegBankinfo();
            if (passedmodel.Branch != null)
            {
                model = new CustomerRegBankinfo();
                String userbranch = Session["user_branch"].ToString();
                model = ds.GetUserRegistrationData(passedmodel.Branch);

                model.Branches = ds.PopulateBranchs(model.Branch, model.CustomerID);
                model.AccTypes = ds.PopulateAccountTypes(model.CustomerID);
                model.Currencies = ds.PopulateCurrencies(model.CurrencyCode);

                model.catgories = ds.GetGatgories();
                model.Channels = ds.Channels();
                return View("DeActiveCustomer", model);
            }
            else
            {
                String userbranch = "";
                if (Session["addaccountresult"] != null)
                {
                    ViewBag.SuccessMessage = Session["addaccountresult"].ToString();
                    Session["addaccountresult"] = null;
                }

                userbranch = Session["user_branch"].ToString();
                model.Branches = ds.PopulateBranchs(userbranch);
                model.AccTypes = ds.PopulateAccountTypes();
                model.Currencies = ds.PopulateCurrencies();
                model.catgories = ds.GetGatgories();
                return View("DeActiveCustomer", model);
            }
            

        }

    }
}
