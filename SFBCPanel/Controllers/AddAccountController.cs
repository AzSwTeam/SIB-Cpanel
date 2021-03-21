using Newtonsoft.Json.Linq;
using SFBCpanel.Models;
using SFBCPanel.Context;
using SFBCPanel.Models;
using SIBCPanel.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Protocols;

namespace SFBSPancel.Controllers
{
    public class AddAccountController : Controller
    {
        DataSource ds = new DataSource();
        //
        // GET: /AddAcount/
        public ActionResult Add()
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
            if (Session["addaccountresult"] != null)
            {
                ViewBag.SuccessMessage = Session["addaccountresult"].ToString();
                Session["addaccountresult"] = null;
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
        public ActionResult Add(CustomerRegBankinfo model)
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
                //var selectedBranch = model.Branches.Find(p => p.Value == model.BranchCode.ToString());
                //var selectedAccType = model.AccTypes.Find(p => p.Value == model.AccountTypecode.ToString());
                //var selectedCurrency = model.Currencies.Find(p => p.Value == model.CurrencyCode.ToString());
                //var selectedcategory = model.catgories.Find(p => p.Value == model.CategoryCode.ToString());
                //if (selectedBranch != null)
                //{
                //    selectedBranch.Selected = true;

                //}
                //if (selectedAccType != null)
                //{
                //    selectedAccType.Selected = true;

                //}
                //if (selectedCurrency != null)
                //{
                //    selectedCurrency.Selected = true;

                //}
                //if (selectedcategory != null)
                //{
                //    selectedcategory.Selected = true;

                //}

                if (ModelState.IsValid)
                {
                    //String Accountnumber = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    //String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    String response = ds.custregcheck2(model.cif, model.CategoryCode);
                    if (response.Equals("This Account is Already exist"))
                    {
                        Session["modelCategoryCode"] = model.CategoryCode;
                        return RedirectToAction(actionName: "newCustomerAccount", routeValues: new { cif = model.cif });
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
        public ActionResult Addprocess(CustomerRegBankinfo passedmodel)
        {
            CustomerRegBankinfo model = new CustomerRegBankinfo();
            if (passedmodel.Branch != null)
            {
                model = new CustomerRegBankinfo();
                String userbranch = Session["user_branch"].ToString();
                model = ds.GetUserRegistrationData(passedmodel.Branch);

                model.Branches = ds.PopulateBranchs(model.BranchCode, passedmodel.Branch);
                model.AccTypes = ds.PopulateAccountTypes(passedmodel.Branch);
                model.Currencies = ds.PopulateCurrencies(model.CurrencyCode);

                model.catgories = ds.GetGatgories();
                model.Channels = ds.Channels();
                return View("Add", model);
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

        public ActionResult newCustomerAccount(String cif)
        {
            CustomerRegBankinfo model = new CustomerRegBankinfo();

            //Session["Accountold"] = "";
            //Session["Accountold"] = Account;
            //String CategoryCode = Session["modelCategoryCode"].ToString();
            string AccountNumber1, AccountType, BranchName, Currency;

            custinfo infomodel = ds.getcustinfo("", "", "", "", "", cif);
            string response = Connecttocore.GetCustinfobycif(cif);

            if (infomodel.lblconfirm == "This Account is Already exist")
            {
                JObject jobj = new JObject();
                jobj = JObject.Parse(response);
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

                    UserDetailsModel user = ds.GetUserDetails(cif);
                    Session["CustID"] = cif;
                    Session["custname"] = user.user_name;
                    Session["custnamearabic"] = user.user_name;
                    Session["CustomerAddress"] = user.user_address;
                    Session["custphone"] = user.user_mobile;
                    Session["custcat"] = user.category;
                    Session["CustomerAccounts"] = model.CustomerAccounts;

                    ViewBag.CustomerAccounts = model.CustomerAccounts;

                    String name = infomodel.user_name;
                    String username = infomodel.user_log;
                    if (model.customernameArabic != null)
                    {
                        ViewBag.name = model.customernameArabic;
                    }
                    else if (model.CustomerName != null)
                    {
                        ViewBag.name = model.CustomerName;
                    }
                    else
                    {
                        ViewBag.name = "N/A";
                    }
                    ViewBag.username = model.cif;
                    Session["Accountoldname"] = "";
                    Session["Accountoldname"] = name;
                    Session["Accountoldusername"] = "";
                    Session["Accountoldusername"] = username;
                    //Session["modelCategoryCode"] = CategoryCode;


                    //String userbranch = Session["user_branch"].ToString();
                    //if (name != "")
                    //{
                    //    model = ds.GetUserRegistrationData(username);

                    //    model.Branches = ds.PopulateBranchs(model.BranchCode, username);
                    //    model.AccTypes = ds.PopulateAccountTypes(username);
                    //    model.Currencies = ds.PopulateCurrencies();
                    //}

                    //model.Branches = ds.PopulateBranchs(userbranch);
                    //model.AccTypes = ds.PopulateAccountTypes();
                    //model.Currencies = ds.PopulateCurrencies();
                    //model.catgories = ds.GetGatgories();


                    return View(model);
                }
                else
                {
                    ModelState.AddModelError("", "This customer is not registered.");
                    TempData["Success"] = true;
                    ViewBag.ResponseStat = "Successfully Sent";
                    ViewBag.ResponseMSG = "Thank you for Contacting us";
                    return RedirectToAction("Add", "AddAccount");
                }
            }
            else if (infomodel.lblconfirm == "This Account is available")
            {
                ModelState.AddModelError("", "This customer is not registered.");
                TempData["Success"] = true;
                ViewBag.ResponseStat = "Successfully Sent";
                ViewBag.ResponseMSG = "Thank you for Contacting us";
                return RedirectToAction("Add", "AddAccount");
            }
            else
            {
                ModelState.AddModelError("", "Something went wrong.");
                TempData["Success"] = true;
                ViewBag.ResponseStat = "Successfully Sent";
                ViewBag.ResponseMSG = "Thank you for Contacting us";
                return RedirectToAction("Add", "AddAccount");
            }
        }

        [HttpPost]
        public ActionResult newCustomerAccount(CustomerRegBankinfo model, string command)
        {
            //String CategoryCode = Session["modelCategoryCode"].ToString();
            //model.CategoryCode = CategoryCode;

            ViewBag.name = Session["Accountoldname"].ToString();
            ViewBag.username = Session["Accountoldusername"].ToString();

            if (command == "Check")
            {
                String name = checknewCustomerAccount(model);
                if (name == "Account available")
                {
                    // do stuff

                    ViewBag.msg = name;
                    //getting registered account
                    UserDetailsModel user = ds.GetUserDetails(model.cif);
                    Session["Accountold"] = user.defult_account;
                    Session["Categorytoadd"] = user.category;
                    model.CustomerName = Session["custname"].ToString();
                    model.customernameArabic = Session["custnamearabic"].ToString();
                    model.address = Session["CustomerAddress"].ToString();
                    model.CustomerPhone = Session["custphone"].ToString();
                    model.CustomerAccounts = (List<SelectListItem>)Session["CustomerAccounts"];
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Add(new SelectListItem
                    {
                        Text = ds.getbranchnameenglish(model.AccountNumber.Substring(2, 3)) + " - " + ds.getaccounttype(model.AccountNumber.Substring(5, 5)) + " - " + ds.GetCurrencyName(model.AccountNumber.Substring(10, 3)) + " - " + model.AccountNumber.Substring(13),
                        Value = model.AccountNumber
                    });

                    ViewBag.CustomerAccounts = list;

                    ViewData["Success"] = "This Account is available to be linked.";
                }
                else
                {
                    //getting registered account
                    UserDetailsModel user = ds.GetUserDetails(model.cif);
                    Session["Accountold"] = user.defult_account;
                    Session["Categorytoadd"] = user.category;
                    model.CustomerName = Session["custname"].ToString();
                    model.customernameArabic = Session["custnamearabic"].ToString();
                    model.address = Session["CustomerAddress"].ToString();
                    model.CustomerPhone = Session["custphone"].ToString();
                    model.CustomerAccounts = (List<SelectListItem>)Session["CustomerAccounts"];

                    ViewBag.CustomerAccounts = model.CustomerAccounts;

                    ModelState.AddModelError("", name);
                    ViewData["Fail"] = "This Account is already linked to user.";
                }
                return View(model);
            }
            else
                if (command == "Add")
            {


                String message;
                //  account model;
                try
                {
                    String userbranch = Session["user_branch"].ToString();


                    //model.Branches = ds.PopulateBranchs(userbranch);
                    //model.AccTypes = ds.PopulateAccountTypes();
                    //model.Currencies = ds.PopulateCurrencies();

                    //var selectedBranch = model.Branches.Find(p => p.Value == model.BranchCode.ToString());
                    //var selectedAccType = model.AccTypes.Find(p => p.Value == model.AccountTypecode.ToString());
                    //var selectedCurrency = model.Currencies.Find(p => p.Value == model.CurrencyCode.ToString());
                    //if (selectedBranch != null)
                    //{
                    //    selectedBranch.Selected = true;

                    //}
                    //if (selectedAccType != null)
                    //{
                    //    selectedAccType.Selected = true;

                    //}
                    //if (selectedCurrency != null)
                    //{
                    //    selectedCurrency.Selected = true;

                    //}
                    //String Accountnumber = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    //String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;

                    List<int> userids = ds.getaccountsids(Session["Accountold"].ToString());

                    String result2 = "N/A";
                    string category = "2";

                    //corprate
                    if (userids.Count > 1)
                    {
                        foreach (var id in userids)
                        {
                            result2 = ds.addnewacount(Session["Accountold"].ToString(), model.AccountNumber, category, int.Parse(id.ToString()));
                            category = "3";
                        }
                    }
                    //personal
                    else
                    {
                        result2 = ds.addnewacount(Session["Accountold"].ToString(), model.AccountNumber, Session["Categorytoadd"].ToString(), int.Parse(userids[0].ToString()));
                    }


                    String res = " " + model.AccountNumber.Substring(13) + " : " + result2;

                    string custname = Session["Accountoldname"].ToString();//ds.getcustomerfullname(Accountnumber);
                                                                           //string usershorthand = "11" + model.BranchCode + model.AccountNumber;
                    string usershorthand = model.AccountNumber;
                    string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                    ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Account link request", usershorthand + " - " + custname, DateTime.Now.ToString());

                    Session["addaccountresult"] = res;
                    return RedirectToAction("Add");
                    // return RedirectToAction(actionName: "newCustomerAccount", routeValues: new { Account = Accountnumber });

                }
                catch (Exception ex)
                {
                    message = "Please Contact for Support";
                    ModelState.AddModelError("", "Something is missing" + message);

                }
                List<SelectListItem> list = new List<SelectListItem>();
                list.Add(new SelectListItem
                {
                    Text = ds.getbranchnameenglish(model.AccountNumber.Substring(2, 3)) + " - " + ds.getaccounttype(model.AccountNumber.Substring(5, 5)) + " - " + ds.GetCurrencyName(model.AccountNumber.Substring(10, 3)) + " - " + model.AccountNumber.Substring(13),
                    Value = model.AccountNumber
                });

                ViewBag.CustomerAccounts = list;
                return View(model);
            }
            else
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Add(new SelectListItem
                {
                    Text = ds.getbranchnameenglish(model.AccountNumber.Substring(2, 3)) + " - " + ds.getaccounttype(model.AccountNumber.Substring(5, 5)) + " - " + ds.GetCurrencyName(model.AccountNumber.Substring(10, 3)) + " - " + model.AccountNumber.Substring(13),
                    Value = model.AccountNumber
                });

                ViewBag.CustomerAccounts = list;
                ModelState.AddModelError("", "Please check one of buttons");
                return View(model);
            }
        }


        [HttpPost]

        public String checknewCustomerAccount(CustomerRegBankinfo model)
        {

            String message;
            //  account model;
            try
            {
                UserDetailsModel user = ds.GetUserDetails(model.cif);
                string response = ds.checkaccountifbound(model.AccountNumber,user.user_id.ToString());
                return response;
                //String userbranch = Session["user_branch"].ToString();


                //model.Branches = ds.PopulateBranchs(userbranch);
                //model.AccTypes = ds.PopulateAccountTypes();
                //model.Currencies = ds.PopulateCurrencies();
                //model.catgories = ds.GetGatgories();

                //var selectedBranch = model.Branches.Find(p => p.Value == model.BranchCode.ToString());
                //var selectedAccType = model.AccTypes.Find(p => p.Value == model.AccountTypecode.ToString());
                //var selectedCurrency = model.Currencies.Find(p => p.Value == model.CurrencyCode.ToString());
                ////     var selectedcategory = model.catgories.Find(p => p.Value == model.CategoryCode.ToString());
                //if (selectedBranch != null)
                //{
                //    selectedBranch.Selected = true;

                //}
                //if (selectedAccType != null)
                //{
                //    selectedAccType.Selected = true;

                //}
                //if (selectedCurrency != null)
                //{
                //    selectedCurrency.Selected = true;

                //}
                //if (selectedcategory != null)
                //{
                //    selectedcategory.Selected = true;

                //}

                //String CategoryCode = Session["modelCategoryCode"].ToString();
                //model.CategoryCode = CategoryCode;

                //while (model.AccountNumber.ToString().Length != 7)
                //{
                //    model.AccountNumber = "0" + model.AccountNumber;
                //}
                ////String Accountnumber = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                //String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;


                //String response = Connecttocore.GetCustinfo(model.AccountNumber);
                //JObject jobj = new JObject();
                //jobj = JObject.Parse(response);
                //dynamic result = jobj;

                //string responseStatus = result.responseStatus;
                //string responseMessage = result.responseMessage;
                //string bal = result.result;
                //string[] separators = { ",", ":" };
                //string value = bal;
                //string[] acc = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                //String custname;
                //String custID;
                //String custphone;

                //if (acc.Length >= 10)
                //{
                //    custID = acc[1].ToString();
                //    custname = acc[3].ToString();

                //    custphone = acc[5].ToString();
                //    Session["custID"] = custID;
                //    Session["custname"] = custname;
                //    Session["custphone"] = custphone;

                //    ViewBag.custname = custname;
                //    return custname;
                //    // return RedirectToAction(actionName: "newCustomerAccount", routeValues: new { Account = Accountnumber });
                //}
                //else
                //{
                //    ModelState.AddModelError("", "Please Check Customer Information");
                //}
            }
            catch (Exception ex)
            {
                message = "Please Contact for Support";
                ModelState.AddModelError("", "Something is missing" + message);
                return "No Customer Found";
            }
        }

        public ActionResult Delete()
        {
            String userbranch = "";
            if (Session["deleleaccountresult"] != null)
            {
                ViewBag.SuccessMessage = Session["deleleaccountresult"].ToString();
                Session["deleleaccountresult"] = null;
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
        public ActionResult Delete(CustomerRegBankinfo model)
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
                    //String Accountnumber = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    Session["accounttodeleteaccountfrom"] = Accountnumber;
                    String response = ds.custregcheck2(Accountnumber, model.CategoryCode);
                    if (response.Equals("This Account is Already exist"))
                    {
                        Session["modelCategoryCode"] = model.CategoryCode;
                        return RedirectToAction(actionName: "deleteCustomerAccount", routeValues: new { Account = Accountnumber });
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
        public ActionResult Deleteprocess(CustomerRegBankinfo passedmodel)
        {
            CustomerRegBankinfo model = new CustomerRegBankinfo();
            if (passedmodel.CustomerID != null)
            {
                model = new CustomerRegBankinfo();
                String userbranch = Session["user_branch"].ToString();
                model = ds.GetUserRegistrationData(passedmodel.CustomerID);

                model.Branches = ds.PopulateBranchs(model.BranchCode, passedmodel.CustomerID);
                model.AccTypes = ds.PopulateAccountTypes(passedmodel.CustomerID);
                model.Currencies = ds.PopulateCurrencies(model.CurrencyCode);

                model.catgories = ds.GetGatgories();
                model.Channels = ds.Channels();
                return View("Delete", model);
            }
            else
            {
                String userbranch = "";
                if (Session["deleleaccountresult"] != null)
                {
                    ViewBag.SuccessMessage = Session["deleleaccountresult"].ToString();
                    Session["deleleaccountresult"] = null;
                }

                userbranch = Session["user_branch"].ToString();
                model.Branches = ds.PopulateBranchs(userbranch);
                model.AccTypes = ds.PopulateAccountTypes();
                model.Currencies = ds.PopulateCurrencies();
                model.catgories = ds.GetGatgories();
                return View("Delete", model);
            }
        }

        public ActionResult deleteCustomerAccount(String Account)
        {
            CustomerRegBankinfo model = new CustomerRegBankinfo();

            Session["Accountold"] = "";
            Session["Accountold"] = Account;
            String CategoryCode = Session["modelCategoryCode"].ToString();

            custinfo infomodel = ds.getcustinfo(Account.Substring(2, 3), Account.Substring(5, 5), Account.Substring(13), Account.Substring(10, 3), CategoryCode, Account);
            String name = infomodel.user_name;
            String username = infomodel.user_log;
            ViewBag.name = name;
            ViewBag.username = username;
            Session["Accountoldname"] = "";
            Session["Accountoldname"] = name;
            Session["Accountoldusername"] = "";
            Session["Accountoldusername"] = username;
            Session["modelCategoryCode"] = CategoryCode;


            String userbranch = Session["user_branch"].ToString();
            if (name != "")
            {
                model = ds.GetUserRegistrationData(username);

                model.Branches = ds.PopulateBranchs(model.BranchCode, username);
                model.AccTypes = ds.PopulateAccountTypes(username);
                model.Currencies = ds.PopulateCurrencies();
            }

            model.Branches = ds.PopulateBranchs(userbranch);
            model.AccTypes = ds.PopulateAccountTypes();
            model.Currencies = ds.PopulateCurrencies();
            model.catgories = ds.GetGatgories();


            return View(model);

        }

        [HttpPost]
        public ActionResult deleteCustomerAccount(CustomerRegBankinfo model, string command)
        {
            String CategoryCode = Session["modelCategoryCode"].ToString();
            model.CategoryCode = CategoryCode;

            ViewBag.name = Session["Accountoldname"].ToString();

            ViewBag.username = Session["Accountoldusername"].ToString();
            if (command == "Check")
            {
                String name = checkdeleteCustomerAccount(model);
                if (name != "No Customer Found")
                {
                    // do stuff  
                    ViewBag.msg = name;
                    return View(model);
                }
                else
                    ModelState.AddModelError("", name);
                return View(model);
            }
            else
                if (command == "Delete")
            {


                String message;
                //  account model;
                try
                {
                    String userbranch = Session["user_branch"].ToString();


                    model.Branches = ds.PopulateBranchs(userbranch);
                    model.AccTypes = ds.PopulateAccountTypes();
                    model.Currencies = ds.PopulateCurrencies();

                    var selectedBranch = model.Branches.Find(p => p.Value == model.BranchCode.ToString());
                    var selectedAccType = model.AccTypes.Find(p => p.Value == model.AccountTypecode.ToString());
                    var selectedCurrency = model.Currencies.Find(p => p.Value == model.CurrencyCode.ToString());
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


                    if (ModelState.IsValidField("BranchCode") && ModelState.IsValidField("AccountTypecode") && ModelState.IsValidField("CurrencyCode") && ModelState.IsValidField("AccountNumber"))
                    {
                        //String Accountnumber = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                        String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;

                        String result2 = ds.deleteaccount(Session["Accountold"].ToString(), Accountnumber, CategoryCode);
                        String res = " " + Accountnumber.Substring(11, 7) + " : " + result2;

                        string custname = Session["Accountoldname"].ToString();//ds.getcustomerfullname(Accountnumber);
                        //string usershorthand = "11" + model.BranchCode + model.AccountNumber;
                        string usershorthand = model.AccountNumber;
                        string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                        ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Account link request", usershorthand + " - " + custname, DateTime.Now.ToString());

                        Session["deleleaccountresult"] = res;
                        return RedirectToAction("Delete");
                        // return RedirectToAction(actionName: "newCustomerAccount", routeValues: new { Account = Accountnumber });

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
            else
            {
                ModelState.AddModelError("", "Please check one of buttons");
                return View(model);
            }
        }


        [HttpPost]

        public String checkdeleteCustomerAccount(CustomerRegBankinfo model)
        {

            String message;
            //  account model;
            try
            {
                String userbranch = Session["user_branch"].ToString();


                model.Branches = ds.PopulateBranchs(userbranch);
                model.AccTypes = ds.PopulateAccountTypes();
                model.Currencies = ds.PopulateCurrencies();
                model.catgories = ds.GetGatgories();

                var selectedBranch = model.Branches.Find(p => p.Value == model.BranchCode.ToString());
                var selectedAccType = model.AccTypes.Find(p => p.Value == model.AccountTypecode.ToString());
                var selectedCurrency = model.Currencies.Find(p => p.Value == model.CurrencyCode.ToString());
                //     var selectedcategory = model.catgories.Find(p => p.Value == model.CategoryCode.ToString());
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
                //if (selectedcategory != null)
                //{
                //    selectedcategory.Selected = true;

                //}

                String CategoryCode = Session["modelCategoryCode"].ToString();
                model.CategoryCode = CategoryCode;
                if (ModelState.IsValidField("BranchCode") && ModelState.IsValidField("AccountTypecode") && ModelState.IsValidField("CurrencyCode") && ModelState.IsValidField("AccountNumber"))
                {

                    while (model.AccountNumber.ToString().Length != 7)
                    {
                        model.AccountNumber = "0" + model.AccountNumber;
                    }
                    //String Accountnumber = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;

                    string userid = ds.getCustIDFromAcc(Session["accounttodeleteaccountfrom"].ToString());
                    Boolean doseitbelongsto = ds.checkaccountbelongstouser(userid, Accountnumber);
                    if (doseitbelongsto)
                    {
                        String response = Connecttocore.GetCustinfo(Accountnumber);
                        JObject jobj = new JObject();
                        jobj = JObject.Parse(response);
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

                            ViewBag.custname = custname;
                            return custname;
                            // return RedirectToAction(actionName: "newCustomerAccount", routeValues: new { Account = Accountnumber });
                        }
                        else
                        {
                            ModelState.AddModelError("", "Please Check Customer Information");
                        }
                    }
                    else
                    {
                        message = " this account : '" + model.AccountNumber + "' is not added to this specific users";
                        ModelState.AddModelError("", "Something is wrong" + message);
                        return "Account not added to this specific users";
                    }
                }
                else
                {
                    message = "All Fields are required ";
                    ModelState.AddModelError("", "Something is missing" + message);
                    return "No Customer Found";
                }
            }
            catch (Exception ex)
            {
                message = "Please Contact for Support";
                ModelState.AddModelError("", "Something is missing" + message);
                return "No Customer Found";
            }

            return "No Customer Found";
        }

        public ActionResult CustomerAccounts(String Account)
        {
            try
            {
                string response = Connecttocore.GetCustaccounts(Account);
                //"{'1':{'ACT_C_NAME':'¿¿¿¿ ¿¿¿¿¿ ¿¿¿¿ ¿¿¿¿¿¿¿¿','CURRENCY_C_CODE':'001','CUST_I_NO':'467','MOBILE_C_NO':'0','BRANCH_C_CODE':'004','ACT_C_TYPE':'20102'},'tranDateTime':'180118082930','NoOfAct':2,'2':{'ACT_C_NAME':'¿¿¿¿ ¿¿¿¿¿ ¿¿¿¿ ¿¿¿¿¿¿¿¿','CURRENCY_C_CODE':'001','CUST_I_NO':'82','MOBILE_C_NO':'0','BRANCH_C_CODE':'004','ACT_C_TYPE':'20105'},'uuid':'d0088690-368a-4737-a9e0-5f330add73c1','errormsg':'Successfully','errorcode':'1'}";//Connecttocore.GetCustaccounts(Account);
                JObject jobj = new JObject();
                jobj = JObject.Parse(response);
                dynamic result = jobj;
                List<addaccount> items = new List<addaccount>();
                string errormsg = result.errormsg;
                string errorcode = result.errorcode;
                string NoOfAct = result.NoOfAct;
                if (errorcode.Equals("1") && !NoOfAct.Equals("0"))
                {
                    int index = Convert.ToInt32(NoOfAct);
                    for (int i = 1; i <= index; i++)
                    {

                        try
                        {

                            JToken singlerow = result[i.ToString()];
                            JObject newObj = new JObject();
                            dynamic singleObj = singlerow;
                            String Branchname = singleObj.BRANCH_C_CODE;
                            String AccountTypename = singleObj.ACT_C_TYPE;// ds.getaccounttype(singleObj.ACT_C_TYPE);
                            String Currencyname = singleObj.CURRENCY_C_CODE;// ds.getcurrencyname(singleObj.CURRENCY_C_CODE);
                            String accno = singleObj.CUST_I_NO;
                            if (!Account.Substring(13).Equals(accno))
                            {
                                items.Add(new addaccount
                                {
                                    AccountID = i + 1,
                                    AccountNumber = singleObj.CUST_I_NO,
                                    AccountNumbercomplete = "13" + singleObj.BRANCH_C_CODE + singleObj.ACT_C_TYPE + singleObj.CURRENCY_C_CODE + singleObj.CUST_I_NO,
                                    Branch = ds.getbranchnameenglish(Branchname),
                                    AccountType = ds.getaccounttype(AccountTypename),
                                    Currency = ds.getcurrencyname(Currencyname),
                                    IsSelected = false,
                                });
                            }
                            Session["Accountold"] = Account;
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("", "System Error");

                        }



                    }

                    accountsresult accountsresult = new accountsresult();
                    accountsresult.accountSelected = items;
                    return View(accountsresult);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "System Error");

            }
            return View();
        }
        //[HttpPost]
        //public ActionResult CustomerAccounts(accountsresult model)
        //{
        //    String result = "", res = "";
        //    List<addaccount> lHob = new List<addaccount>();
        //    lHob = model.accountSelected;
        //    foreach (var item in lHob)
        //    {
        //        if (item.IsSelected == true)
        //        {
        //            result = ds.addnewacount(Session["Accountold"].ToString(), item.AccountNumbercomplete, Session["modelCategoryCode"].ToString());
        //            res += " " + item.AccountNumbercomplete + " : " + result;

        //        }
        //        Session["addaccountresult"] = res;
        //    }
        //    return RedirectToAction("Add");
        //}


        public ActionResult Authorizer()
        {
            if (Session["authresult"] != null)
            {
                ViewBag.SuccessMessage = Session["authresult"].ToString();
                Session["authresult"] = null;
            }

            Session["bracode"] = "000";
            String branchcode = Session["bracode"].ToString();

            List<pendingacts> customer = new List<pendingacts>();
            customer = ds.Pendingacounts(branchcode);

            return View(customer);

        }
        public ActionResult Details(int id, String act)
        {
            actAuthorizationinfo model = new actAuthorizationinfo();
            Session["CustomerBranchCode"] = act.Substring(2, 3);
            List<actAuthorizationinfo> customer = new List<actAuthorizationinfo>();
            customer = ds.newactAuthorizationinfo(id.ToString(), act);

            Session["customer"] = customer;
            foreach (var item in customer)
            {
                model.Branch = item.Branch;
                model.AccountType = item.AccountType;
                model.Customername = item.Customername;
                model.Currency = item.Currency;
                model.Customeraccount = item.Customeraccount;
                model.completeact = item.completeact;
                model.userid = item.userid;

            }
            model.authsts = "true";
            model.rjtsts = "false";
            Session["model"] = model;
            return View(model);
        }
        public ActionResult Authorize(int id, String act)
        {

            int response = ds.updateAccount(id.ToString(), act, "A");
            if (response != -1)
            {
                actAuthorizationinfo model = new actAuthorizationinfo();
                model = (actAuthorizationinfo)Session["model"];
                string custname = model.Customername;
                //string usershorthand = "11" + Session["CustomerBranchCode"].ToString() + model.Customeraccount;
                string usershorthand = model.Customeraccount;
                string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Authorized account link", usershorthand + " - " + custname, DateTime.Now.ToString());

                Session["authresult"] = "Account Authorization Completed Successfuly";
                //return RedirectToAction("Index", "Home", new { area = "" });
                return RedirectToAction("Authorizer");
            }
            else
            {
                return RedirectToAction(actionName: "Details", routeValues: new { id = id, act = act });

                //return RedirectToAction("Details", id, act);
            }
        }
        public ActionResult Reject(int id, String act)
        {

            int response = ds.updateAccount(id.ToString(), act, "R");
            if (response != -1)
            {
                actAuthorizationinfo model = new actAuthorizationinfo();
                model = (actAuthorizationinfo)Session["model"];
                string custname = model.Customername;
                //string usershorthand = "11" + Session["CustomerBranchCode"].ToString() + model.Customeraccount;
                string usershorthand = model.Customeraccount;
                string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Rejected account link", usershorthand + " - " + custname, DateTime.Now.ToString());

                Session["authresult"] = "Reject Completed Successfuly";
                // return RedirectToAction("Index", "Home", new { area = "" });
                return RedirectToAction("Authorizer");
            }
            else
            {
                return RedirectToAction(actionName: "Details", routeValues: new { id = id, act = act });

                //return RedirectToAction("Details", id, act);
            }
        }



        public ActionResult GetACC()
        {
            if (Session["addaccountresult"] != null)
            {
                ViewBag.SuccessMessage = Session["addaccountresult"].ToString();
                Session["addaccountresult"] = null;
            }
            CustomerRegBankinfo model = new CustomerRegBankinfo();
            String userbranch = Session["user_branch"].ToString();


            model.Branches = ds.PopulateBranchs(userbranch);
            model.AccTypes = ds.PopulateAccountTypes();
            model.Currencies = ds.PopulateCurrencies();
            model.catgories = ds.GetGatgories();

            //model.catgories.RemoveAt(0);
            return View(model);

        }
        [HttpPost]
        public ActionResult GetACC(CustomerRegBankinfo model)
        {
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
                    //String Accountnumber = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    String response = ds.custregcheck2(Accountnumber, model.CategoryCode);
                    if (response.Equals("This Account is Already exist"))
                    {
                        Session["modelCategoryCode"] = model.CategoryCode;
                        return RedirectToAction(actionName: "CustomerAccounts", routeValues: new { Account = Accountnumber });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please Check Customer Information");
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
    }
}