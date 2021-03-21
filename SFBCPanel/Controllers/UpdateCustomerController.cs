using SFBCPanel.Context;
using SFBCPanel.Models;
using SIBCPanel.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cpanel.Controllers
{
    public class UpdateCustomerController : Controller
    {
        DataSource ds = new DataSource();

        //
        // GET: /UpdateCustomer/
        public ActionResult CustInfo()
        {
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["updatestatus"] != null)
            {
                ViewBag.SuccessMessage = Session["updatestatus"].ToString();
                Session["updatestatus"] = null;
            }
            if (Session["deletestatus"] != null)
            {
                ViewBag.SuccessMessage = Session["deletestatus"].ToString();
                Session["deletestatus"] = null;
            }
            if (Session["FailedMessage"] != null)
            {
                ViewBag.FailedMessage = Session["FailedMessage"].ToString();
                Session["FailedMessage"] = null;
            }
            CustomerRegBankinfo model = new CustomerRegBankinfo();
            String userbranch = Session["user_branch"].ToString();


            model.Branches = ds.PopulateBranchs(userbranch);
            model.AccTypes = ds.PopulateAccountTypes();
            model.Currencies = ds.PopulateCurrencies();
            model.catgories = ds.GetGatgories();

            return View(model);
        }

        [HttpPost]
        public ActionResult CustInfo(CustomerRegBankinfo model)
        {

            string message = "";
            try
            {
                ModelState.Clear();
                GETpassword model1 = new GETpassword();
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
                    String fullaccount = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    String response;

                    infomodel = ds.getcustinfo(model.BranchCode, model.AccountTypecode, model.AccountNumber, model.CurrencyCode, model.CategoryCode, fullaccount);
                    response = infomodel.lblconfirm;
                    if (response.Equals("This Account is Already exist") && infomodel.status.ToString().Equals("A"))
                    {
                        //String Accountnumber = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                        String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                        infomodel.Profiles = ds.PopulateProfiles();
                        Session["resultcustinfo"] = infomodel;
                        return RedirectToAction("Detials");

                    }

                    else if (infomodel.status.ToString().Equals("P"))
                    {

                        message = "This Customer Account Is Not Authorized";
                        Session["FailedMessage"] = message;
                        ModelState.AddModelError("", message);
                        return View(model);
                    }
                    else if (infomodel.status.ToString().Equals("U"))
                    {

                        message = "This Customer Account Is Not Activied";
                        Session["FailedMessage"] = message;
                        ModelState.AddModelError("", message);
                        return View(model);
                    }

                    else if (infomodel.status.ToString().Equals("R"))
                    {
                        message = "This Customer Account Is Rejected";
                        Session["FailedMessage"] = message;
                        ModelState.AddModelError("", message);
                        return View(model);

                    }

                    else if (infomodel.status.ToString().Equals("A"))
                    {
                        message = "This Customer Account Is  activated already";
                        Session["FailedMessage"] = message;
                        ModelState.AddModelError("", message);
                        return View(model);
                    }
                    else if (infomodel.status.ToString().Equals("D"))
                    {
                        message = "This Customer Account Is  Deactivated";
                        Session["FailedMessage"] = message;
                        ModelState.AddModelError("", message);
                        return View(model);


                    }
                    else if (infomodel.status.ToString().Equals("B"))
                    {
                        message = "This Customer Account Is  Blocked";
                        Session["FailedMessage"] = message;
                        ModelState.AddModelError("", message);
                        return View(model);


                    }
                    else if (infomodel.status.ToString().Equals("DE"))
                    {
                        message = "This Customer Account Is Deleted";
                        Session["FailedMessage"] = message;
                        ModelState.AddModelError("", message);
                        return View(model);


                    }
                    else if (infomodel.status.ToString().Equals("S"))
                    {
                        message = "This Customer Account Is Stoped";
                        Session["FailedMessage"] = message;
                        ModelState.AddModelError("", message);
                        return View(model);


                    }
                }



            }
            catch (Exception ex)
            {
                message = "Please Contact for Support";
                Session["FailedMessage"] = message;
                ModelState.AddModelError("", "Something is missing" + message);

            }
            return View(model);
        }

        [HttpPost]
        public ActionResult GetCustomerData(CustomerRegBankinfo model)
        {
            string message;

            model = ds.GetUserRegistrationData(model.placeholder);

            model.Branches = ds.PopulateBranchs(model.BranchCode, model.placeholder);
            model.AccTypes = ds.PopulateAccountTypes(model.placeholder);
            model.Currencies = ds.PopulateCurrencies();

            model.catgories = ds.GetGatgories();
            model.Channels = ds.Channels();

            if (ModelState.IsValidField(model.BranchCode) && ModelState.IsValidField(model.AccountNumber) &&
                   ModelState.IsValidField(model.AccountTypecode) && ModelState.IsValidField(model.CurrencyCode))

            {
                custinfo infomodel = new custinfo();
                String fullaccount = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                String response;

                infomodel = ds.getcustinfo(model.BranchCode, model.AccountTypecode, model.AccountNumber, model.CurrencyCode, model.CategoryCode, fullaccount);
                response = infomodel.lblconfirm;
                if (response.Equals("This Account is Already exist") && infomodel.status.ToString().Equals("A"))
                {
                    //String Accountnumber = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    infomodel.Profiles = ds.PopulateProfiles();
                    Session["resultcustinfo"] = infomodel;
                    return RedirectToAction("Detials");

                }

                else if (infomodel.status.ToString().Equals("P"))
                {

                    message = "This Customer Account Is Not Authorized";
                    Session["FailedMessage"] = message;
                    ModelState.AddModelError("", message);
                    return RedirectToAction("CustInfo", model);
                }
                else if (infomodel.status.ToString().Equals("U"))
                {

                    message = "This Customer Account Is Not Activied";
                    Session["FailedMessage"] = message;
                    ModelState.AddModelError("", message);
                    return RedirectToAction("CustInfo", model);
                }

                else if (infomodel.status.ToString().Equals("R"))
                {
                    message = "This Customer Account Is Rejected";
                    Session["FailedMessage"] = message;
                    ModelState.AddModelError("", message);
                    return RedirectToAction("CustInfo", model);
                }

                else if (infomodel.status.ToString().Equals("A"))
                {
                    message = "This Customer Account Is  activated already";
                    Session["FailedMessage"] = message;
                    ModelState.AddModelError("", message);
                    return RedirectToAction("CustInfo", model);
                }
                else if (infomodel.status.ToString().Equals("D"))
                {
                    message = "This Customer Account Is  Deactivated";
                    Session["FailedMessage"] = message;
                    ModelState.AddModelError("", message);
                    return RedirectToAction("CustInfo", model);
                }
                else if (infomodel.status.ToString().Equals("B"))
                {
                    message = "This Customer Account Is  Blocked";
                    Session["FailedMessage"] = message;
                    ModelState.AddModelError("", message);
                    return RedirectToAction("CustInfo", model);
                }
                else if (infomodel.status.ToString().Equals("DE"))
                {
                    message = "This Customer Account Is Deleted";
                    Session["FailedMessage"] = message;
                    ModelState.AddModelError("", message);
                    return RedirectToAction("CustInfo", model);
                }
                else if (infomodel.status.ToString().Equals("S"))
                {
                    message = "This Customer Account Is Stoped";
                    Session["FailedMessage"] = message;
                    ModelState.AddModelError("", message);
                    return RedirectToAction("CustInfo", model);
                }
            }
            else
            {
                message = "Customer Dosen't exist";
                ModelState.AddModelError("", message);
                Session["FailedMessage"] = message;
                return RedirectToAction("CustInfo", model);
            }
            return RedirectToAction("CustInfo", model);
        }

        [HttpPost]
        public ActionResult CustInfoprocess(CustomerRegBankinfo passedmodel)
        {
            ModelState.Clear();
            CustomerRegBankinfo model = new CustomerRegBankinfo();
            if (passedmodel.placeholder != null)
            {
                model = new CustomerRegBankinfo();
                String userbranch = Session["user_branch"].ToString();
                model = ds.GetUserRegistrationData(passedmodel.placeholder);

                model.Branches = ds.PopulateBranchs(model.BranchCode, passedmodel.placeholder);
                model.AccTypes = ds.PopulateAccountTypes(passedmodel.placeholder);
                model.Currencies = ds.PopulateCurrencies(model.CurrencyCode);

                model.catgories = ds.GetGatgories();
                model.Channels = ds.Channels();
                return RedirectToAction("CustInfo", model);
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
                return RedirectToAction("CustInfo", model);
            }
        }

        public ActionResult Detials()
        {
            custinfo model = new custinfo();

            model = (custinfo)Session["resultcustinfo"];

            return View(model);
        }
        public ActionResult Update(String userid)
        {
            custinfo model = new custinfo();
            model = (custinfo)Session["resultcustinfo"];
            model.Profiles = ds.PopulateProfiles(int.Parse(model.user_id));
            model.Channels = ds.Channels();

            return View(model);
        }
        [HttpPost]
        public ActionResult Update(custinfo model)
        {

            model.Channels = ds.Channels();
            //  custinfo model = new custinfo();
            String message = "";
            //   model = (custinfo)Session["updatecustinfo"];
            try
            {

                model.Profiles = ds.PopulateProfiles();

                var selectedProfile = model.Profiles.Find(p => p.Value == model.profileCode.ToString());
                if (selectedProfile != null)
                {
                    selectedProfile.Selected = true;

                }

                if (model.SelectedChannelsID.Length.Equals(model.Channels.Count))
                {
                    //Session["service"] = "3";
                    model.Channel = "3";
                }
                else
                {
                    foreach (var item in model.SelectedChannelsID)
                    {

                        if (item.Equals("2"))
                        {  //append each checked records into StringBuilder   
                            //Session["ebranch"] = "T";
                            //Session["service"] = "2";
                            model.Channel = "2";
                        }
                        else if (item.Equals("1"))
                        {  //append each checked records into StringBuilder   
                            //Session["ebank"] = "T";
                            //Session["service"] = "1";
                            model.Channel = "1";
                        }
                        else
                        {
                            model.Channel = "0";
                            //Session["service"] = "0";
                        }


                    }
                }

                int result = ds.Updatecustomer("U", model);
                if (result != -1)
                {
                    string custname = model.user_name;
                    Customerinfopass customer = ds.GetUserinfoData(model.user_id);
                    string usershorSthand = "18" + customer.BranchCode + customer.AccountNumber;
                    string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                    ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Customer information update", usershorSthand + " - " + custname, DateTime.Now.ToString());

                    Session["updatestatus"] = "These Customer information has been Updated";
                    return RedirectToAction("CustInfo");
                }
                else
                {
                    message = "Problem";
                    ModelState.AddModelError("", message);

                }
            }
            catch (Exception ex)
            {
                message = "Please Contact for Support";
                ModelState.AddModelError("", "Something is missing" + message);

            }

            return View(model);
        }
        public ActionResult Delete(String userid)
        {
            custinfo model = new custinfo();
            String message = "";
            model = (custinfo)Session["resultcustinfo"];
            int result = ds.Updatecustomer("DE", model);
            if (result != -1)
            {
                string custname = model.user_name;
                Customerinfopass customer = ds.GetUserinfoData(model.user_id);
                string usershorSthand = "18" + customer.BranchCode + customer.AccountNumber;
                string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Deleted customer", usershorSthand + " - " + custname, DateTime.Now.ToString());

                Session["deletestatus"] = "These Customer Account has been Deleted";
                return RedirectToAction("CustInfo");
            }
            else
            {
                message = "Problems";
                ModelState.AddModelError("", message);

            }


            return View(model);
        }
    }
}