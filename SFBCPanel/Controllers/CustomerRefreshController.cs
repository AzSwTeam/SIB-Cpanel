using Newtonsoft.Json.Linq;
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
    public class CustomerRefreshController : Controller
    {
        DataSource ds = new DataSource();
        //
        // GET: CustomerRefresh
        public ActionResult CustomerRefresh()
        {
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            String userbranch = "";
            if (Session["refreshaccountresult"] != null)
            {
                ViewBag.SuccessMessage = Session["refreshaccountresult"].ToString();
                Session["refreshaccountresult"] = null;
            }
            CustomerRegBankinfo model = new CustomerRegBankinfo();
            if (Session["user_branch"].ToString() != null)
            {
                userbranch = Session["user_branch"].ToString();
            }
            else
            {
                RedirectToAction("Index", "Home");
            }

            model.Branches = ds.PopulateBranchs(userbranch);
            model.AccTypes = ds.PopulateAccountTypes();
            model.Currencies = ds.PopulateCurrencies();
            model.catgories = ds.GetGatgories();
            return View(model);
        }

        [HttpPost]
        public ActionResult CustomerRefresh(CustomerRegBankinfo model)
        {
            ModelState.Clear();
            String message;
            //  account model;
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

                if (ModelState.IsValid)
                {
                    while (model.AccountNumber.Length < 7)
                    {
                        model.AccountNumber = "0" + model.AccountNumber;
                    }
                    String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    String response = ds.custregcheck2(model.Branch, model.CategoryCode);
                    if (response.Equals("This Account is Already exist"))
                    {
                        String act = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                        string AccountNumber1, AccountType, BranchName, Currency;
                        //act = "1101601560001634680001000";
                        Session["Account"] = act;
                        Session["branchcode"] = model.BranchCode;
                        //Session["shortaccount"] = shortaccount;
                        string response2 = Connecttocore.GetCustinfobycif(model.Branch);
                        JObject jobj = new JObject();
                        jobj = JObject.Parse(response2);
                        dynamic result = jobj;

                        if (result.errorcode == 1)
                        {
                            model.customernameArabic = result.CustomerNameAR;
                            model.customerphonenumber = result.CustomerMobile;
                            model.address = result.CustomerAddress;
                            model.CustomerPhone = result.CustomerMobile;
                            model.CustomerAccounts = new List<SelectListItem>();
                            string customeraccounts = result.CustomerAccounts;
                            string[] accounts = customeraccounts.Split('-');
                            for (int i = 0; i < accounts.Length; i++)
                            {
                                AccountNumber1 = accounts[i].ToString().Substring(13);
                                AccountType = ds.getaccounttype(accounts[i].ToString().Substring(5, 5));
                                BranchName = ds.getbranchnameenglish(accounts[i].ToString().Substring(2, 3));
                                Currency = ds.GetCurrencyName(accounts[i].ToString().Substring(10, 3));
                                model.CustomerAccounts.Add(new SelectListItem
                                {
                                    Text = BranchName + " - " + AccountType + " - " + Currency + " - " + AccountNumber1,
                                    Value = accounts[i]
                                });
                            }
                            //Session["CustID"] = model.cif;
                            Session["custname"] = model.CustomerName;
                            Session["custnamearabic"] = model.customernameArabic;
                            Session["CustomerAddress"] = model.address;
                            Session["custphone"] = model.CustomerPhone;
                            Session["CustomerAccounts"] = model.CustomerAccounts;
                            Session["custcat"] = model.CategoryCode;
                            //ViewBag.CustomerAccounts = model.CustomerAccounts;
                            //
                            //Session["custID"] = custID;
                            //Session["custname"] = custname;
                            //Session["custnamearabic"] = custnamearabic;
                            //Session["custphone"] = custphone;
                            //Session["CustomerAddress"] = custaddress;
                            //Session["custcat"] = model.CategoryCode;
                            //string usershorthand = "11" + model.BranchCode + model.AccountNumber;
                            //string usershorthand = model.AccountNumber;
                            //string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                            //ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Customer inquery", usershorthand + " - " + custname, DateTime.Now.ToString());

                            //if (model.SelectedChannelsID.Length.Equals(model.Channels.Count))
                            //{
                            //    Session["service"] = "3";
                            //}
                            //else
                            //{
                            //    foreach (var item in model.SelectedChannelsID)
                            //    {

                            //        if (item.Equals("2"))
                            //        {  //append each checked records into StringBuilder   
                            //            Session["ebranch"] = "T";
                            //            Session["service"] = "2";
                            //        }
                            //        else if (item.Equals("1"))
                            //        {  //append each checked records into StringBuilder   
                            //            Session["ebank"] = "T";
                            //            Session["service"] = "1";
                            //        }
                            //        else
                            //        {
                            //            Session["service"] = "0";
                            //        }


                            //    }
                            //}
                            return RedirectToAction("Refreshuser");
                            //return View(model);
                            //string bal = result.result;
                            //string[] separators = { ",", ":" };
                            //string value = bal;
                            //string[] acc = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                            //String custname;
                            //String custphone;

                            //if (acc.Length >= 10)
                            //{
                            //    model.ToAccount = toAccount;
                            //    custname = acc[3].ToString();
                            //    custphone = acc[5].ToString();
                            //    model.customername = custname;
                            //    model.customerphonenumber = custphone;

                            //    Session["custname"] = custname;
                            //    Session["custphone"] = custphone;

                            //    return View(model);
                            //}
                        }
                        else
                        {
                            Session["nottobankcustomeraccount"] = "Invalid CIF number, try again.";
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("", "Please Check Customer Information");
                        TempData["Success"] = true;
                        ViewBag.ResponseStat = "Successfully Sent";
                        ViewBag.ResponseMSG = "Thank you for Contacting us";
                    }
                }
                else
                {
                    message = "All Fields are required ";
                    ModelState.AddModelError("", "Something is missing" + message);
                    TempData["Success"] = true;
                    ViewBag.ResponseStat = "Successfully Sent";
                    ViewBag.ResponseMSG = "Thank you for Contacting us";
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
        public ActionResult GetCustomerRefreshPage(CustomerRegBankinfo model)
        {
            string message;
            if (model.Branch != null)
            {
                model = new CustomerRegBankinfo();
                String userbranch = Session["user_branch"].ToString();
                model = ds.GetUserRegistrationData(model.Branch);

                model.Branches = ds.PopulateBranchs(model.BranchCode, model.Branch);
                model.AccTypes = ds.PopulateAccountTypes(model.Branch);
                model.Currencies = ds.PopulateCurrencies();

                model.catgories = ds.GetGatgories();
                model.Channels = ds.Channels();
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
            }
            try
            {
                String userbranch = Session["user_branch"].ToString();
                //model.catgories.RemoveAt(0);
                if (ModelState.IsValid)
                {
                    while (model.AccountNumber.Length < 7)
                    {
                        model.AccountNumber = "0" + model.AccountNumber;
                    }
                    String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    String response = ds.custregcheck2(Accountnumber, model.CategoryCode);
                    if (response.Equals("This Account is Already exist"))
                    {
                        String act = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                        //act = "1101601560001634680001000";
                        Session["Account"] = act;
                        Session["branchcode"] = model.BranchCode;
                        //Session["shortaccount"] = shortaccount;
                        string response2 = Connecttocore.GetCustinfo(act);
                        JObject jobj = new JObject();
                        jobj = JObject.Parse(response2);
                        dynamic result = jobj;

                        string responseStatus = result.responseStatus;
                        string responseMessage = result.responseMessage;
                        string bal = result.result;
                        string[] separators = { ",", ":" };
                        string value = bal;
                        string[] acc = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        String custname;
                        String custID;
                        String custphone;

                        if (acc.Length >= 10)
                        {
                            custID = acc[1].ToString();
                            custname = acc[3].ToString();

                            custphone = acc[5].ToString();
                            Session["custID"] = custID;
                            Session["custname"] = custname;
                            Session["custphone"] = custphone;
                            Session["custcat"] = model.CategoryCode;
                            //string usershorthand = "11" + model.BranchCode + model.AccountNumber;
                            string usershorthand = model.AccountNumber;
                            string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                            ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Customer Inquery", usershorthand + " - " + custname, DateTime.Now.ToString());
                            return RedirectToAction("Refreshuser");
                        }

                        else
                        {
                            message = "Please check customer information something wrong ";
                            ModelState.AddModelError("", message);
                            return View(model);
                        }
                        // 
                        //}
                        //else
                        //{
                        //    message = "Sorry You Cannot register to this account because  ";
                        //    ModelState.AddModelError("", message + response);
                        //    return View(model);
                        //}
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please Check Customer Information");
                        TempData["Success"] = true;
                        ViewBag.ResponseStat = "Successfully Sent";
                        ViewBag.ResponseMSG = "Thank you for Contacting us";
                    }
                }
                else
                {
                    message = "All Fields are required ";
                    ModelState.AddModelError("", "Something is missing" + message);
                    TempData["Success"] = true;
                    ViewBag.ResponseStat = "Successfully Sent";
                    ViewBag.ResponseMSG = "Thank you for Contacting us";
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
        public ActionResult CustomerRefreshprocess(CustomerRegBankinfo passedmodel)
        {
            CustomerRegBankinfo model = new CustomerRegBankinfo();
            if (passedmodel.Branch != null)
            {
                model = new CustomerRegBankinfo();
                String userbranch = Session["user_branch"].ToString();
                model = ds.GetUserRegistrationData(passedmodel.Branch);

                //model.CustomerName = 
                model.Branches = ds.PopulateBranchs(model.BranchCode, passedmodel.Branch);
                model.AccTypes = ds.PopulateAccountTypes(passedmodel.Branch);
                model.Currencies = ds.PopulateCurrencies(model.CurrencyCode);

                model.catgories = ds.GetGatgories();
                model.Channels = ds.Channels();
                return View("CustomerRefresh", model);
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
                return View("Add", model);
            }
        }
        public ActionResult Refreshuser()
        {
            if (Session["refreshaccountresult"] != null)
            {
                ViewBag.msg = Session["refreshaccountresult"].ToString();
            }
            CustomerRegBankinfo model = new CustomerRegBankinfo();
            //model.CustomerID = Session["CustID"].ToString();
            model.CustomerName = Session["custname"].ToString();
            model.CustomerPhone = Session["custphone"].ToString();
            return View(model);
        }

        public ActionResult executeupdate(string customername, string customerphonenumber)
        {
            string accounttorefresh = Session["Account"].ToString();
            string userid = ds.getCustIDFromAcc(accounttorefresh);
            if (ds.refreshcustomer(int.Parse(userid), customername, customerphonenumber))
            {
                Session["refreshaccountresult"] = "Customer data updated accordingly";
            }
            else
            {
                Session["refreshaccountresult"] = "Something has gone wrong, please try again.";
            }
            return RedirectToAction("CustomerRefresh", "CustomerRefresh");
        }
    }
}