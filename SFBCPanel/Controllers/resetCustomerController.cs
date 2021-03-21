using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json.Linq;
using SFBCPanel.Context;
using SFBCPanel.Models;
using SIBCPanel.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Cpanel.Controllers
{
    public class resetCustomerController : Controller
    {
        DataSource ds = new DataSource();
        Connecttocore core = new Connecttocore();
        //
        // GET: /resetCustomer/
        public ActionResult ResetCust()
        {
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            String userbranch = Session["user_branch"].ToString();


            Customerinfopass model = new Customerinfopass();
            model.Branches = ds.PopulateBranchs(userbranch);
            model.AccTypes = ds.PopulateAccountTypes();
            model.Currencies = ds.PopulateCurrencies();
            model.catgories = ds.GetGatgories();

            Session["regmodel"] = model;
            return View(model);
        }

        [HttpPost]
        public ActionResult ResetCust(Customerinfopass model)
        {
            resetpass restmodel = new resetpass();
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


                if (ModelState.IsValid)
                {
                    custinfo infomodel = new custinfo();

                    String response;
                    String fullaccountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    infomodel = ds.getcustinfo(model.BranchCode, model.AccountTypecode, model.AccountNumber, model.CurrencyCode, model.CategoryCode,fullaccountnumber);
                    response = infomodel.lblconfirm;

                    if (response.Equals("This Account is Already exist") && infomodel.status.ToString().Equals("A"))
                    {
                        //String Accountnumber = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode +
                        //    model.AccountNumber;
                        String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;

                        //String Accountnumber = restmodel.account;
                        List<resetpass> result = new List<resetpass>();
                        result = ds.resetpassword(Accountnumber);
                        foreach (var item in result)
                        {
                            if (item.lblconfirm == "Successfully")
                            {
                                restmodel.name = item.name;
                                restmodel.account = item.account;
                                restmodel.branchname = item.branchname;
                                restmodel.pass = item.pass;
                                Session["presetpassresult"] = restmodel;
                                return RedirectToAction("Print","resetCustomer");
                            }
                            else
                            {
                                ModelState.AddModelError("", item.lblconfirm);
                            }
                        }


                        return View(model);
                    }
                    else if (response.Equals("This Account is Already exist") && infomodel.status.ToString().Equals("B"))
                    {
                        String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                        //String Accountnumber = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;

                        //String Accountnumber = restmodel.account;
                        List<resetpass> result = new List<resetpass>();
                        result = ds.resetpassword(Accountnumber);

                        foreach (var item in result)
                        {
                            if (item.lblconfirm == "Successfully")
                            {
                                restmodel.name = item.name;
                                restmodel.account = item.account;
                                restmodel.branchname = item.branchname;
                                restmodel.pass = item.pass;
                                Session["presetpassresult"] = restmodel;
                                return RedirectToAction("Print", "resetCustomer");
                            }
                            else
                            {
                                ModelState.AddModelError("", item.lblconfirm);
                            }
                        }


                        return View(model);
                    }

                    else if (response.Equals("This Account is Already exist") && infomodel.status.ToString().Equals("P"))
                    {

                        message = "This Customer Account Is Not Authorized";
                        ModelState.AddModelError("", message);
                        return View(model);
                    }

                    else if (response.Equals("This Account is not activated yet") && infomodel.status.ToString().Equals("U"))
                    {

                        message = "This Customer Account Is Not Authorized";
                        ModelState.AddModelError("", message);
                        return View(model);
                    }

                    else if (response.Equals("This Account is Already exist") && infomodel.status.ToString().Equals("R"))
                    {
                        message = "This Customer Account Is Rejected";
                        ModelState.AddModelError("", message);
                        return View(model);

                    }


                    else if (response.Equals("This Account is Already exist") && infomodel.status.ToString().Equals("D"))
                    {
                        message = "This Customer Account Is Deleted or Deactivated";
                        ModelState.AddModelError("", message);
                        return View(model);


                    }
                    else if (response.Equals("This Account is Already exist") && infomodel.status.ToString().Equals("S"))
                    {
                        message = "This Customer Account Is Stoped";
                        ModelState.AddModelError("", message);
                        return View(model);


                    }
                    else
                    {
                        message = "This Customer Account Is Not Register";
                        ModelState.AddModelError("", message);
                        return View(model);


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
        public ActionResult ResetCustprocess(Customerinfopass passedmodel)
        {
            Customerinfopass model = new Customerinfopass();
            if (passedmodel.Branch != null)
            {
                model = new Customerinfopass();
                String userbranch = Session["user_branch"].ToString();
                model = ds.GetUserinfoData(passedmodel.Branch);
                model.Branches = ds.PopulateBranchs(model.BranchCode, passedmodel.Branch);
                model.AccTypes = ds.PopulateAccountTypes(passedmodel.Branch);
                model.Currencies = ds.PopulateCurrencies(model.CurrencyCode);

                model.catgories = ds.GetGatgories();
                return View("ResetCust", model);
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
                return View("ResetCust", model);
            }
            
        }

        public ActionResult smspassword(string password,string account)
        {
            custinfo customerinformations = ds.getcustinfo("","","","","",account);
            string msg = "Your Account temporery password is : "+password;
            string response = core.sendotp(customerinformations.user_id, msg, customerinformations.user_mobile);
            JObject jobj = new JObject();
            jobj = JObject.Parse(response);
            dynamic result = jobj;

            var errorCode = result.errorcode;
            var errormsg = result.errormsg;
            var Status = result.status;

            if (Status == 1)
            {
                string custname = customerinformations.name;
                string customeraccount = account;
                string usershorSthand = "18" + customeraccount.Substring(3, 3) + customeraccount.Substring(13);
                string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Reset password sent to customer vis sms", usershorSthand + " - " + custname, DateTime.Now.ToString());

                TempData["Success"] = true;
                ViewBag.ResponseStat = "Successful";
                ViewBag.ResponseMSG = "Password sent to customer via sms successfully";
                ViewBag.SuccessMessage = "Password sent to customer via SMS.";
                return RedirectToAction("ResetCust");
            }
            else
            {
                TempData["Success"] = true;
                ViewBag.ResponseStat = "Not Successful";
                ViewBag.ResponseMSG = "Faild to send password sms, please try again.";
                ViewBag.SuccessMessage = "Message was not sent to customer, Please try again.";
                return RedirectToAction("ResetCust");
            }
        }

        public ActionResult Print()
        {
            resetpass model = new resetpass();

            model = (resetpass)Session["presetpassresult"];
            return View(model);
        }


        public FileResult SavePDF()
        {
            //List < Employee > employees = _context.employees.ToList < Employee > ();  
            resetpass model = new resetpass();

            model = (resetpass)Session["presetpassresult"];

            string custname = model.name;
            string customeraccount = model.account;
            string usershorSthand = "18" + customeraccount.Substring(3,3) + customeraccount.Substring(13);
            string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
            ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Reset password printed to customer", usershorSthand + " - " + custname, DateTime.Now.ToString());


            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created   
            string strPDFFileName = string.Format("Customerpassword - " + model.account.ToString() + " - " + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 5 columns  
            PdfPTable tableLayout = new PdfPTable(5);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table  

            //file will created in this path  
            string strAttachment = Server.MapPath("~/Downloadss/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF   

            doc.Add(Add_Content_To_PDF(tableLayout));

            // Closing the document  
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        protected PdfPTable Add_Content_To_PDF(PdfPTable tableLayout)
        {

            float[] headers = { 50, 24, 45, 35, 50 }; //Header Widths  
            tableLayout.SetWidths(headers); //Set the pdf headers  
            tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
            tableLayout.HeaderRows = 1;
            //Add Title to the PDF file at the top  




            tableLayout.AddCell(new PdfPCell(new Phrase("SFB Internet Banking", new Font(Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
            {
                Colspan = 12,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });


            ////Add header 

            AddCellToHeader(tableLayout, "Customer Account");
            AddCellToHeader(tableLayout, "Customer Branch");
            AddCellToHeader(tableLayout, "Customer Name");
            AddCellToHeader(tableLayout, "Customer Password");
            AddCellToHeader(tableLayout, "Date");

            ////Add body  

            resetpass model = new resetpass();

            model = (resetpass)Session["presetpassresult"];


            AddCellToBody(tableLayout, model.account.ToString());
            AddCellToBody(tableLayout, model.branchname.ToString());
            AddCellToBody(tableLayout, model.name.ToString());
            AddCellToBody(tableLayout, model.pass.ToString());
            AddCellToBody(tableLayout, DateTime.Now.ToString());



            return tableLayout;
        }


        // Method to add single cell to the Header  
        private static void AddCellToHeader(PdfPTable tableLayout, string cellText)
        {

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE)))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                BackgroundColor = new iTextSharp.text.BaseColor(128, 128, 128)
            });
        }

        // Method to add single cell to the body  
        private static void AddCellToBody(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.BLACK)))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
            });
        }
    }
}