using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SFBCpanel.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SFBCPanel.Context;
using SFBCPanel.Models;
using SIBCPanel.Context;
using Newtonsoft.Json.Linq;
using System.Data.OracleClient;
using System.Data;
using Microsoft.Ajax.Utilities;
using System.Web.Services.Description;

namespace SFBCpanel.Controllers
{
    public class CustomerReportController : Controller
    {
        DataSource ds = new DataSource();
        //
        // GET: /CustomerReport/
        public ActionResult CustomersReport()
        {
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            Custreport model = new Custreport();
            String userbranch = Session["user_branch"].ToString();


            model.Branches = ds.PopulateBranchs(userbranch);
            model.catgories = ds.GetGatgories();
            model.CustomerStatus = ds.PopulateCustStatus();

            Session["CustReport"] = model;

            return View(model);
        }

        [HttpPost]
        public ActionResult CustomersReport(Custreport model)
        {
            string message = "";

            try
            {
                String userbranch = Session["user_branch"].ToString();


                model.Branches = ds.PopulateBranchs(userbranch);
                model.catgories = ds.GetGatgories();
                model.CustomerStatus = ds.PopulateCustStatus();

                var selectedBranch = model.Branches.Find(p => p.Value == model.BranchCode.ToString());
                var selectedCategory = model.catgories.Find(p => p.Value == model.CategoryCode.ToString());
                var selectedStatus = model.CustomerStatus.Find(p => p.Value == model.StatusCode.ToString());

                if (selectedBranch != null)
                {
                    selectedBranch.Selected = true;

                }
                if (selectedCategory != null)
                {
                    selectedCategory.Selected = true;

                }
                if (selectedStatus != null)
                {
                    selectedStatus.Selected = true;

                }


                if (ModelState.IsValid)
                {
                    List<Custreport> accass = new List<Custreport>();

                    accass = ds.GetBranchUsers(model.BranchCode, model.CategoryCode, model.StatusCode);
                    if (accass.Count > 0)
                    {
                        if (model.BranchCode != "0")
                            Session["Branchname"] = ds.getbranchnameenglish(model.BranchCode);
                        else
                            Session["Branchname"] = "All Branches";
                        Session["BranchUsers"] = accass;
                        return RedirectToAction("ViewReport");
                    }


                    else
                    {
                        message = "No Customer Registered";
                        ModelState.AddModelError("", message);
                        return View(model);
                    }
                }
                else
                {
                    message = "Please Contact us for Support";
                    ModelState.AddModelError("", "Something is missing" + message);
                    return View(model);
                }

            }
            catch (Exception e)
            {
                message = "Please Contact for Support";
                ModelState.AddModelError("", "Something is missing" + message);
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult CustomersReportprocess(Custreport passedmodel)
        {
            Custreport model = new Custreport();
            if (passedmodel.Branch != null)
            {
                model = new Custreport();
                String userbranch = Session["user_branch"].ToString();
                model = ds.GetCustomerReportData(passedmodel.Branch);
                //model.catgories = ds.GetGatgories();
                model.CustomerStatus = ds.PopulateCustStatus(passedmodel.Branch);
                //model.Branches = ds.PopulateBranchs(model.BranchCode, passedmodel.Branch);
                model.Branches = ds.PopulateBranchs(model.Branch, passedmodel.Branch);
                model.catgories = ds.GetGatgories();
                //model.catgories = ds.GetGatgories( passedmodel.Branch);
                model.catgories.RemoveAt(0);
                return View("CustomersReport", model);
            }
            else
            {
                String userbranch = Session["user_branch"].ToString();


                model.Branches = ds.PopulateBranchs(userbranch);
                model.catgories = ds.GetGatgories();
                model.CustomerStatus = ds.PopulateCustStatus();

                Session["CustReport"] = model;

                return View("CustomersReport", model);
            }
        }

        public ActionResult CustomersRegistrationReport(Custreport model)
        {
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            string adminbranch = Session["user_branch"].ToString();
            List<Custreport> customers = ds.getbranchcustomers(adminbranch);
            return View(customers);
        }

        public ActionResult BillersReport()
        {

            List<req_res_model> req_res_data = new List<req_res_model>();
            req_res_model model = new req_res_model();
            dynamic requestdata = null, responsedata = null;

            req_res_data = ds.getreq_res_log();
            Session["billersreport"] = req_res_data;
            List<SelectListItem> billers = new List<SelectListItem>();

            billers = ds.billers_statuses();


            //});
            Session["bilers"] = billers;

            return View();

        }

        public JsonResult FilteredBillersReport(string fromdate, string todate, string biller)
        {



            string formatedFromDate = DateTime.Parse(fromdate).ToString();
            string formatedtodate = DateTime.Parse(todate).ToString();
            string[] readyfromdate = formatedFromDate.Split(' ');
            string[] readytodate = formatedtodate.Split(' ');
            List<req_res_model> req_res_data = new List<req_res_model>();


            req_res_data = ds.getfilteredreq_res_log(fromdate, todate, biller);
            List<req_res_model> billersreport = new List<req_res_model>();

            billersreport = req_res_data;
            Session["billersreport"] = billersreport;
            JsonResult data = Json(new { data = billersreport }, JsonRequestBehavior.AllowGet);
            data.MaxJsonLength = int.MaxValue;
            return data;
        }

        public FileResult SavePDF4()
        {



            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created   
            string strPDFFileName = string.Format("CustomerCountReport For " /*+ Session["totalbillercustomer"].ToString() */+ dTime.ToString("ddMMMyyyyHHmmss") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 6 columns  
            /*PdfPTable tableLayout = new PdfPTable(5);*/
            PdfPTable tableLayout = new PdfPTable(9);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table  

            //file will created in this path  
            string strAttachment = Server.MapPath("~/Downloadss/" + strPDFFileName);

            PdfWriter.GetInstance(doc, workStream).CloseStream = false;

            doc.Open();

            //Add Content to PDF   

            doc.Add(Add_Content_To_PDF4(tableLayout));

            // Closing the document  
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        protected PdfPTable Add_Content_To_PDF4(PdfPTable tableLayout)
        {



            PdfPTableHeader tableHeader = new PdfPTableHeader();

            string fontpath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, true);
            float[] headers = { 25, 24, 45, 30, 30, 30, 25, 25 ,25 }; //Header Widths  
            tableLayout.SetWidths(headers); //Set the pdf headers  
            tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
            tableLayout.HeaderRows = 1;

            //Add Title to the PDF file at the top  

            //List < Employee > UserLog = _context.UserLog.ToList < Employee > (); 
            //List<SelectListItem> billers = Session["bilers"] as List<SelectListItem>;
            //List<CustomerTransferReportViewModel> req_res_data_biller = new List<CustomerTransferReportViewModel>();
            //req_res_data_biller = (List<CustomerTransferReportViewModel>)Session["billersreport"];

            List<req_res_model> req_res_data_biller = new List<req_res_model>();
            req_res_data_biller = (List<req_res_model>)Session["billersreport"];

            DateTime dTime = DateTime.Now;

            //paragraphs
            Paragraph Title = new Paragraph("SIB - Rabih" + "\n\n" + "Payments Report For Bilers",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            
            Paragraph Title2 = new Paragraph("",
               new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss") + "\n\n",
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Empty = new Paragraph("Empty",
            new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            //Chunk c = new Chunk("Total of Customers Registered : " + Session["totalbillercustomer"].ToString(),
            //    new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));

            //Paragraph Total = new Paragraph(c);
            //Adding Cells
            Paragraph empty = new Paragraph("\n\n",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            //Adding Cells

            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {
                Colspan = 8,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Title2))
            {
                Colspan = 8,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 2,
                PaddingRight = 10,
                Border = 0,
                PaddingBottom = 10,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_LEFT
            });
            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 6,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 10,
                PaddingTop = 5,

                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_RIGHT
            });
            tableLayout.AddCell(new PdfPCell(new Phrase(Title2))
            {
                Colspan = 8,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            //tableLayout.AddCell(new PdfPCell(new Phrase(empty))
            //{
            //    Colspan = 8,
            //    PaddingLeft = 60,
            //    Rowspan = 1,
            //    Border = 0,
            //    PaddingBottom = 15,
            //    PaddingTop = 15,
            //    HorizontalAlignment = Element.ALIGN_LEFT
            //});

            //Add header 

            AddCellToHeader4(tableLayout, "B_Id");
            AddCellToHeader4(tableLayout, "Date");
            AddCellToHeader4(tableLayout, "Biller name");
            AddCellToHeader4(tableLayout, "Voucher");
            AddCellToHeader4(tableLayout, "Bill amount");
            AddCellToHeader4(tableLayout, "Biller Ref");
            AddCellToHeader(tableLayout, "Bank Ref");
            AddCellToHeader4(tableLayout, "System Ref");
            AddCellToHeader4(tableLayout, " Status");


            ////Add body  

            foreach (var emp in req_res_data_biller)
            {

                AddCellToBody4(tableLayout, emp.bbl_id.ToString());
                AddCellToBody4(tableLayout, emp.TRAN_Data.ToString());
                AddCellToBody4(tableLayout, emp.sub_tran_name.ToString());
                AddCellToBody4(tableLayout, emp.VoucherRes.ToString());
                AddCellToBody4(tableLayout, emp.BILL_AMOUNT.ToString());
                AddCellToBody4(tableLayout, emp.BBL_BILLERREFRENCE.ToString());
                AddCellToBody4(tableLayout, emp.BBL_BNKREFRENCE.ToString());
                AddCellToBody4(tableLayout, emp.BBL_SYS_TRACENO.ToString());
                if (emp.status.ToLower().Equals("s"))
                {
                    AddCellToBody4(tableLayout, "Succeeded");
                }
                else if (emp.status.ToLower().Equals("f"))
                {
                    AddCellToBody4(tableLayout, "Failed");
                }
                else
                {
                    AddCellToBody4(tableLayout, emp.status);
                }



            }

            //tableLayout.AddCell(new PdfPCell(new Phrase(empty))
            //{
            //    Colspan = 4,
            //    PaddingLeft = 60,
            //    Rowspan = 3,
            //    Border = 1,
            //    PaddingTop = 20,

            //    PaddingBottom = 5,
            //    HorizontalAlignment = Element.ALIGN_LEFT

            //});
            
            return tableLayout;
        }

        private static void AddCellToHeader4(PdfPTable tableLayout, string cellText)
        {

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(67, 160, 106))))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                Border = Rectangle.BOX,
                BorderWidth = 1,
                BorderWidthLeft = 0,
                BorderWidthRight = 0,
                BorderWidthTop = 0,

                BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
            });
        }

        // Method to add single cell to the body  
        private static void AddCellToBody4(PdfPTable tableLayout, string cellText)
        {

            string fontpath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, true);

