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
                model.catgories = ds.GetGatgories();
                model.CustomerStatus = ds.PopulateCustStatus(passedmodel.Branch);
                model.Branches = ds.PopulateBranchs(model.BranchCode, passedmodel.Branch);
                model.catgories = ds.GetGatgories();
                //model.catgories.RemoveAt(0);
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
    }
}