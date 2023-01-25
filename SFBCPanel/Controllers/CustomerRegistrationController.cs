using Newtonsoft.Json.Linq;
using SFBCPanel.Context;
using SFBCPanel.Models;
using SIBCPanel.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SFBCpanel.Controllers
{
    public class CustomerRegistrationController : Controller
    {
        DataSource ds = new DataSource();

        //
        // GET: /CustomerRegistration/
        public ActionResult Registration()
        {
            //System.Int64 timeout = System.Web.HttpContext.Current.Session.Timeout;
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["message"] != null)
            {
                ViewBag.SuccessMessage = Session["message"].ToString();
                Session["message"] = null;
            }
            CustomerRegBankinfo model = new CustomerRegBankinfo();
            String userbranch = Session["user_branch"].ToString();
            model.Branches = ds.PopulateBranchs(userbranch);
            model.AccTypes = ds.PopulateAccountTypes();
            model.Currencies = ds.PopulateCurrencies();

            model.catgories = ds.GetGatgories();
            model.Channels = ds.Channels();
            Session["regmodel"] = model;
            return View(model);
        }

        [HttpPost]
        public ActionResult Registration(CustomerRegBankinfo model)
        {
            string message = "";
            ModelState.Clear();
            try
            {
                String userbranch = Session["user_branch"].ToString();
                model.Branches = ds.PopulateBranchs(userbranch);
                model.AccTypes = ds.PopulateAccountTypes();
                model.Currencies = ds.PopulateCurrencies();
                model.catgories = ds.GetGatgories();
                //model.catgories.RemoveAt(0);
                model.Channels = ds.Channels();
                //var selectedBranch = model.Branches.Find(p => p.Value == model.BranchCode.ToString());
                //var selectedAccType = model.AccTypes.Find(p => p.Value == model.AccountTypecode.ToString());
                //var selectedCurrency = model.Currencies.Find(p => p.Value == model.CurrencyCode.ToString());
                var selectedcategory = model.catgories.Find(p => p.Value == model.CategoryCode.ToString());
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
                if (selectedcategory != null)
                {
                    selectedcategory.Selected = true;

                }


                if (ModelState.IsValid)
                {
                    // verifying if account already exists
                    //string tempnumber = model.AccountNumber;
                    //string shortaccount = model.AccountNumber;
                    //while (tempnumber.Length != 7)
                    //{
                    //    tempnumber = "0" + tempnumber;
                    //}
                    //model.AccountNumber = tempnumber;
                    //String response = ds.custregcheck(model.BranchCode, model.AccountTypecode, model.AccountNumber, model.CurrencyCode, model.CategoryCode,model.SUBNO,model.SUBGL);

                    //if (response.Equals("This Account is available"))
                    //{
                    //String act = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    ////act = "1101601560001634680001000"; 18001010007361000300
                    //Session["Account"] = act;
                    //Session["branchcode"] = model.BranchCode;
                    //Session["shortaccount"] = shortaccount;
                    string AccountNumber1, AccountType, BranchName, Currency;
                    string response = Connecttocore.GetCustinfobycif(model.cif); //"{\"tranDateTime\":\"160221114201\",\"CustomerNameAR\":\"نهاد هاشم مصطفى محمد\",\"CustomerMobile\":\"00249126929406\",\"CustomerAccounts\":\"18001020009384107400-18002020009384107400\",\"CustomerId\":\"0073903\",\"CustomerName\":\"NIHAD HASHIM MUSTAFA MOHAMMED\",\"uuid\":\"8a902d96-24e0-4f6e-9946-ee9084bcc349\",\"errormsg\":\"Secussfully\",\"errorcode\":\"1\",\"CustomerAddress\":\"KHARTOUM MANSHYA\"}"; //Connecttocore.GetCustinfobycif(model.cif);
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
                        Session["CustID"] = model.cif;
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

                        if (model.SelectedChannelsID.Length.Equals(model.Channels.Count))
                        {
                            Session["service"] = "3";
                        }
                        else
                        {
                            foreach (var item in model.SelectedChannelsID)
                            {

                                if (item.Equals("2"))
                                {  //append each checked records into StringBuilder   
                                    Session["ebranch"] = "T";
                                    Session["service"] = "2";
                                }
                                else if (item.Equals("1"))
                                {  //append each checked records into StringBuilder   
                                    Session["ebank"] = "T";
                                    Session["service"] = "1";
                                }
                                else
                                {
                                    Session["service"] = "0";
                                }


                            }
                        }
                        return RedirectToAction("custinfo");
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

                    //string responseStatus = result.responseStatus;
                    //string responseMessage = result.responseMessage;
                    //string bal = result.result;
                    //string[] separators = { ",", ":" };
                    //string value = bal;
                    //string[] acc = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    //String custname;
                    //String custID;
                    //String custphone;
                    //String custaddress;
                    //String custnamearabic;

                    //if (acc.Length >= 10)
                    //{
                    //    custID = acc[1].ToString();
                    //    custname = acc[3].ToString();
                    //    custnamearabic = acc[5].ToString();
                    //    custphone = acc[7].ToString();
                    //    custaddress = acc[9].ToString();

                    //    Session["custID"] = custID;
                    //    Session["custname"] = custname;
                    //    Session["custnamearabic"] = custnamearabic;
                    //    Session["custphone"] = custphone;
                    //    Session["CustomerAddress"] = custaddress;
                    //    Session["custcat"] = model.CategoryCode;
                    //    //string usershorthand = "11" + model.BranchCode + model.AccountNumber;
                    //    string usershorthand = model.AccountNumber;
                    //    string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                    //    ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Customer inquery", usershorthand + " - " + custname, DateTime.Now.ToString());

                    //    if (model.SelectedChannelsID.Length.Equals(model.Channels.Count))
                    //    {
                    //        Session["service"] = "3";
                    //    }
                    //    else
                    //    {
                    //        foreach (var item in model.SelectedChannelsID)
                    //        {

                    //            if (item.Equals("2"))
                    //            {  //append each checked records into StringBuilder   
                    //                Session["ebranch"] = "T";
                    //                Session["service"] = "2";
                    //            }
                    //            else if (item.Equals("1"))
                    //            {  //append each checked records into StringBuilder   
                    //                Session["ebank"] = "T";
                    //                Session["service"] = "1";
                    //            }
                    //            else
                    //            {
                    //                Session["service"] = "0";
                    //            }


                    //        }
                    //    }
                    //    return RedirectToAction("custinfo");
                    //}

                    //else
                    //{
                    //    message = "Please check customer information something wrong ";
                    //    ModelState.AddModelError("", message);
                    //    return View(model);
                    //}
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
        public ActionResult Registrationprocess(CustomerRegBankinfo model)
        {
            return View("Registration", model);
        }

        public ActionResult custinfo()
        {
            CustomerRegBankinfo2 model = new CustomerRegBankinfo2();
            model.CustomerID = Session["custID"].ToString();
            if (Session["custname"] != null)
            {
                model.CustomerName = Session["custname"].ToString();

            }
            if (Session["custnamearabic"] != null)
            {
                model.CustomerNameArabic = Session["custnamearabic"].ToString();
            }
            if (Session["custphone"] != null)
            {
                model.CustomerPhone = Session["custphone"].ToString();
            }
            if (Session["CustomerAddress"] != null)
            {
                model.CustomerAddress = Session["CustomerAddress"].ToString();
            }
            ViewBag.CustomerAccounts = (List<SelectListItem>)Session["CustomerAccounts"];
            ViewData["customername"] = model.CustomerNameArabic;

            return View(model);
        }
        [HttpPost]
        public ActionResult custinfo(CustomerRegBankinfo2 model)
        {
            Session["Account"] = model.CustomerAccount;


            return RedirectToAction("Registrationpersonalinfo");
        }
        public ActionResult Registrationpersonalinfo()
        {
            CustomerRegpersonalinfo model = new CustomerRegpersonalinfo();
            model.Profiles = ds.PopulateProfiles();
            model.phonenumber = Session["custphone"].ToString();
            model.UserName = Session["custID"].ToString();

            return View(model);
        }
        [HttpPost]
        public ActionResult Registrationpersonalinfo(CustomerRegpersonalinfo model)
        {
            String message;
            model.Profiles = ds.PopulateProfiles();

            try
            {
                var selectedProfile = model.Profiles.Find(p => p.Value == model.profileCode.ToString());
                if (selectedProfile != null)
                {
                    selectedProfile.Selected = true;
                }
            }
            catch (Exception e)
            {
                message = "All Fields are required ";
                ModelState.AddModelError("", "Something is missing" + message);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                String username = model.UserName;
                String email = model.Email;
                String address = model.Address;
                String account = Session["Account"].ToString();
                String customerprofile = model.profileCode.ToString();
                String CustomerID = Session["custID"].ToString();
                String CustomerName = "N/A";
                if (Session["custnamearabic"] != null)
                {
                    CustomerName = Session["custnamearabic"].ToString();
                }
                else if (Session["custname"] != null)
                {
                    CustomerName = Session["custname"].ToString();
                }
                String CustomerPhone = model.phonenumber;//Session["custphone"].ToString();
                String customercatgory = Session["custcat"].ToString();
                String CUSTOMERSERVICE = Session["service"].ToString();
                string adminusername = Session["custID"].ToString();

                int response = ds.custreg(CustomerID, CustomerName, account, username, address, CustomerPhone, email, customerprofile, customercatgory, CUSTOMERSERVICE, adminusername);

                if (response.Equals(1))
                {
                    string usershorthand = Session["custID"].ToString();
                    string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                    ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Customer registration", usershorthand + " - " + CustomerName, DateTime.Now.ToString());
                    message = "Customer Registration Complete Successfully";
                    Session["message"] = message;
                    // ModelState.AddModelError("", message );
                    return RedirectToAction("Registration");
                }
                else if (response.Equals(2))
                {
                    message = "This customer is registered already.";
                    Session["message"] = message;
                    // ViewBag.SuccessMessage = message;
                    //return View(model);
                    return RedirectToAction("Registration");
                }
                else
                {
                    message = "please check customer and information.";
                    Session["message"] = message;
                    //ModelState.AddModelError("", message);
                    //return View(model);
                    return RedirectToAction("Registration");
                }
            }
            else
            {
                message = "All Fields are required ";
                ModelState.AddModelError("", "Something is missing" + message);
                return View(model);
            }

        }
    }
}