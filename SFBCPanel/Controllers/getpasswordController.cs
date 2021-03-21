using hashmakersol.pdfmaker;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
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
    public class getpasswordController : Controller
    {
        DataSource ds = new DataSource();
        Connecttocore core = new Connecttocore();
        //
        // GET: /getpassword/
        public ActionResult GetPassword()
        {
            //return View();
            Customerinfopass model = new Customerinfopass();
            String userbranch = Session["user_branch"].ToString();


            model.Branches = ds.PopulateBranchs(userbranch);
            model.AccTypes = ds.PopulateAccountTypes();
            model.Currencies = ds.PopulateCurrencies();
            model.catgories = ds.GetGatgories();

            Session["regmodel"] = model;
            return View(model);

        }

        [HttpPost]
        public ActionResult GetPassword(Customerinfopass model)
        {
            string message = "";
            try
            {
                String userbranch = Session["user_branch"].ToString();


                GETpassword model1 = new GETpassword();
                model.Branches = ds.PopulateBranchs(userbranch);
                model.AccTypes = ds.PopulateAccountTypes();
                model.Currencies = ds.PopulateCurrencies();
                model.catgories = ds.GetGatgories();
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

                    String fullaccount = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                    Session["getpasswordfullaccount"] = fullaccount;
                    infomodel = ds.getcustinfo(model.BranchCode, model.AccountTypecode, model.AccountNumber, model.CurrencyCode, model.CategoryCode, fullaccount);
                    response = infomodel.lblconfirm;
                    if (response.Equals("This Account is Already exist") && infomodel.status.ToString().Equals("U"))
                    {
                        String Accountnumber = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;
                        //String Accountnumber = model1.account;
                        List<GETpassword> result = new List<GETpassword>();
                        result = ds.getpassword(Accountnumber);
                        foreach (var item in result)
                        {
                            if (item.lblconfirm == "Successfully")
                            {
                                //ds.updatecustomer(infomodel.user_id.ToString(), "A");
                                //ds.updateAccount(infomodel.user_id.ToString(), Accountnumber.ToString(), "A");

                                model1.name = item.name;
                                model1.account = item.account;
                                model1.branchname = item.branchname;
                                model1.pass = item.pass;
                                model1.fullaccount = fullaccount;
                                Session["pgetpassresult"] = model1;
                                return RedirectToAction("Print", "getpassword");
                                //return RedirectToAction("Print","getpassword");
                            }
                            else
                            {
                                ModelState.AddModelError("", item.lblconfirm);
                            }
                        }
                    }

                    else if (response.Equals("This Account is Already exist") && infomodel.status.ToString().Equals("P"))
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

                    else if (response.Equals("This Account is Already exist") && infomodel.status.ToString().Equals("A"))
                    {
                        message = "This Customer Account Is  activated already";
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
        public ActionResult GetPasswordprocess(Customerinfopass passedmodel)
        {
            Customerinfopass model = new Customerinfopass();
            if (passedmodel.Branch != null)
            {
                model = new Customerinfopass();
                String userbranch = Session["user_branch"].ToString();
                model = ds.GetUserinfoData(passedmodel.Branch);
                model.Branches = ds.PopulateBranchs(model.BranchCode);
                model.AccTypes = ds.PopulateAccountTypes(passedmodel.Branch);
                model.Currencies = ds.PopulateCurrencies(model.CurrencyCode);

                model.catgories = ds.GetGatgories();
                return View("GetPassword", model);
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
                return View("GetPassword", model);
            }
        }

        public ActionResult smspassword(string password, string account)
        {
            custinfo customerinformations = ds.getcustinfo("", "", "", "", "", account);
            string msg = "Your Account temporery password is : " + password;
            string response = core.sendotp(customerinformations.user_id, msg, customerinformations.user_mobile);
            JObject jobj = new JObject();
            jobj = JObject.Parse(response);
            dynamic result = jobj;

            var errorCode = result.errorcode;
            var errormsg = result.errormsg;
            var Status = result.status;

            if (Status == 1)
            {
                string customeraccount = Session["getpasswordfullaccount"].ToString();
                string userid = ds.getCustIDFromAcc(customeraccount);
                ds.updatecustomer(userid, "A");
                ds.updateAccount(userid, customeraccount, "A");
                string custname = Session["customername"].ToString();
                string usershorSthand = "18" + customeraccount.Substring(2, 3) + customeraccount.Substring(13);
                string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
                ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Password sent to customer via sms", usershorSthand + " - " + custname, DateTime.Now.ToString());

                TempData["Success"] = true;
                ViewBag.ResponseStat = "Successful";
                ViewBag.ResponseMSG = "Password sent to customer via sms successfully";
                ViewBag.SuccessMessage = "Password sent to customer via SMS.";
                return RedirectToAction("GetPassword");
            }
            else
            {
                TempData["Success"] = true;
                ViewBag.ResponseStat = "Not Successful";
                ViewBag.ResponseMSG = "Faild to send password sms, please try again.";
                ViewBag.SuccessMessage = "Password has not been sent yet.";
                return RedirectToAction("GetPassword");
            }
        }

        public ActionResult Print()
        {
            GETpassword model = new GETpassword();

            model = (GETpassword)Session["pgetpassresult"];
            Session["customername"] = model.name;
            Session["customeraccount"] = model.account;
            return View(model);
        }

        public ActionResult DownloadPdf()
        {
            List<GETpassword> model = new List<GETpassword>();
            model = (List<GETpassword>)Session["pgetpassresult"];
            return new PdfResult(model, "Print");
        }
        public FileResult SavePDF()
        {
            string custname = Session["customername"].ToString();
            string customeraccount = Session["getpasswordfullaccount"].ToString();
            string usershorSthand = "18" + customeraccount.Substring(2, 3) + customeraccount.Substring(13);
            string adminbranch = ds.getbranchnameenglish(Session["user_branch"].ToString());
            ds.insertadminslog(Session["UserId"].ToString(), Session["user_name"].ToString(), adminbranch, Session["user_roleid"].ToString(), Session["user_status"].ToString(), "Password printed to customer", usershorSthand + " - " + custname, DateTime.Now.ToString());

            //List < Employee > employees = _context.employees.ToList < Employee > ();  
            GETpassword model = new GETpassword();

            model = (GETpassword)Session["pgetpassresult"];

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




            tableLayout.AddCell(new PdfPCell(new Phrase("SSB Internet Banking", new Font(Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
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

            GETpassword model = new GETpassword();

            model = (GETpassword)Session["pgetpassresult"];


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