            const string regex_match_arabic_hebrew = @"[\u0600-\u06FF\u0590-\u05FF]+";
            if (Regex.IsMatch(cellText, regex_match_arabic_hebrew, RegexOptions.IgnoreCase))
            {
                tableLayout.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                tableLayout.AddCell(new PdfPCell(new Phrase(cellText,
                    new Font(basefont, 8, 1, iTextSharp.text.BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5,
                    Border = Rectangle.BOX,
                    BorderWidth = 1,
                    BorderWidthLeft = 0,
                    BorderWidthRight = 0,
                    BorderWidthTop = 0,
                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
                });
            }
            else
            {
                tableLayout.RunDirection = PdfWriter.RUN_DIRECTION_LTR;

                tableLayout.AddCell(new PdfPCell(new Phrase(cellText,
                    new Font(basefont, 8, 1, iTextSharp.text.BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    Padding = 5,
                    Border = Rectangle.BOX,
                    BorderWidth = 1,
                    BorderWidthLeft = 0,
                    BorderWidthRight = 0,
                    BorderWidthTop = 0,

                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
                });
            }
        }

        public ActionResult ViewReport()
        {
            List<Custreport> accass = new List<Custreport>();
            accass = (List<Custreport>)Session["BranchUsers"];
            ViewBag.Total = accass.Count;
            ViewBag.Date = DateTime.Now.ToString("dd-MMM-yyyy");
            //  ViewBag.Username = DateTime.Now.ToString("HH:mm:ss");
            ViewBag.Time = DateTime.Now.ToString("HH:mm:ss");
            ViewBag.Branchname = Session["Branchname"].ToString();
            Session["totalcustomer"] = accass.Count;
            return View(accass);
        }

        [HttpPost]
        public FileResult savecreditapireport(Custreport model)
        {
            string transtatus = model.CategoryCode;
            Session["transtatus"] = transtatus;

            string branchname = ds.getbranchnameenglish(model.BranchCode);

            if (branchname == "000")
                branchname = "All Branchs";

            Session["Branchname"] = branchname;
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("All Transactions Report For " + branchname + " With Status " + transtatus + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 5 columns
            PdfPTable tableLayout = new PdfPTable(7);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table

            //file will created in this path
            string strAttachment = Server.MapPath("~/Downloads/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF 
            doc.Add(Add_Content_To_Account_To_Card_PDF(tableLayout));

            // Closing the document
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        //public ActionResult CreditAPIReport()
        //{
        //    if (Session["user_name"] == null)
        //    {
        //        return RedirectToAction("Login", "Login");
        //    }
        //    if (Session["user_branch"] == null)
        //    {
        //        return RedirectToAction("Login", "Login");
        //    }
        //    List<CustomerTransferReportViewModel> creditapitransactions = ds.GetCreditAPITransaction();
        //    foreach (CustomerTransferReportViewModel transaction in creditapitransactions)
        //    {
        //        dynamic requestdata = JObject.Parse(transaction.TranFullReq);
        //        dynamic responsedata = JObject.Parse(transaction.TranFullResp);
        //        transaction.TranReqAmount = requestdata.tranamount;
        //        transaction.PAN = requestdata.PAN;
        //        transaction.TranFromAccount = requestdata.Fromaccount;
        //        transaction.CustomerName = requestdata.customerName;
        //        transaction.ResponseStatus = responsedata.responseStatus;
        //        transaction.RRN = responsedata.RRN;
        //        string word = responsedata.status;
        //        string[] words = word.Split(':');
        //        transaction.FT = words[1];
        //        string amorpm = transaction.TranDate.Substring(transaction.TranDate.Length - 2);
        //        transaction.TranDate = transaction.TranDate.Substring(0, 15) + " " + amorpm;
        //    }
        //    Session["creditapitransactions"] = creditapitransactions;
        //    return View(creditapitransactions);
        //}

        //[HttpGet]
        //public JsonResult DateFilteredCreditAPIReport(string fromdate, string todate)
        //{
        //    string formatedFromDate = DateTime.Parse(fromdate).ToString().Substring(0, 10);
        //    string formatedtodate = DateTime.Parse(todate).ToString().Substring(0, 10);

        //    List<CustomerTransferReportViewModel> creditapitransactions = ds.DateFilteredGetCreditAPITransaction(formatedFromDate, formatedtodate);
        //    foreach (CustomerTransferReportViewModel transaction in creditapitransactions)
        //    {
        //        dynamic requestdata = JObject.Parse(transaction.TranFullReq);
        //        dynamic responsedata = JObject.Parse(transaction.TranFullResp);
        //        transaction.TranFromAccount = requestdata.Fromaccount;
        //        transaction.PAN = requestdata.PAN;
        //        transaction.TranReqAmount = requestdata.tranamount;
        //        transaction.ResponseStatus = responsedata.responseStatus;
        //        transaction.RRN = responsedata.RRN;
        //        string word = responsedata.status;
        //        string[] words2 = word.Split(':');
        //        transaction.FT = words2[1];
        //        string amorpm = transaction.TranDate.Substring(transaction.TranDate.Length - 2);
        //        transaction.TranDate = transaction.TranDate.Substring(0, 15) + " " + amorpm;
        //        //transaction.CustomerName = requestdata.customerName;
        //    }

        //    JsonResult data = Json(new { data = creditapitransactions }, JsonRequestBehavior.AllowGet);
        //    return data;
        //}

        public ActionResult OverviewReport()
        {
            Custreport model = new Custreport();
            model.Branches = ds.PopulateBranchs();
            model.transactions_names = ds.populatetransactionsnames();

            List<CustomerTransferReportViewModel> accumulativereport = ds.TotalTransactionsAmountsPerBranch("000");
            Session["accumulativereport"] = accumulativereport;

            List<CustomerTransferReportViewModel> transactionperbranch = ds.GetTransactionPerBranch("All");
            Session["transactionperbranch"] = transactionperbranch;

            return View(model);
        }

        public ActionResult AllTransactionsReport()
        {
            Custreport model = new Custreport();
            model.Branches = ds.PopulateBranchs();
            model.transactions_statuses = ds.populatetransactionsstatuses();
            model.transactions_names = ds.populatetransactionsnames();

            List<CustomerTransferReportViewModel> alltransactions = ds.GetAllTransactions();

            foreach (CustomerTransferReportViewModel transaction in alltransactions)
            {
                string[] words = transaction.TranFullReq.ToString().Split(',');
                transaction.TranFromAccount = words[0];
                string amorpm = transaction.TranDate.Substring(transaction.TranDate.Length - 2);
                transaction.TranDate = transaction.TranDate.Substring(0, 15) + " " + amorpm;
            }

            Session["alltransactions"] = alltransactions;

            return View(model);
        }

        public ActionResult CustomersByAdmin()
        {
            CustomerReportModel customer = new CustomerReportModel();
            customer.admins = ds.populateadmins();

            List<CustomerReportModel> customers = new List<CustomerReportModel>();
            customers = ds.GetCustomersByAdmin("All");
            Session["customersbyadmin"] = customers;

            return View(customer);
        }

        public JsonResult FilteredCustomersByAdmin(string admin)
        {
            List<CustomerReportModel> customers = new List<CustomerReportModel>();
            customers = ds.GetCustomersByAdmin(admin);
            Session["customersbyadmin"] = customers;
            JsonResult data = Json(new { data = customers }, JsonRequestBehavior.AllowGet);
            return data;
        }

        public JsonResult FilteredOverviewReport(string branch_code)
        {
            List<CustomerTransferReportViewModel> accumulativereport = ds.TotalTransactionsAmountsPerBranch(branch_code);
            Session["accumulativereport"] = accumulativereport;
            JsonResult data = Json(new { data = accumulativereport }, JsonRequestBehavior.AllowGet);
            return data;
        }

        public JsonResult FilterTransactionsPerBranches(string transaction_name)
        {
            List<CustomerTransferReportViewModel> accumulativereport = ds.GetTransactionPerBranch(transaction_name);
            Session["transactionperbranch"] = accumulativereport;
            JsonResult data = Json(new { data = accumulativereport }, JsonRequestBehavior.AllowGet);
            return data;
        }

        public JsonResult FilterAllTransactionsReport(string branch_code, string status, string transaction_name)
        {
            List<CustomerTransferReportViewModel> alltransactions = ds.GetAllTransactions(branch_code, status, transaction_name);
            foreach (CustomerTransferReportViewModel transaction in alltransactions)
            {
                string[] words = transaction.TranFullReq.ToString().Split(',');
                transaction.TranFromAccount = words[0];
                string amorpm = transaction.TranDate.Substring(transaction.TranDate.Length - 2);
                transaction.TranDate = transaction.TranDate.Substring(0, 15) + " " + amorpm;
            }
            Session["alltransactions"] = alltransactions;
            JsonResult data = Json(new { data = alltransactions }, JsonRequestBehavior.AllowGet);
            return data;
        }

        [HttpPost]
        public FileResult saveaccounttoaccountreport(Custreport model)
        {
            string transtatus = model.CategoryCode;
            Session["transtatus"] = transtatus;

            string branchname = ds.getbranchnameenglish(model.BranchCode);

            if (branchname == "000")
                branchname = "All Branchs";

            Session["Branchname"] = branchname;
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("Account To Account Report For " + branchname + " With Status " + transtatus + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 5 columns
            PdfPTable tableLayout = new PdfPTable(7);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table

            //file will created in this path
            string strAttachment = Server.MapPath("~/Downloads/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF 
            doc.Add(Add_Content_To_Account_To_Account_PDF(tableLayout));

            // Closing the document
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        protected PdfPTable Add_Content_To_Account_To_Account_PDF(PdfPTable tableLayout)
        {

            float[] headers = { 30, 15, 10, 10, 10, 15, 10 };  //Header Widths
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 95;       //Set the PDF File witdh percentage
            tableLayout.HeaderRows = 1;

            DateTime dTime = DateTime.Now;

            //paragraphs
            //paragraphs
            Paragraph Title = new Paragraph("SSB - Internet banking & Rabih mobile",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Title2 = new Paragraph("Account To Account Report For " + Session["Branchname"].ToString() + " With Status " + Session["transtatus"].ToString(),
               new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Chunk c = new Chunk("SSB - Account To Account Report",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));

            Paragraph Total = new Paragraph(c);
            //Adding Cells
            Paragraph empty = new Paragraph("\n\n",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {
                Colspan = 7,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Title2))
            {
                Colspan = 7,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 4,
                PaddingRight = 10,
                Border = 0,
                PaddingBottom = 10,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 3,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 10,
                PaddingTop = 5,

                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_RIGHT
            });


            tableLayout.AddCell(new PdfPCell(new Phrase(empty))
            {
                Colspan = 7,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 15,
                PaddingTop = 15,
                HorizontalAlignment = Element.ALIGN_LEFT
            });


            ////Add header
            AddCellToHeaderRefined(tableLayout, "From Account");
            AddCellToHeaderRefined(tableLayout, "To Account");
            AddCellToHeaderRefined(tableLayout, "Amount");
            AddCellToHeaderRefined(tableLayout, "Recipient Name");
            AddCellToHeaderRefined(tableLayout, "Status");
            AddCellToHeaderRefined(tableLayout, "FT");
            AddCellToHeaderRefined(tableLayout, "Date");

            List<CustomerTransferReportViewModel> creditapireport = new List<CustomerTransferReportViewModel>();
            creditapireport = (List<CustomerTransferReportViewModel>)Session["accounttranfertransactions"];

            foreach (var report in creditapireport)
            {
                AddCellToBodyRefined(tableLayout, report.Customername);
                AddCellToBodyRefined(tableLayout, report.TranToAccount);
                AddCellToBodyRefined(tableLayout, report.TranReqAmount);
                AddCellToBodyRefined(tableLayout, report.CustomerName);
                AddCellToBodyRefined(tableLayout, report.TranStatus);
                AddCellToBodyRefined(tableLayout, report.FT);
                AddCellToBodyRefined(tableLayout, report.TranDate);

            }

            tableLayout.AddCell(new PdfPCell(new Phrase(" "))
            {
                Colspan = 7,
                PaddingLeft = 60,
                Rowspan = 3,
                Border = 1,
                Top = 5,
                PaddingTop = 5,
                //BackgroundColor = new BaseColor(67, 160, 106),
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Total))
            {
                Colspan = 7,
                PaddingLeft = 60,
                Rowspan = 3,
                Border = 1,
                Top = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            return tableLayout;
        }

        protected PdfPTable Add_Content_To_Account_To_Card_PDF(PdfPTable tableLayout)
        {

            float[] headers = { 15, 15, 15, 10, 15, 20, 10 };  //Header Widths
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 95;       //Set the PDF File witdh percentage
            tableLayout.HeaderRows = 1;

            DateTime dTime = DateTime.Now;

            //paragraphs
            //paragraphs
            Paragraph Title = new Paragraph("SSB - Internet banking & Rabih mobile",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Title2 = new Paragraph("Account To Card Report For " + Session["Branchname"].ToString() + " With Status " + Session["transtatus"].ToString(),
               new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Chunk c = new Chunk("SSB - Account To Card Report",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));

            Paragraph Total = new Paragraph(c);
            //Adding Cells
            Paragraph empty = new Paragraph("\n\n",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {
                Colspan = 7,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Title2))
            {
                Colspan = 7,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 4,
                PaddingRight = 10,
                Border = 0,
                PaddingBottom = 10,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 3,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 10,
                PaddingTop = 5,

                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_RIGHT
            });


            tableLayout.AddCell(new PdfPCell(new Phrase(empty))
            {
                Colspan = 7,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 15,
                PaddingTop = 15,
                HorizontalAlignment = Element.ALIGN_LEFT
            });


            ////Add header
            AddCellToHeaderRefined(tableLayout, "From Account");
            AddCellToHeaderRefined(tableLayout, "PAN");
            AddCellToHeaderRefined(tableLayout, "Amount");
            AddCellToHeaderRefined(tableLayout, "Status");
            AddCellToHeaderRefined(tableLayout, "RRN");
            AddCellToHeaderRefined(tableLayout, "FT");
            AddCellToHeaderRefined(tableLayout, "Date");

            List<CustomerTransferReportViewModel> creditapireport = new List<CustomerTransferReportViewModel>();
            creditapireport = (List<CustomerTransferReportViewModel>)Session["creditapitransactions"];

            foreach (var report in creditapireport)
            {
                AddCellToBodyRefined(tableLayout, report.TranFromAccount.ToString());
                AddCellToBodyRefined(tableLayout, report.PAN.ToString());
                AddCellToBodyRefined(tableLayout, report.TranReqAmount.ToString());
                AddCellToBodyRefined(tableLayout, report.ResponseStatus.ToString());
                AddCellToBodyRefined(tableLayout, report.RRN);
                AddCellToBodyRefined(tableLayout, report.FT.ToString());
                AddCellToBodyRefined(tableLayout, report.TranDate.ToString());

            }

            tableLayout.AddCell(new PdfPCell(new Phrase(" "))
            {
                Colspan = 7,
                PaddingLeft = 60,
                Rowspan = 3,
                Border = 1,
                Top = 5,
                PaddingTop = 5,
                //BackgroundColor = new BaseColor(67, 160, 106),
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Total))
            {
                Colspan = 7,
                PaddingLeft = 60,
                Rowspan = 3,
                Border = 1,
                Top = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            return tableLayout;
        }

        [HttpPost]
        public FileResult savecustomersbyadminsreport(CustomerReportModel model)
        {

            string branchname = model.BranchCode;

            if (branchname == "Admin")
                branchname = "All Users";

            Session["Branchname"] = branchname;
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("Registered Customers By " + branchname + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 5 columns
            PdfPTable tableLayout = new PdfPTable(8);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table

            //file will created in this path
            string strAttachment = Server.MapPath("~/Downloads/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF 
            doc.Add(Add_Content_To_Customer_By_Admins_PDF(tableLayout));

            // Closing the document
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        protected PdfPTable Add_Content_To_Customer_By_Admins_PDF(PdfPTable tableLayout)
        {

            float[] headers = { 20, 12, 10, 12, 10, 10, 16, 10 };  //Header Widths
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 95;       //Set the PDF File witdh percentage
            tableLayout.HeaderRows = 1;

            DateTime dTime = DateTime.Now;

            //paragraphs
            //paragraphs
            Paragraph Title = new Paragraph("SSB - Internet banking & Rabih mobile",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Title2 = new Paragraph("Registered Customers By " + Session["Branchname"].ToString(),
               new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Chunk c = new Chunk("SSB - Registered Customers By Users",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));

            Paragraph Total = new Paragraph(c);
            //Adding Cells
            Paragraph empty = new Paragraph("\n\n",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {
                Colspan = 8,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Title2))
            {
                Colspan = 8,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 4,
                PaddingRight = 10,
                Border = 0,
                PaddingBottom = 10,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 4,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 10,
                PaddingTop = 5,

                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_RIGHT
            });


            tableLayout.AddCell(new PdfPCell(new Phrase(empty))
            {
                Colspan = 8,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 15,
                PaddingTop = 15,
                HorizontalAlignment = Element.ALIGN_LEFT
            });


            ////Add header
            AddCellToHeaderRefined(tableLayout, "Customer Name");
            AddCellToHeaderRefined(tableLayout, "Customer Log");
            AddCellToHeaderRefined(tableLayout, "E-Mail");
            AddCellToHeaderRefined(tableLayout, "Mobile");
            AddCellToHeaderRefined(tableLayout, "Address");
            AddCellToHeaderRefined(tableLayout, "Status");
            AddCellToHeaderRefined(tableLayout, "Account Number");
            AddCellToHeaderRefined(tableLayout, "Create By");

            List<CustomerReportModel> customersbyadmins = new List<CustomerReportModel>();
            customersbyadmins = (List<CustomerReportModel>)Session["customersbyadmin"];

            foreach (var customer in customersbyadmins)
            {

                AddCellToBodyRefined(tableLayout, customer.CustomerName.ToString());
                AddCellToBodyRefined(tableLayout, customer.CustomerLog.ToString());
                AddCellToBodyRefined(tableLayout, customer.Email.ToString());
                AddCellToBodyRefined(tableLayout, customer.mobile.ToString());
                AddCellToBodyRefined(tableLayout, customer.address.ToString());
                AddCellToBodyRefined(tableLayout, customer.CustStatus.ToString());
                AddCellToBodyRefined(tableLayout, customer.AccountNumber.ToString());
                AddCellToBodyRefined(tableLayout, customer.created_by.ToString());

            }

            tableLayout.AddCell(new PdfPCell(new Phrase(" "))
            {
                Colspan = 8,
                PaddingLeft = 60,
                Rowspan = 3,
                Border = 1,
                Top = 5,
                PaddingTop = 5,
                //BackgroundColor = new BaseColor(67, 160, 106),
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Total))
            {
                Colspan = 8,
                PaddingLeft = 60,
                Rowspan = 3,
                Border = 1,
                Top = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            return tableLayout;
        }

        [HttpPost]
        public FileResult saveaccumulativereport(Custreport model)
        {

            string branchname = ds.getbranchnameenglish(model.BranchCode);

            if (branchname == "Admin")
                branchname = "All Branchs";

            Session["Branchname"] = branchname;
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("Accumulative Report For " + branchname + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 5 columns
            PdfPTable tableLayout = new PdfPTable(3);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table

            //file will created in this path
            string strAttachment = Server.MapPath("~/Downloads/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF 
            doc.Add(Add_Content_To_Accumulative_PDF(tableLayout));

            // Closing the document
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        protected PdfPTable Add_Content_To_Accumulative_PDF(PdfPTable tableLayout)
        {

            float[] headers = { 40, 30, 30 };  //Header Widths
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 95;       //Set the PDF File witdh percentage
            tableLayout.HeaderRows = 1;

            DateTime dTime = DateTime.Now;

            //paragraphs
            //paragraphs
            Paragraph Title = new Paragraph("SSB - Internet banking & Rabih mobile",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Title2 = new Paragraph("Customer Report For " + Session["Branchname"].ToString(),
               new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Chunk c = new Chunk("SSB - Accumulative Report",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));

            Paragraph Total = new Paragraph(c);
            //Adding Cells
            Paragraph empty = new Paragraph("\n\n",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {
                Colspan = 5,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Title2))
            {
                Colspan = 5,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 2,
                PaddingRight = 10,
                Border = 0,
                PaddingBottom = 10,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 2,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 10,
                PaddingTop = 5,

                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_RIGHT
            });


            tableLayout.AddCell(new PdfPCell(new Phrase(empty))
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 15,
                PaddingTop = 15,
                HorizontalAlignment = Element.ALIGN_LEFT
            });


            ////Add header
            AddCellToHeaderRefined(tableLayout, "Service");
            AddCellToHeaderRefined(tableLayout, "Transactions Count");
            AddCellToHeaderRefined(tableLayout, "Accumulitive Amount");
            //AddCellToHeader(tableLayout, "Role");
            //AddCellToHeader(tableLayout, "Status");


            List<CustomerTransferReportViewModel> accumulativereport = new List<CustomerTransferReportViewModel>();
            accumulativereport = (List<CustomerTransferReportViewModel>)Session["accumulativereport"];

            foreach (var report in accumulativereport)
            {

                AddCellToBodyRefined(tableLayout, report.TranResult.ToString());
                AddCellToBodyRefined(tableLayout, report.CurrencyCode);
                AddCellToBodyRefined(tableLayout, report.TranReqAmount);
                //AddCellToBody(tableLayout, user.rolename);
                //AddCellToBody(tableLayout, user.user_status);

            }

            tableLayout.AddCell(new PdfPCell(new Phrase(" "))
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 3,
                Border = 1,
                Top = 5,
                PaddingTop = 5,
                //BackgroundColor = new BaseColor(67, 160, 106),
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Total))
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 3,
                Border = 1,
                Top = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            return tableLayout;
        }

        [HttpPost]
        public FileResult savetransactionperbranchreport(Custreport model)
        {
            string service = model.BranchCode;
            if (service == "All")
                service = "All Services";

            Session["service"] = service;
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("Transaction Per Branch Report For " + model.BranchCode + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 5 columns
            PdfPTable tableLayout = new PdfPTable(3);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table

            //file will created in this path
            string strAttachment = Server.MapPath("~/Downloads/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF 
            doc.Add(Add_Content_To_Transaction_Per_Branch_PDF(tableLayout));

            // Closing the document
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        protected PdfPTable Add_Content_To_Transaction_Per_Branch_PDF(PdfPTable tableLayout)
        {

            float[] headers = { 40, 30, 30 };  //Header Widths
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 95;       //Set the PDF File witdh percentage
            tableLayout.HeaderRows = 1;

            DateTime dTime = DateTime.Now;

            //paragraphs
            //paragraphs
            Paragraph Title = new Paragraph("SSB - Internet banking & Rabih mobile",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Title2 = new Paragraph("Transaction Per Branch Report For " + Session["service"].ToString(),
               new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Chunk c = new Chunk("SSB - Transaction Per Branch Report",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));

            Paragraph Total = new Paragraph(c);
            //Adding Cells
            Paragraph empty = new Paragraph("\n\n",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {
                Colspan = 5,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Title2))
            {
                Colspan = 5,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 2,
                PaddingRight = 10,
                Border = 0,
                PaddingBottom = 10,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 2,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 10,
                PaddingTop = 5,

                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_RIGHT
            });


            tableLayout.AddCell(new PdfPCell(new Phrase(empty))
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 15,
                PaddingTop = 15,
                HorizontalAlignment = Element.ALIGN_LEFT
            });


            ////Add header
            AddCellToHeaderRefined(tableLayout, "Branch Name");
            AddCellToHeaderRefined(tableLayout, "Transactions Count");
            AddCellToHeaderRefined(tableLayout, "Accumulative Amount");
            //AddCellToHeader(tableLayout, "Role");
            //AddCellToHeader(tableLayout, "Status");


            List<CustomerTransferReportViewModel> accumulativereport = new List<CustomerTransferReportViewModel>();
            accumulativereport = (List<CustomerTransferReportViewModel>)Session["transactionperbranch"];

            foreach (var report in accumulativereport)
            {

                AddCellToBodyRefined(tableLayout, report.TranResult.ToString());
                AddCellToBodyRefined(tableLayout, report.CurrencyCode);
                AddCellToBodyRefined(tableLayout, report.TranReqAmount);
                //AddCellToBody(tableLayout, user.rolename);
                //AddCellToBody(tableLayout, user.user_status);

            }

            tableLayout.AddCell(new PdfPCell(new Phrase(" "))
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 3,
                Border = 1,
                Top = 5,
                PaddingTop = 5,
                //BackgroundColor = new BaseColor(67, 160, 106),
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Total))
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 3,
                Border = 1,
                Top = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            return tableLayout;
        }

        // Method to add single cell to the Header
        private static void AddCellToHeaderRefined(PdfPTable tableLayout, string cellText)
        {

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE))) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(67, 184, 120) });
        }

        // Method to add single cell to the body
        private static void AddCellToBodyRefined(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.BLACK))) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255) });
        }

        //customer registration report pdf
        public FileResult CreatePdf()
        {
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("UsersRegistrationReport" + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 5 columns
            PdfPTable tableLayout = new PdfPTable(7);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table

            //file will created in this path
            string strAttachment = Server.MapPath("~/Downloads/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF 
            doc.Add(Add_Content_To_PDF2(tableLayout));

            // Closing the document
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        protected PdfPTable Add_Content_To_PDF2(PdfPTable tableLayout)
        {

            float[] headers = { 10, 15, 17, 25, 13, 10, 10 };  //Header Widths
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 95;       //Set the PDF File witdh percentage
            tableLayout.HeaderRows = 1;
            //Add Title to the PDF file at the top

            //List<userlist> userlist = ds.GetAllusers();
            string adminbranch = Session["user_branch"].ToString();
            List<Custreport> customers = ds.getbranchcustomers(adminbranch);

            tableLayout.AddCell(new PdfPCell(new Phrase("Customers Registration Report", new Font(Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0)))) { Colspan = 12, Border = 0, PaddingBottom = 5, HorizontalAlignment = Element.ALIGN_CENTER });


            ////Add header
            AddCellToHeader2(tableLayout, "CUstomerID");
            AddCellToHeader2(tableLayout, "CustomerFullName");
            AddCellToHeader2(tableLayout, "CustomerUsername");
            AddCellToHeader2(tableLayout, "AccountNumber");
            AddCellToHeader2(tableLayout, "PhoneNumber");
            AddCellToHeader2(tableLayout, "Address");
            AddCellToHeader2(tableLayout, "CreatedBy");

            ////Add body




            foreach (var customer in customers)
            {
                AddCellToBody2(tableLayout, customer.CustomerID);
                AddCellToBody2(tableLayout, customer.customerfullname);
                AddCellToBody2(tableLayout, customer.CustomerName);
                AddCellToBody2(tableLayout, customer.AccountNumber);
                AddCellToBody2(tableLayout, customer.phonenumber);
                AddCellToBody2(tableLayout, customer.address);
                AddCellToBody2(tableLayout, customer.createdby);
            }

            return tableLayout;
        }

        // Method to add single cell to the Header
        private static void AddCellToHeader2(PdfPTable tableLayout, string cellText)
        {

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE))) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(36, 41, 94) });
        }

        // Method to add single cell to the body
        private static void AddCellToBody2(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.BLACK))) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255) });
        }

        // end of pdf preperation


        public FileResult SavePDF()
        {

            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created   
            string strPDFFileName = string.Format("CustomerReport For " + Session["Branchname"].ToString() + dTime.ToString("ddMMMyyyyHHmmss") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 6 columns  
            /*PdfPTable tableLayout = new PdfPTable(5);*/
            PdfPTable tableLayout = new PdfPTable(3);
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

        //Add Content
        protected PdfPTable Add_Content_To_PDF(PdfPTable tableLayout)
        {



            PdfPTableHeader tableHeader = new PdfPTableHeader();

            string fontpath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, true);
            float[] headers = { 50, 20, 50 }; //Header Widths  
            tableLayout.SetWidths(headers); //Set the pdf headers  
            tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
            tableLayout.HeaderRows = 1;

            //Add Title to the PDF file at the top  

            //List < Employee > UserLog = _context.UserLog.ToList < Employee > ();  
            List<Custreport> UserLog = new List<Custreport>();
            UserLog = (List<Custreport>)Session["BranchUsers"];

            DateTime dTime = DateTime.Now;
            //string UserName = Session["name"].ToString();
            //string AccNo = Session["AccNo"].ToString();
            //string fromDate = Session["fromDate"].ToString();
            //string toDate = Session["toDate"].ToString();
            //string AccountNumber = AccNo.Substring(13);
            //string AccountType = data.getaccounttype(AccNo.ToString().Substring(5, 5));
            //string BranchName = data.getbranchnameenglish(AccNo.ToString().Substring(2, 3));
            //string currency = data.GetCurrencyName(AccNo.Substring(10, 3));
            //String oDate = DateTime.ParseExact(fromDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dd-MMM-yyyy");

            //paragraphs
            Paragraph Title = new Paragraph("Rabih Control Panel  ",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Title2 = new Paragraph("Customer Report For " + Session["Branchname"].ToString(),
               new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));

            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));

            /*Paragraph From = new Paragraph("Statement of Account From  : " + DateTime.ParseExact(fromDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dd-MMM-yyyy") + " To " + DateTime.ParseExact(toDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));*/
            Chunk c = new Chunk("Total of Customers Registered : " + Session["totalcustomer"].ToString(),
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));

            Paragraph Total = new Paragraph(c);

            /*Paragraph AccountNo = new Paragraph("Account No : " + AccountNumber,
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));*/

            /* Paragraph Currency = new Paragraph("Currency : " + currency,
                 new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));*/

            /*Paragraph customerName = new Paragraph("User Name:" + UserName,
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));*/

            //Adding Cells
            Paragraph empty = new Paragraph("\n\n",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {
                Colspan = 5,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(0, 192, 239),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Title2))
            {
                Colspan = 5,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(0, 192, 239),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 1,
                PaddingRight = 10,
                Border = 0,
                PaddingBottom = 10,
                BackgroundColor = new BaseColor(0, 192, 239),
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 2,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 10,
                PaddingTop = 5,

                BackgroundColor = new BaseColor(0, 192, 239),
                HorizontalAlignment = Element.ALIGN_RIGHT
            });


            tableLayout.AddCell(new PdfPCell(new Phrase(empty))
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 15,
                PaddingTop = 15,
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            /* tableLayout.AddCell(new PdfPCell(new Phrase(accountType))
             {
                 Colspan = 4,
                 PaddingLeft = 10,
                 Rowspan = 1,
                 Border = 0,
                 PaddingBottom = 5,
                 HorizontalAlignment = Element.ALIGN_LEFT
             });

             tableLayout.AddCell(new PdfPCell(new Phrase(AccountNo))
             {
                 Colspan = 1,
                 PaddingRight = 10,
                 Rowspan = 1,
                 Border = 0,
                 PaddingBottom = 5,
                 HorizontalAlignment = Element.ALIGN_RIGHT
             });

             tableLayout.AddCell(new PdfPCell(new Phrase(customerName))
             {
                 Colspan = 4,
                 PaddingLeft = 10,
                 Rowspan = 1,
                 Border = 0,
                 PaddingBottom = 5,
                 HorizontalAlignment = Element.ALIGN_LEFT
             });

             tableLayout.AddCell(new PdfPCell(new Phrase(Currency))
             {
                 Colspan = 1,
                 PaddingRight = 10,
                 Rowspan = 1,
                 Border = 0,
                 PaddingBottom = 5,
                 HorizontalAlignment = Element.ALIGN_RIGHT
             });*/

            ////Add header 

            AddCellToHeader(tableLayout, "Name");
            AddCellToHeader(tableLayout, "Status");
            AddCellToHeader(tableLayout, "Account");
            //AddCellToHeader(tableLayout, "Statement ID");  

            ////Add body  

            foreach (var emp in UserLog)
            {

                AddCellToBody(tableLayout, emp.CustomerName.ToString());
                AddCellToBody(tableLayout, emp.CustStatus.ToString());
                AddCellToBody(tableLayout, emp.AccountNumber.ToString());

                //AddCellToBody(tableLayout, emp.StateID.ToString());  

            }
            tableLayout.AddCell(new PdfPCell(new Phrase(empty))
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 3,
                Border = 1,
                PaddingTop = 20,

                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_LEFT

            });
            tableLayout.AddCell(new PdfPCell(new Phrase(Total))
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 3,
                Border = 1,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(0, 192, 239),
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            return tableLayout;
        }

        //Header Cells:
        // Method to add single cell to the Header  
        private static void AddCellToHeader(PdfPTable tableLayout, string cellText)
        {

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 192, 239))))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                Border = Rectangle.BOX,
                BorderWidth = 1,
                BorderWidthLeft = 0,
                BorderWidthRight = 0,
                BorderWidthTop = 0,

                BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
            });
        }

        // Method to add single cell to the body  
        private static void AddCellToBody(PdfPTable tableLayout, string cellText)
        {
            string fontpath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, true);
            const string regex_match_arabic_hebrew = @"[\u0600-\u06FF\u0590-\u05FF]+";
            if (Regex.IsMatch(cellText, regex_match_arabic_hebrew, RegexOptions.IgnoreCase))
            {
                tableLayout.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                tableLayout.AddCell(new PdfPCell(new Phrase(cellText,
                    new Font(basefont, 8, 1, iTextSharp.text.BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5,
                    Border = Rectangle.BOX,
                    BorderWidth = 1,
                    BorderWidthLeft = 0,
                    BorderWidthRight = 0,
                    BorderWidthTop = 0,
                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
                });
            }
            else
            {
                tableLayout.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                tableLayout.AddCell(new PdfPCell(new Phrase(cellText,
                    new Font(basefont, 8, 1, iTextSharp.text.BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    Padding = 5,
                    Border = Rectangle.BOX,
                    BorderWidth = 1,
                    BorderWidthLeft = 0,
                    BorderWidthRight = 0,
                    BorderWidthTop = 0,
                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
                });
            }
        }


        // Method to add single cell to the body  
        private static void AddCellToBody3(PdfPTable tableLayout, string cellText)
        {

            string fontpath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, true);

            const string regex_match_arabic_hebrew = @"[\u0600-\u06FF\u0590-\u05FF]+";
            if (Regex.IsMatch(cellText, regex_match_arabic_hebrew, RegexOptions.IgnoreCase))
            {
                tableLayout.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                tableLayout.AddCell(new PdfPCell(new Phrase(cellText,
                    new Font(basefont, 8, 1, iTextSharp.text.BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5,
                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
                });
            }
            else
            {
                tableLayout.RunDirection = PdfWriter.RUN_DIRECTION_LTR;

                tableLayout.AddCell(new PdfPCell(new Phrase(cellText,
                    new Font(basefont, 8, 1, iTextSharp.text.BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    Padding = 5,
                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
                });
            }
        }

        //Header Cells:
        // Method to add single cell to the Header  
        private static void AddCellToHeader3(PdfPTable tableLayout, string cellText)
        {

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(67, 160, 106))))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                // BackgroundColor = new iTextSharp.text.BaseColor(128, 128, 128)
                BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
            });
        }

        //Add Content
        protected PdfPTable Add_Content_SettlementTempDetails_To_PDF(PdfPTable tableLayout)
        {



            PdfPTableHeader tableHeader = new PdfPTableHeader();
            string fontpath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, true);
            float[] headers = { 20, 30, 30, 30, 30, 20, 20, 20, 20 }; //Header Widths  
                                                                      // float[] headers = { 20, 20, 20, 20, 20, 20 }; //Header Widths  
            tableLayout.SetWidths(headers); //Set the pdf headers  
            tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
            tableLayout.HeaderRows = 4;
            //Add Title to the PDF file at the top  

            //List < Employee > UserLog = _context.UserLog.ToList < Employee > ();  
            List<CustSettreport> UserLog = new List<CustSettreport>();
            UserLog = (List<CustSettreport>)Session["SettlementTempsLog"];

            DateTime dTime = DateTime.Now;


            //paragraphs  (67, 160, 106)
            Paragraph Title = new Paragraph("SIB - Rabih - Settlement Temp Details Log",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            //new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));

            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));

            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            Paragraph Empty = new Paragraph("",
               new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));



            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {

                Colspan = 9,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 5,
                PaddingRight = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 15,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 4,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 10,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_RIGHT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Empty)) //Empty
            {
                Colspan = 9,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_LEFT
            });


            ////Add header 



            AddCellToHeader3(tableLayout, "Settlement Biller ID");
            AddCellToHeader3(tableLayout, "Settlement Transaction Date");
            AddCellToHeader3(tableLayout, "Settlement Transaction ID");
            AddCellToHeader3(tableLayout, "Settlement Bank Refrence");
            AddCellToHeader3(tableLayout, "Settlement Trace Number");
            AddCellToHeader3(tableLayout, "Settlement Transaction Refrence");
            AddCellToHeader3(tableLayout, "Settlement Amount");
            AddCellToHeader3(tableLayout, "Settlement Fees");
            AddCellToHeader3(tableLayout, "Settlement Biller Response");


            ////Add body  

            foreach (var emp in UserLog)
            {

                AddCellToBody3(tableLayout, emp.sbt_sett_biller_id.ToString());
                AddCellToBody3(tableLayout, emp.sbt_sett_transaction_date.ToString());
                AddCellToBody3(tableLayout, emp.sbt_sett_transaction_id.ToString());
                AddCellToBody3(tableLayout, emp.sbt_sett_bank_ref.ToString());
                AddCellToBody3(tableLayout, emp.sbt_sett_trace_number.ToString());
                AddCellToBody3(tableLayout, emp.sbt_sett_transaction_ref.ToString());
                AddCellToBody3(tableLayout, emp.sbt_sett_amount.ToString());
                AddCellToBody3(tableLayout, emp.sbt_sett_fees.ToString());
                AddCellToBody3(tableLayout, emp.sbt_sett_biller_response.ToString());


            }

            return tableLayout;
        }

        public FileResult SaveSettlementTempDetailsPDF()
        {
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created   
            string strPDFFileName = string.Format("UserLogPdf" + dTime.ToString("ddMMMyyyy") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 6 columns  
            /*PdfPTable tableLayout = new PdfPTable(5);*/
            PdfPTable tableLayout = new PdfPTable(9);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table  

            //file will created in this path  
            string strAttachment = Server.MapPath("~/Downloadss/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF   

            doc.Add(Add_Content_SettlementTempDetails_To_PDF(tableLayout));

            // Closing the document  
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        //Add Content
        protected PdfPTable Add_Content_SettlementDetails_To_PDF(PdfPTable tableLayout)
        {



            PdfPTableHeader tableHeader = new PdfPTableHeader();
            string fontpath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, true);
            float[] headers = { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20 }; //Header Widths  
                                                                              // float[] headers = { 20, 20, 20, 20, 20, 20 }; //Header Widths  
            tableLayout.SetWidths(headers); //Set the pdf headers  
            tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
            tableLayout.HeaderRows = 4;
            //Add Title to the PDF file at the top  

            //List < Employee > UserLog = _context.UserLog.ToList < Employee > ();  
            List<CustSettreport> UserLog = new List<CustSettreport>();
            UserLog = (List<CustSettreport>)Session["SettlementDetailsLog"];

            DateTime dTime = DateTime.Now;


            //paragraphs  (67, 160, 106)
            Paragraph Title = new Paragraph("SIB - Rabih - Settlement Details Log",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            //new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));

            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));

            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            Paragraph Empty = new Paragraph("",
               new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));



            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {

                Colspan = 11,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 5,
                PaddingRight = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 15,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 6,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 10,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_RIGHT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Empty)) //Empty
            {
                Colspan = 11,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_LEFT
            });


            ////Add header 

            AddCellToHeader3(tableLayout, "Advice Number");
            AddCellToHeader3(tableLayout, "Create Date");
            AddCellToHeader3(tableLayout, "Settlement File Name");
            AddCellToHeader3(tableLayout, "Biller Name");
            AddCellToHeader3(tableLayout, "Transaction Type");
            AddCellToHeader3(tableLayout, "Amount");
            AddCellToHeader3(tableLayout, "From Account");
            AddCellToHeader3(tableLayout, "To Account");
            AddCellToHeader3(tableLayout, "CB Date");
            AddCellToHeader3(tableLayout, "CB FT");
            AddCellToHeader3(tableLayout, "Status");

            ////Add body  

            foreach (var emp in UserLog)
            {

                AddCellToBody3(tableLayout, emp.sbt_advice_no.ToString());
                AddCellToBody3(tableLayout, emp.sbt_create_date.ToString());
                AddCellToBody3(tableLayout, emp.sbt_sett_file.ToString());
                AddCellToBody3(tableLayout, emp.bil_name.ToString());
                AddCellToBody3(tableLayout, emp.sbt_type.ToString());
                AddCellToBody3(tableLayout, emp.sbt_amount.ToString());
                AddCellToBody3(tableLayout, emp.sbt_source_account.ToString());
                AddCellToBody3(tableLayout, emp.sbt_destination_account.ToString());
                AddCellToBody3(tableLayout, emp.sbt_destination_date.ToString());
                AddCellToBody3(tableLayout, emp.sbt_authorization_number.ToString());
                AddCellToBody3(tableLayout, emp.sbt_status.ToString());

            }

            return tableLayout;
        }


        public FileResult SaveSettlementDetailsPDF()
        {
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created   
            string strPDFFileName = string.Format("UserLogPdf" + dTime.ToString("ddMMMyyyy") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 6 columns  
            /*PdfPTable tableLayout = new PdfPTable(5);*/
            PdfPTable tableLayout = new PdfPTable(11);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table  

            //file will created in this path  
            string strAttachment = Server.MapPath("~/Downloadss/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF   

            doc.Add(Add_Content_SettlementDetails_To_PDF(tableLayout));

            // Closing the document  
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }


        public FileResult SaveSettlementFilePDF()
        {
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created   
            string strPDFFileName = string.Format("UserLogPdf" + dTime.ToString("ddMMMyyyy") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 6 columns  
            /*PdfPTable tableLayout = new PdfPTable(5);*/
            PdfPTable tableLayout = new PdfPTable(6);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table  

            //file will created in this path  
            string strAttachment = Server.MapPath("~/Downloadss/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF   

            doc.Add(Add_Content_Settlement_To_PDF(tableLayout));

            // Closing the document  
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        //Add Content
        protected PdfPTable Add_Content_Settlement_To_PDF(PdfPTable tableLayout)
        {



            PdfPTableHeader tableHeader = new PdfPTableHeader();
            string fontpath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, true);
            float[] headers = { 20, 30, 30, 20, 20, 30 }; //Header Widths  
            tableLayout.SetWidths(headers); //Set the pdf headers  
            tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
            tableLayout.HeaderRows = 4;
            //Add Title to the PDF file at the top  

            //List < Employee > UserLog = _context.UserLog.ToList < Employee > ();  
            List<CustSettreport> UserLog = new List<CustSettreport>();
            UserLog = (List<CustSettreport>)Session["SettlementLog"];

            DateTime dTime = DateTime.Now;


            //paragraphs  (67, 160, 106)
            Paragraph Title = new Paragraph("SIB - Rabih - Settlement File Log",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            //new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));

            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));

            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            Paragraph Empty = new Paragraph("",
               new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));



            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {

                Colspan = 5,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 2,
                PaddingRight = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 15,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 5,
                PaddingLeft = 8,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 10,
                // BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Empty)) //Empty
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_LEFT
            });



            ////Add header 

            AddCellToHeader3(tableLayout, "ID");
            AddCellToHeader3(tableLayout, "Create Date");
            AddCellToHeader3(tableLayout, "Status");
            AddCellToHeader3(tableLayout, "Number OF Record");
            AddCellToHeader3(tableLayout, "Number OF Settlement Record");
            AddCellToHeader3(tableLayout, "Settlement File");

            ////Add body  

            foreach (var emp in UserLog)
            {

                AddCellToBody3(tableLayout, emp.sbf_sett_id.ToString());
                AddCellToBody3(tableLayout, emp.sbf_create_date.ToString());
                AddCellToBody3(tableLayout, emp.sbf_sattus.ToString());
                AddCellToBody3(tableLayout, emp.sbf_no_rec.ToString());
                AddCellToBody3(tableLayout, emp.sbf_no_sett_rec.ToString());
                AddCellToBody3(tableLayout, emp.Settlement_File.ToString());

            }

            return tableLayout;
        }

        public ActionResult SDECReport()
        {



            List<req_res_model> req_res_data = new List<req_res_model>();
            req_res_model model = new req_res_model();
            dynamic requestdata = null, responsedata = null;

            req_res_data = ds.getreq_res_log();
            Session["billersreport"] = req_res_data;
            //List<SelectListItem> billers = new List<SelectListItem>();

            //billers = ds.billers_statuses();



            //Session["bilers"] = billers;

            return View();

        }



        public ActionResult BillerAuthorization()
        {
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
          

            if (Session["stsresult"] != null)
            {
                ViewBag.SuccessMessage = Session["stsresult"].ToString();
                Session["stsresult"] = null;
            }

            List<PaymentsReportModel> customer = new List<PaymentsReportModel>();
            customer = ds.PendingBillerTran();

            return View(customer);
        }
        





        [HttpGet]
        public ActionResult PaymentsReport()
        {
            PaymentsReportModel model = new PaymentsReportModel();

            List<SelectListItem> transactions_statuses = new List<SelectListItem>();
            List<SelectListItem> billerDescListItems = new List<SelectListItem>();
            List<SelectListItem> billerListItems = new List<SelectListItem>();
            transactions_statuses.Add(new SelectListItem { Text = "All", Value = "0" });
            transactions_statuses.Add(new SelectListItem { Text = "Successful", Value = "S" });
            transactions_statuses.Add(new SelectListItem { Text = "Failed", Value = "F" });

            List<PaymentBillers> billerList = ds.GetBillersData();
            billerListItems.Add(new SelectListItem { Text = "All", Value = "0" });
            for (int i = 0; i < billerList.Count; i++)
            {
                billerListItems.Add(new SelectListItem
                {
                    Text = billerList[i].BillerName,
                    Value = billerList[i].BillerId.ToString()
                });
            }
            List<PaymentBillers> billerDescList = ds.GetBillersDescData();
            billerDescListItems.Add(new SelectListItem { Text = "All", Value = "0" });
            for (int i = 0; i < billerDescList.Count; i++)
            {
                billerDescListItems.Add(new SelectListItem
                {
                    Text = billerDescList[i].Biller_Dec_Name,
                    Value = billerDescList[i].Biller_Dec_Id.ToString()
                });
            }
            model.Billers = billerListItems;
            model.SubBillers = billerDescListItems;
            model.transactions_statuses = transactions_statuses;
            Session["billerListItems"] = billerListItems;
            Session["billerDescListItems"] = billerDescListItems;
            return View(model);
        }

        [HttpPost]
        public JsonResult GetStates(string id)
        {
            if (id == null)
            {
                id = "0";
            }

            List<SelectListItem> billerDescListItems = new List<SelectListItem>();

            int CountriesID = Convert.ToInt32(id);
            List<PaymentBillers> billerDescList = ds.GetBillersDescData();
            List<PaymentBillers> billerList = ds.GetBillersData();
            billerDescListItems.Add(new SelectListItem { Text = "All", Value = "0" });
            var result = (from subBiller in billerDescList
                          where subBiller.BillerId.ToString() == id
                     select new
                     {
                         subBiller.Biller_Dec_Id
                     ,
                         subBiller.Biller_Dec_Name
                     }).ToList();
            return Json(new SelectList(result, "Biller_Dec_Id", "Biller_Dec_Name"));
        }
        public ActionResult PaymentsReportDetails(string service_code, string tran_id)
        {

            if (Session["stsresult"] != null)
            {
                ViewBag.SuccessMessage = Session["stsresult"].ToString();
                Session["stsresult"] = null;
            }
            string responseStatus = "";
            string responseMessage = "";
            //string bilresponseMessage = "";
            string bilresponseStatus = "";
            // PaymentsReportModel model = (PaymentsReportModel)Session["Transactions"];
            List<PaymentsReportModel> billrep = new List<PaymentsReportModel>();
            //model.transactions = model.transactions.Where(s => s.bbl_id == Id).ToList();
            //String response = Connecttocore.GetStatus(service_code , tran_id);
            if (service_code != null || tran_id != null) {
                String response = Connecttocore.GetStatus(service_code, tran_id);
                //string response = " {   \"responseCode\": 559, \"responseMessage\": \"Transaction Status Check Failed\", \"responseStatus\" : \"Failed\"  } ";
                //string response = "{\r\n    \"applicationId\": \"SAUS\",\r\n    \"sourceTransactionId\": \"2301192010029456\",\r\n    \"bpgTransactionId\": null,\r\n    \"responseCode\": 501,\r\n    \"responseMessage\": \"Successful Operation\",\r\n    \"responseStatus\": \"Success\",\r\n    \"origionalSourceTransactionId\": \"2301192012396461\",\r\n    \"origionalTransaction\": {\r\n        \"billerId\": 102,\r\n        \"serviceRequestType\": 2,\r\n        \"transactionPaidAmount\": 1960.0,\r\n        \"serviceCode\": \"201\",\r\n        \"bpgTransactionId\": 401752479,\r\n        \"responseStatus\": \"Successful\",\r\n        \"sourceTransactionId\": 2301192012396461,\r\n        \"billDueAmount\": null,\r\n        \"token\": null,\r\n        \"responseCode\": 500,\r\n        \"transactionTotalAmount\": null,\r\n        \"billTotalAmount\": null,\r\n        \"transactionFee\": null,\r\n        \"sourceTransactionDateTime\": null,\r\n        \"currency\": null,\r\n        \"additionalData\": {\r\n            \"return\": \"true\",\r\n            \"SubTransactionID\": \"124514712\",\r\n            \"Balance\": \"-3\",\r\n            \"Error\": \"0\"\r\n        },\r\n        \"applicationId\": \"SAUS\",\r\n        \"billDueDate\": null,\r\n        \"responseMessage\": \"operation was executed successfully\",\r\n        \"customerBillerRef\": \"0124840881\"\r\n    }\r\n}";

                JObject jobj = new JObject();
                jobj = JObject.Parse(response);
                dynamic result = jobj;
                //result.responseCode.Equals(500)
                if (result.responseCode == "500")
                {
                    JObject jobj2 = new JObject();
                    JSToken jobj3 = new JSToken();
                    JToken message = result.SelectToken("origionalTransaction");
                    //string origintran = result.origionalTransaction;
                    //jobj2 = JObject.Parse(result.origionalTransaction);
                    dynamic result2 = message;

                    bilresponseStatus = result2.responseStatus;
                    //bilresponseMessage = result.responseMessage;

                }
                else
                {
                    bilresponseStatus = "Failed to check status";
                }

                responseStatus = result.responseStatus;
                responseMessage = result.responseMessage;
            }

            List<PaymentsReportModel> transts = ds.getstatusbnkbilloftran(tran_id, service_code);
            //string bal = result.result;
            //string[] separators = { ",", ":" };
            //string value = bal;
            //string[] acc = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            string bnk_response = "";
            string biller_response = "";

            foreach (var item in transts) {

                bnk_response = item.bnk_response;
                 biller_response = item.biller_response;

           
            }

            if(bnk_response.Equals("00001"))
            {
                bnk_response = "Successfull";
            }
            if (!bnk_response.Equals("00001"))
            {
                bnk_response = "Failed";
            }

            if (biller_response.Equals("500"))
            {
                biller_response = "Successfull";
            }
            if (!biller_response.Equals("500"))
            {
                biller_response = "Failed";
            }
            billrep.Add(new PaymentsReportModel
            {
                status = responseStatus,
                message = responseMessage,
                tran_id = tran_id,
                SubBillersId = service_code,
                bil_message = bilresponseStatus,
                bnk_response = bnk_response,
                biller_response = biller_response
            });
            //model.status = responseStatus;
            //model.message = responseMessage;


             Session["billersreport"] = billrep;
            return View(billrep);
        }



        public ActionResult Reverse(string account_no, string biller_id,string billersubid,string channel_id ,string bnkrefrance, string amount,string vocher_id)
        {

            if (Session["stsresult"] != null)
            {
                ViewBag.SuccessMessage = Session["stsresult"].ToString();
                Session["stsresult"] = null;
            }
            string responseCode = "";
            string responseMessage = "";
            string mess = "";
            //string bilresponseMessage = "";
            string bilresponseStatus = "";
            // PaymentsReportModel model = (PaymentsReportModel)Session["Transactions"];
            List<PaymentsReportModel> billrep = new List<PaymentsReportModel>();
            //model.transactions = model.transactions.Where(s => s.bbl_id == Id).ToList();
            //String response = Connecttocore.GetStatus(service_code , tran_id);
            if (billersubid != null )
            {
                String response = Connecttocore.BillerReverse(account_no , biller_id,billersubid,channel_id,bnkrefrance,amount,vocher_id);
                //string response = " {   \"responseCode\": 559, \"responseMessage\": \"Transaction Status Check Failed\", \"responseStatus\" : \"Failed\"  } ";
               // string response = "{\r\n\r\n    \"reponseMessage\": \"Execution Error\",\r\n\r\n    \"responseCode\": \"1003\"\r\n\r\n}";
                JObject jobj = new JObject();
                jobj = JObject.Parse(response);
                dynamic result = jobj;
                if(result.errcode == "0")
                {
                    Session["stsresult"] = "Failed revese operation";
                    return RedirectToAction("BillerAuthorization");
                }
                responseCode = result.responseCode;
                responseMessage = result.reponseMessage;
                if (responseCode == "1")
                {
                    string username = Session["user_name"].ToString();
                    int res = ds.updatefinalsts(bnkrefrance , username);

                    if (!res.Equals(-1))
                    {


                         mess = "Final Status Updated Successfully";
                        //Session["chqmessage"] = mess;

                    }
                    else
                    {
                       mess = "Sorry You Cannot process now";
                       // Session["chqmessage"] = message;

                    }
                    Session["stsresult"] = responseMessage + " "+ mess;
                    return RedirectToAction("BillerAuthorization");
                }
                //responseMessage = result.reponseMessage;
                Session["stsresult"] = responseMessage +  " " + mess;
                return RedirectToAction("BillerAuthorization");
            }



            //model.status = responseStatus;
            //model.message = responseMessage;


            //Session["billersreport"] = billrep;
            //return View();
            Session["stsresult"] = "Failed revese operation";
            return RedirectToAction("BillerAuthorization");
        }

        public ActionResult PaymentsReportChangeStatus( string tran_id , string service_code)
        {
            // PaymentsReportModel model = (PaymentsReportModel)Session["Transactions"];
            List<PaymentsReportModel> billrep = new List<PaymentsReportModel>();
            //model.transactions = model.transactions.Where(s => s.bbl_id == Id).ToList();
            Session["tran_id"] = tran_id;
            if (!string.IsNullOrEmpty(tran_id))
            {
                string username = Session["user_name"].ToString() ;
                int result = ds.updateBillerstatusfoereverse(tran_id ,username);
                if (result != -1)
                {
                    Session["stsresult"] = "The Transction  will changed status to be reverse Successfully";
                   
                    return RedirectToAction("PaymentsReportDetails", new { service_code = service_code, tran_id = tran_id });
                }
                else
                {
                    Session["stsresult"] = "The Transction  will not changed status to be reverse Successfully  or status already Successfull ";

              
                    return RedirectToAction("PaymentsReportDetails", new { service_code = service_code, tran_id = tran_id });
                }
            }

                   // Session["stsresult"] = billrep;
              return RedirectToAction("PaymentsReportDetails", new { service_code = service_code, tran_id = tran_id });
        }


        [HttpPost]
        public ActionResult PaymentsReport(PaymentsReportModel model)
        {
            model.transactions = ds.GetPaymentsReportData(model.fromDate, model.toDate, model.BillerId, model.SubBillersId, model.transactions_statusesId);
            List<SelectListItem> transactions_statuses = new List<SelectListItem>();
            List<SelectListItem> billerDescListItems = new List<SelectListItem>();
            List<SelectListItem> billerListItems = new List<SelectListItem>();
            transactions_statuses.Add(new SelectListItem { Text = "All", Value = "0" });
            transactions_statuses.Add(new SelectListItem { Text = "Successful", Value = "S" });
            transactions_statuses.Add(new SelectListItem { Text = "Failed", Value = "F" });
            billerListItems = (List<SelectListItem>)Session["billerListItems"];
            billerDescListItems = (List<SelectListItem>)Session["billerDescListItems"];
            model.Billers = billerListItems;
            model.SubBillers = billerDescListItems;
            model.transactions_statuses = transactions_statuses;
            Session["Transactions"] = model;
            Session["billersreport"] = model.transactions;
            return View(model);
        }



        public ActionResult ZainReport()
        {



            List<req_res_model> req_res_data = new List<req_res_model>();
            req_res_model model = new req_res_model();
            dynamic requestdata = null, responsedata = null;
            List<SelectListItem> transactions_statuses = new List<SelectListItem>();
            transactions_statuses.Add(new SelectListItem { Text = "All", Value = "All" });
            transactions_statuses.Add(new SelectListItem { Text = "Successful", Value = "Secussfully" });
            transactions_statuses.Add(new SelectListItem { Text = "Failed", Value = "Failed" });


            model.transactions_statuses = transactions_statuses;
            req_res_data = ds.getreq_res_log();
            Session["billersreport"] = req_res_data;
            //List<SelectListItem> billers = new List<SelectListItem>();

            //billers = ds.billers_statuses();



            //Session["bilers"] = billers;

            return View(model);

        }

        public JsonResult FilteredSDECReport(string fromdate, string todate)
        {


            string formatedFromDate = DateTime.Parse(fromdate).ToString();
            string formatedtodate = DateTime.Parse(todate).ToString();
            string[] readyfromdate = formatedFromDate.Split(' ');
            string[] readytodate = formatedtodate.Split(' ');
            List<req_res_model> req_res_data = new List<req_res_model>();


            req_res_data = ds.getfilteredSDEC(fromdate, todate);

            Session["billersreport"] = req_res_data;


            foreach (req_res_model transaction in req_res_data)
            {
                dynamic requestdata = JObject.Parse(transaction.tran_req);
                dynamic responsedata = JObject.Parse(transaction.tran_resp);
                if (requestdata.Account != null)
                {
                    transaction.PayCustCode = requestdata.PayCustomerCode;
                    transaction.PayCustName = requestdata.PayCustomerName;
                    transaction.PayAmount = responsedata.PayAmount;
                    //transaction.Voucher = requestdata.BillerVoucher;
                    transaction.Account = requestdata.Account;
                    //transaction.status = responsedata.OrderStatus;
                    transaction.VoucherRes = responsedata.PaymentVoucherNo;

                    transaction.status = responsedata.OrderStatus;
                    //transaction.Account = responsedata.Account;
                }
                if (transaction.VoucherRes == null || transaction.VoucherRes == "")
                {
                    transaction.VoucherRes = "N/A";

                }

                if (responsedata.PayAmount == null)
                {
                    transaction.PayAmount = "N/A";

                }

                if (transaction.token == null || transaction.token == "")
                {
                    transaction.token = "N/A";

                }


                if (requestdata.Account == null)
                {
                    transaction.Account = "N/A";

                }

                if (requestdata.PayCustomerCode == null)
                {
                    transaction.PayCustCode = "N/A";

                }

                if (requestdata.PayCustomerName == null)
                {
                    transaction.PayCustName = "N/A";

                }

                if (responsedata.OrderStatus == null)
                {
                    transaction.status = "N/A";


                }


            }


            // List<req_res_model> sdecreport = new List<req_res_model>();

            //sdecreport = req_res_data;
            Session["sdecreport"] = req_res_data;
            JsonResult data = Json(new { data = req_res_data }, JsonRequestBehavior.AllowGet);
            data.MaxJsonLength = int.MaxValue;
            return data;
        }

        public JsonResult FilteredZainReport(string fromdate, string todate, string status)
        {



            string formatedFromDate = DateTime.Parse(fromdate).ToString();
            string formatedtodate = DateTime.Parse(todate).ToString();
            string[] readyfromdate = formatedFromDate.Split(' ');
            string[] readytodate = formatedtodate.Split(' ');
            List<req_res_model> req_res_data = new List<req_res_model>();


            req_res_data = ds.getfilteredZain(fromdate, todate, status);

            Session["billersreport"] = req_res_data;


            foreach (req_res_model transaction in req_res_data)
            {
                dynamic requestdata = JObject.Parse(transaction.BBL_BNKREFRENCE);

                //string ft = requestdata.reference;
                //string tranDate = transaction.TRAN_Data;
                //string amount = transaction.bbl_billamount;
                //string billResponse = transaction.BBL_BILLERRESPONSE;
                //string phone = transaction.bbl_billervoucher;
                //string bnkResponse = transaction.bbl_bnkresponse;
                //string traceNo = transaction.bbl_sys_traceno;
                //string userid = transaction.user_id;



                transaction.ft = requestdata.reference;
                transaction.tranDate = transaction.TRAN_Data;
                transaction.amount = transaction.bbl_billamount;
                transaction.billResponse = transaction.BBL_BILLERRESPONSE;
                transaction.phone = transaction.bbl_billervoucher;
                transaction.bnkResponse = transaction.bbl_bnkresponse;
                transaction.traceNo = transaction.bbl_sys_traceno;
                transaction.userid = transaction.user_id;




            }

            Session["billersreport"] = req_res_data;
            // List<req_res_model> sdecreport = new List<req_res_model>();

            //sdecreport = req_res_data;
            //Session["zainreport"] = req_res_data;
            JsonResult data = Json(new { data = req_res_data }, JsonRequestBehavior.AllowGet);
            data.MaxJsonLength = int.MaxValue;
            return data;
        }

        public ActionResult Accounttoaccountreport()
        {
            Custreport model = new Custreport();
            model.Branches = ds.PopulateBranchs();

            List<SelectListItem> transaction_names_list = new List<SelectListItem>();
            transaction_names_list.Add(new SelectListItem { Text = "All", Value = "All" });
            transaction_names_list.Add(new SelectListItem { Text = "AccountToCardTransfer", Value = "AccountToCardTransfer" });
            transaction_names_list.Add(new SelectListItem { Text = "To Bank Customer Transfer", Value = "To Bank Customer Transfer" });
            List<SelectListItem> transactions_statuses = new List<SelectListItem>();
            transactions_statuses.Add(new SelectListItem { Text = "All", Value = "All" });
            transactions_statuses.Add(new SelectListItem { Text = "Successful", Value = "Secussfully" });
            transactions_statuses.Add(new SelectListItem { Text = "Failed", Value = "Failed" });

            model.transactions_names = transaction_names_list;
            model.transactions_statuses = transactions_statuses;



            List<CustomerTransferReportViewModel> accounttranfertransactions = new List<CustomerTransferReportViewModel>();


            Session["accounttranfertransactions"] = accounttranfertransactions;

            return View(model);
        }

        public JsonResult FilterAccountToAccountReport(string branch_code, string status, string fromdate, string todate, int pageNumber)
        {
            string formatedFromDate = DateTime.Parse(fromdate).ToString();
            string formatedtodate = DateTime.Parse(todate).ToString();
            string[] readyfromdate = formatedFromDate.Split(' ');
            string[] readytodate = formatedtodate.Split(' ');



            List<CustomerTransferReportViewModel> accounttranfertransactions = new List<CustomerTransferReportViewModel>();
            List<CustomerTransferReportViewModel> Printaccounttranfertransactions = new List<CustomerTransferReportViewModel>();
            accounttranfertransactions = ds.FilteredAccountToAccountTransactions(branch_code, status, readyfromdate[0], readytodate[0], pageNumber);
            Printaccounttranfertransactions = ds.FilteredAccountToAccountPrintTransactions(branch_code, status, readyfromdate[0], readytodate[0]);
            foreach (CustomerTransferReportViewModel transaction in accounttranfertransactions)
            {
                dynamic requestdata = JObject.Parse(transaction.TranFullReq);
                dynamic responsedata = JObject.Parse(transaction.TranFullResp);
                // transaction.TranReqAmount = requestdata.tranamount;
                transaction.TranFromAccount = requestdata.accountfrom;
                transaction.TranToAccount = requestdata.accountto;
                transaction.alsocustomername = requestdata.FromAccountName;
                transaction.CustomerName = requestdata.recipientName;
                transaction.ResponseStatus = responsedata.status;
                if (responsedata.status != "")
                {
                    if (transaction.ResponseStatus.ToString() != "00")
                    {
                        string word = transaction.ResponseStatus;
                        string[] words = word.Split(':');
                        transaction.FT = words[1];
                    }
                }
                string amorpm = transaction.TranDate.Substring(transaction.TranDate.Length - 2);
                transaction.TranDate = transaction.TranDate.Substring(0, 15) + " " + amorpm;
            }

            foreach (CustomerTransferReportViewModel transaction in Printaccounttranfertransactions)
            {
                dynamic requestdata = JObject.Parse(transaction.TranFullReq);
                dynamic responsedata = JObject.Parse(transaction.TranFullResp);
                //transaction.TranReqAmount = requestdata.tranamount;
                transaction.TranFromAccount = requestdata.accountfrom;
                transaction.TranToAccount = requestdata.accountto;
                transaction.alsocustomername = requestdata.FromAccountName;
                transaction.CustomerName = requestdata.recipientName;
                transaction.ResponseStatus = responsedata.status;
                if (responsedata.status != "")
                {
                    if (transaction.ResponseStatus.ToString() != "00")
                    {
                        string word = transaction.ResponseStatus;
                        string[] words = word.Split(':');
                        transaction.FT = words[1];
                    }
                }
                string amorpm = transaction.TranDate.Substring(transaction.TranDate.Length - 2);
                transaction.TranDate = transaction.TranDate.Substring(0, 15) + " " + amorpm;
            }
            Session["accounttranfertransactions"] = accounttranfertransactions;
            Session["Printaccounttranfertransactions"] = Printaccounttranfertransactions;
            JsonResult data = Json(new { data = accounttranfertransactions }, JsonRequestBehavior.AllowGet);
            data.MaxJsonLength = int.MaxValue;
            return data;
        }

        public JsonResult FilteredCustomersByAdmin(string admin, string fromdate, string todate, int PageNumber)
        {
            List<CustomerReportModel> customers = new List<CustomerReportModel>();
            List<CustomerReportModel> Printcustomers = new List<CustomerReportModel>();
            if (fromdate == "" || todate == "")
            {
                customers = ds.GetCustomersByAdmin("All", "All", "All", PageNumber);
            }
            else
            {
                string formatedFromDate = DateTime.Parse(fromdate).ToString();//.Substring(0,9);
                                                                              // string parsedDate = DateTime.Parse(todate).ToString();
                string formatedtodate = DateTime.Parse(todate).ToString();
                string[] readyfromdate = formatedFromDate.Split(' ');
                string[] readytodate = formatedtodate.Split(' ');//.Substring(0, 9);

                //List<CustomerReportModel> customers = new List<CustomerReportModel>();
                // List<CustomerReportModel> Printcustomers = new List<CustomerReportModel>();
                customers = ds.GetCustomersByAdmin(admin, readyfromdate[0], readytodate[0], PageNumber);
            }

            //customers = ds.GetCustomersByAdmin(admin, formatedFromDate, formatedtodate, PageNumber);
            // Printcustomers = ds.PrintGetCustomersByAdmin(admin, readyfromdate[0], readytodate[0]);
            Session["customersbyadmin"] = customers;
            // Session["Printcustomersbyadmin"] = Printcustomers;
            JsonResult data = Json(new { data = customers }, JsonRequestBehavior.AllowGet);
            return data;
        }

        public JsonResult FilteredDateOverviewReport(string branch_code, string fromdate, string todate)
        {

            string formatedFromDate = DateTime.Parse(fromdate).ToString();
            string formatedtodate = DateTime.Parse(todate).ToString();
            string[] readyfromdate = formatedFromDate.Split(' ');
            string[] readytodate = formatedtodate.Split(' ');
            List<CustomerTransferReportViewModel> accumulativereport = ds.TotalTransactionsAmountsPerBranch(branch_code, readyfromdate[0], readytodate[0]);
            Session["accumulativereport"] = accumulativereport;
            JsonResult data = Json(new { data = accumulativereport }, JsonRequestBehavior.AllowGet);
            return data;

        }

        public JsonResult FilterTransactionsDatePerBranches(string transaction_name, string fromdate, string todate)
        {

            string formatedFromDate = DateTime.Parse(fromdate).ToString();
            string formatedtodate = DateTime.Parse(todate).ToString();
            string[] readyfromdate = formatedFromDate.Split(' ');
            string[] readytodate = formatedtodate.Split(' ');
            List<CustomerTransferReportViewModel> accumulativereport = ds.GetTransactionPerBranch(transaction_name, readyfromdate[0], readytodate[0]);


            Session["transactionperbranch"] = accumulativereport;
            JsonResult data = Json(new { data = accumulativereport }, JsonRequestBehavior.AllowGet);
            return data;
        }

        public JsonResult FilterCreditAPIReport(string branch_code, string status, string fromdate, string todate, int pageNumber)
        {
            string formatedFromDate = DateTime.Parse(fromdate).ToString();
            string formatedtodate = DateTime.Parse(todate).ToString();
            string[] readyfromdate = formatedFromDate.Split(' ');
            string[] readytodate = formatedtodate.Split(' ');


            List<CustomerTransferReportViewModel> creditapitransactions = ds.GetCreditAPITransaction(branch_code, status, readyfromdate[0], readytodate[0], pageNumber);
            foreach (CustomerTransferReportViewModel transaction in creditapitransactions)
            {
                dynamic requestdata = JObject.Parse(transaction.TranFullReq);
                dynamic responsedata = JObject.Parse(transaction.TranFullResp);
                transaction.TranReqAmount = requestdata.tranamount;
                transaction.PAN = requestdata.PAN;
                transaction.TranFromAccount = requestdata.Fromaccount;
                transaction.CustomerName = requestdata.customerName;
                transaction.ResponseStatus = responsedata.responseStatus;
                transaction.RRN = responsedata.RRN;
                // string word = responsedata.status;
                string word = transaction.Tran_FT;

                string[] words = word.Split(':');
                if (word.Equals("Failed ") || word.Equals("0002"))
                {
                    // transaction.FT = responsedata.responseStatus;
                    transaction.FT = word;


                }
                else
                {

                    transaction.FT = words[1];
                }
                //transaction.FT = words[1];
                string amorpm = transaction.TranDate.Substring(transaction.TranDate.Length - 2);
                transaction.TranDate = transaction.TranDate.Substring(0, 15) + " " + amorpm;
                //transaction.TranDate = transaction.TranDate;
            }
            Session["creditapitransactions"] = creditapitransactions;
            JsonResult data = Json(new { data = creditapitransactions }, JsonRequestBehavior.AllowGet);
            data.MaxJsonLength = int.MaxValue;
            return data;
        }


        [HttpPost]
        public FileResult saveSDECreport(CustomerReportModel model)
        {


            string branchname = "";

            if (branchname == "Admin")
            {
                branchname = "All Users";
                Session["Branchname"] = branchname;
            }

            if (model.BranchCode != null)
            {
                branchname = model.BranchCode;

                Session["Branchname"] = branchname;
            }
            else
            {
                branchname = "All";
                Session["Branchname"] = branchname;
            }


            //
            //  string branchname = model.BranchCode;



            Session["Branchname"] = branchname;
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("SDEC Report " + branchname + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 5 columns
            PdfPTable tableLayout = new PdfPTable(10);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table

            //file will created in this path
            string strAttachment = Server.MapPath("~/Downloads/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF 
            doc.Add(Add_Content_ToSDEC_PDF(tableLayout));

            // Closing the document
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        protected PdfPTable Add_Content_ToSDEC_PDF(PdfPTable tableLayout)
        {

            float[] headers = { 20, 10, 10, 12, 10, 10, 16, 10, 12, 10 };  //Header Widths
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 95;       //Set the PDF File witdh percentage
            tableLayout.HeaderRows = 1;

            DateTime dTime = DateTime.Now;

            //paragraphs
            //paragraphs
            Paragraph Title = new Paragraph("SIB - Rabih",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Title2 = new Paragraph("SDEC Report " + Session["Branchname"].ToString(),
               new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Chunk c = new Chunk("NAS ALBAIT MOBILE - RSDEC Report",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));

            Paragraph Total = new Paragraph(c);
            //Adding Cells
            Paragraph empty = new Paragraph("\n\n",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {
                Colspan = 10,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Title2))
            {
                Colspan = 10,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 5,
                PaddingRight = 10,
                Border = 0,
                PaddingBottom = 10,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 5,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 10,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_RIGHT
            });


            tableLayout.AddCell(new PdfPCell(new Phrase(empty))
            {
                Colspan = 10,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 15,
                PaddingTop = 15,
                HorizontalAlignment = Element.ALIGN_LEFT
            });


            ////Add header
            AddCellToHeaderRefined(tableLayout, "Tran Date");
            AddCellToHeaderRefined(tableLayout, "User ID");
            AddCellToHeaderRefined(tableLayout, "Account");
            AddCellToHeaderRefined(tableLayout, "Amount");
            AddCellToHeaderRefined(tableLayout, "Meter No");
            AddCellToHeaderRefined(tableLayout, "Meter Name");
            AddCellToHeaderRefined(tableLayout, "RRN");
            AddCellToHeaderRefined(tableLayout, "Voucher");
            AddCellToHeaderRefined(tableLayout, "Token");
            AddCellToHeaderRefined(tableLayout, "Status");

            List<req_res_model> Printcustomers = new List<req_res_model>();
            //Printcustomers = (List<CustomerReportModel>)Session["customersbyadmin"];
            Printcustomers = (List<req_res_model>)Session["billersreport"];
            // customersbyadmin
            foreach (var customer in Printcustomers)
            {

                dynamic requestdata = JObject.Parse(customer.tran_req);
                dynamic responsedata = JObject.Parse(customer.tran_resp);
                //if (requestdata != "{}"){ 

                if (requestdata.Account != null)
                {
                    customer.PayCustCode = requestdata.PayCustomerCode;
                    customer.PayCustName = requestdata.PayCustomerName;
                    customer.PayAmount = responsedata.PayAmount;
                    //customer.Voucher = requestdata.BillerVoucher;
                    customer.Account = requestdata.Account;
                    //transaction.status = responsedata.OrderStatus;
                    customer.VoucherRes = responsedata.PaymentVoucherNo;
                    customer.status = responsedata.OrderStatus;
                    //transaction.Account = responsedata.Account;
                }
                // if (responsedata.OrderStatus != null || responsedata != null)

                if (responsedata.PaymentVoucherNo == null)
                {
                    customer.VoucherRes = "N/A";

                }

                if (responsedata.PayAmount == null)
                {
                    customer.PayAmount = "N/A";

                }

                if (customer.tran_resp_result == null || customer.tran_resp_result == " ")
                {
                    customer.tran_resp_result = "N/A";

                }

                if (customer.token == null || customer.token == " ")
                {
                    customer.token = "N/A";

                }

                if (requestdata.Account == null)
                {
                    customer.Account = "N/A";

                }




                if (requestdata.PayCustomerCode == null)
                {
                    customer.PayCustCode = "N/A";

                }

                if (requestdata.PayCustomerName == null)
                {
                    customer.PayCustName = "N/A";

                }


                if (responsedata.OrderStatus == null)
                {
                    customer.status = "N/A";


                }






                AddCellToBodyRefined(tableLayout, customer.TRAN_Data.ToString());
                AddCellToBodyRefined(tableLayout, customer.user_id.ToString());
                AddCellToBodyRefined(tableLayout, customer.Account.ToString());
                AddCellToBodyRefined(tableLayout, customer.PayAmount.ToString());
                AddCellToBodyRefined(tableLayout, customer.PayCustCode.ToString());
                AddCellToBodyRefined(tableLayout, customer.PayCustName.ToString());
                AddCellToBodyRefined(tableLayout, customer.tran_resp_result.ToString());
                AddCellToBodyRefined(tableLayout, customer.VoucherRes.ToString());
                AddCellToBodyRefined(tableLayout, customer.token.ToString());
                AddCellToBodyRefined(tableLayout, customer.status.ToString());

                //}
            }

            //tableLayout.AddCell(new PdfPCell(new Phrase(" "))
            //{
            //    Colspan = 10,
            //    PaddingLeft = 60,
            //    Rowspan = 3,
            //    Border = 1,
            //    Top = 5,
            //    PaddingTop = 5,
            //    //BackgroundColor = new BaseColor(67, 160, 106),
            //    PaddingBottom = 5,
            //    HorizontalAlignment = Element.ALIGN_CENTER

            //});

            //tableLayout.AddCell(new PdfPCell(new Phrase(Total))
            //{
            //    Colspan = 10,
            //    PaddingLeft = 60,
            //    Rowspan = 3,
            //    Border = 1,
            //    Top = 5,
            //    PaddingTop = 5,
            //    BackgroundColor = new BaseColor(67, 160, 106),
            //    PaddingBottom = 5,
            //    HorizontalAlignment = Element.ALIGN_CENTER
            //});

            return tableLayout;

        }

        [HttpPost]
        public FileResult saveZainreport(CustomerReportModel model)
        {


            string branchname = "";

            if (branchname == "Admin")
            {
                branchname = "All Users";
                Session["Branchname"] = branchname;
            }

            if (model.BranchCode != null)
            {
                branchname = model.BranchCode;

                Session["Branchname"] = branchname;
            }
            else
            {
                branchname = "All";
                Session["Branchname"] = branchname;
            }


            //
            //  string branchname = model.BranchCode;



            Session["Branchname"] = branchname;
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("Zain Report " + branchname + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 5 columns
            PdfPTable tableLayout = new PdfPTable(6);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table

            //file will created in this path
            string strAttachment = Server.MapPath("~/Downloads/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF 
            doc.Add(Add_Content_ToZain_PDF(tableLayout));

            // Closing the document
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        protected PdfPTable Add_Content_ToZain_PDF(PdfPTable tableLayout)
        {

            float[] headers = { 20, 10, 10, 12, 10, 10 };  //Header Widths
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 95;       //Set the PDF File witdh percentage
            tableLayout.HeaderRows = 1;

            DateTime dTime = DateTime.Now;

            //paragraphs
            //paragraphs
            Paragraph Title = new Paragraph("SIB - Rabih",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Title2 = new Paragraph("Zain Report " + Session["Branchname"].ToString(),
               new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 5, 1, iTextSharp.text.BaseColor.WHITE));
            Chunk c = new Chunk("NAS ALBAIT MOBILE - Zain Report",
                new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE));

            Paragraph Total = new Paragraph(c);
            //Adding Cells
            Paragraph empty = new Paragraph("\n\n",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));
            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {
                Colspan = 6,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Title2))
            {
                Colspan = 6,
                PaddingLeft = 30,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 3,
                PaddingRight = 10,
                Border = 0,
                PaddingBottom = 10,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 3,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 10,
                PaddingTop = 5,
                BackgroundColor = new BaseColor(67, 160, 106),
                HorizontalAlignment = Element.ALIGN_RIGHT
            });


            tableLayout.AddCell(new PdfPCell(new Phrase(empty))
            {
                Colspan = 6,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 15,
                PaddingTop = 15,
                HorizontalAlignment = Element.ALIGN_LEFT
            });


            ////Add header
            AddCellToHeaderRefined(tableLayout, "Tran Date");
            AddCellToHeaderRefined(tableLayout, "User ID");
            AddCellToHeaderRefined(tableLayout, "Phone Number");
            AddCellToHeaderRefined(tableLayout, "Amount");
            AddCellToHeaderRefined(tableLayout, "Trace Number");
            AddCellToHeaderRefined(tableLayout, "Status");

            List<req_res_model> Printcustomers = new List<req_res_model>();
            //Printcustomers = (List<CustomerReportModel>)Session["customersbyadmin"];
            Printcustomers = (List<req_res_model>)Session["billersreport"];
            // customersbyadmin



            foreach (var customer in Printcustomers)
            {





                AddCellToBodyRefined(tableLayout, customer.TRAN_Data.ToString());
                AddCellToBodyRefined(tableLayout, customer.user_id.ToString());
                AddCellToBodyRefined(tableLayout, customer.bbl_billervoucher.ToString());
                AddCellToBodyRefined(tableLayout, customer.bbl_billamount.ToString());
                AddCellToBodyRefined(tableLayout, customer.bbl_sys_traceno.ToString());
                AddCellToBodyRefined(tableLayout, customer.bbl_bnkresponse.ToString());


                //}
            }

            //tableLayout.AddCell(new PdfPCell(new Phrase(" "))
            //{
            //    Colspan = 10,
            //    PaddingLeft = 60,
            //    Rowspan = 3,
            //    Border = 1,
            //    Top = 5,
            //    PaddingTop = 5,
            //    //BackgroundColor = new BaseColor(67, 160, 106),
            //    PaddingBottom = 5,
            //    HorizontalAlignment = Element.ALIGN_CENTER

            //});

            //tableLayout.AddCell(new PdfPCell(new Phrase(Total))
            //{
            //    Colspan = 10,
            //    PaddingLeft = 60,
            //    Rowspan = 3,
            //    Border = 1,
            //    Top = 5,
            //    PaddingTop = 5,
            //    BackgroundColor = new BaseColor(67, 160, 106),
            //    PaddingBottom = 5,
            //    HorizontalAlignment = Element.ALIGN_CENTER
            //});

            return tableLayout;

        }
    }
}