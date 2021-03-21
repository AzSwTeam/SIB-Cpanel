using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using SFBCPanel.Context;
using SFBCPanel.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SFBCpanel.Models;
using SIBCPanel.Context;

namespace SFBCPanel.Controllers
{
    public class UsersMangementController : Controller
    {
        DataSource ds = new DataSource();
        //
        // GET: /UsersMangement/
        public ActionResult UserLogReport()
        {
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }


        [HttpPost]
        public ActionResult UserLogReport(UsersMangementViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<UsersMangementViewModel> accas = new List<UsersMangementViewModel>();
                accas = ds.GetUserLog(model.Username, model.SuccessfulLogin);

                Session["UserLog"] = accas;

                return RedirectToAction("ViewReport");
            }
            else
            {
                ModelState.AddModelError("", "Fill the Data Please");
                return View();
            }
        }

        public ActionResult ViewReport()
        {
            List<UsersMangementViewModel> accas = new List<UsersMangementViewModel>();
            accas = (List<UsersMangementViewModel>)Session["UserLog"];

            return View(accas);
        }

        //----------------CustomerLogReport()--------------------------------
        public ActionResult CustomerLogReport()
        {
            List<UsersMangementViewModel> accas = new List<UsersMangementViewModel>();
            accas = ds.GetCustomersLog();

            Session["CustomerLog"] = accas;

            return RedirectToAction("ViewCustomerReport");
            //return View();
        }

        [HttpPost]
        public ActionResult CustomerLogReport(UsersMangementViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<UsersMangementViewModel> accas = new List<UsersMangementViewModel>();
                accas = ds.GetCustomersLog();

                Session["CustomerLog"] = accas;

                return RedirectToAction("ViewCustomerReport");
            }
            else
            {
                ModelState.AddModelError("", "Fill the Data Please");
                return View();
            }

            return View();
        }

        public ActionResult UsersDetails()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UsersDetails(UserDetailsModel idorname)
        {
            if (ModelState.IsValid)
            {
                UserDetailsModel userdetails = new UserDetailsModel();
                userdetails = ds.GetUserDetails(idorname.user_name);

                Session["userdetails"] = userdetails;

                return RedirectToAction("ViewDetailedReport");
            }
            else
            {
                ModelState.AddModelError("", "Fill the Data Please");
                return View();
            }
        }

        public ActionResult ViewDetailedReport()
        {
            UserDetailsModel model = new UserDetailsModel();
            model = (UserDetailsModel)Session["userdetails"];
            return View(model);
        }

        public ActionResult ViewCustomerReport()
        {
            List<UsersMangementViewModel> accas = new List<UsersMangementViewModel>();
            accas = (List<UsersMangementViewModel>)Session["CustomerLog"];

            return View(accas);
        }

        public ActionResult ViewCustomerDetails(string CustomerID)
        {
            Custreport customer = new Custreport();
            customer = ds.GetCustomerReportData(CustomerID);
            customer.linkedAccounts = ds.GetCustomerLinkedAccounts(CustomerID);
            customer.channels = ds.GetCustomerChannels(CustomerID);
            return View(customer);
        }


        //--------------------------------------------------

        public FileResult SavePDF()
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

        //Add Content
        protected PdfPTable Add_Content_To_PDF(PdfPTable tableLayout)
        {



            PdfPTableHeader tableHeader = new PdfPTableHeader();


            string fontpath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, true);
            float[] headers = { 20, 20, 10, 50, 40, }; //Header Widths  
            tableLayout.SetWidths(headers); //Set the pdf headers  
            tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
            tableLayout.HeaderRows = 4;
            //Add Title to the PDF file at the top  

            //List < Employee > UserLog = _context.UserLog.ToList < Employee > ();  
            List<UsersMangementViewModel> UserLog = new List<UsersMangementViewModel>();
            UserLog = (List<UsersMangementViewModel>)Session["UserLog"];

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
            Paragraph Title = new Paragraph("Tadamonbank Internet Banking - User Log",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));

            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));

            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));

            /*Paragraph From = new Paragraph("Statement of Account From  : " + DateTime.ParseExact(fromDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dd-MMM-yyyy") + " To " + DateTime.ParseExact(toDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));*/

            /*Paragraph accountType = new Paragraph("Account Type : " + AccountType,
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));*/

            /*Paragraph AccountNo = new Paragraph("Account No : " + AccountNumber,
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));*/

            /* Paragraph Currency = new Paragraph("Currency : " + currency,
                 new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));*/

            /*Paragraph customerName = new Paragraph("User Name:" + UserName,
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));*/

            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 1,
                PaddingRight = 10,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_RIGHT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 2,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase())
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
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

            AddCellToHeader(tableLayout, "IP Address");
            AddCellToHeader(tableLayout, "Login Time");
            AddCellToHeader(tableLayout, "User Login");
            AddCellToHeader(tableLayout, "user Pass");
            AddCellToHeader(tableLayout, "Login Status");
            //AddCellToHeader(tableLayout, "Statement ID");  

            ////Add body  

            foreach (var emp in UserLog)
            {

                AddCellToBody(tableLayout, emp.IpAddress.ToString());
                AddCellToBody(tableLayout, emp.LoginTime.ToString());
                AddCellToBody(tableLayout, emp.UserLogin.ToString());
                AddCellToBody(tableLayout, emp.UserPass.ToString());
                AddCellToBody(tableLayout, emp.LoginStatus.ToString());
                //AddCellToBody(tableLayout, emp.StateID.ToString());  

            }

            return tableLayout;
        }

        //Header Cells:
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

        //////########################################################################################
        /// Customer Report 
        /// 
        public FileResult SaveCustomerPDF()
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

            doc.Add(Add_Content_To_PDF2(tableLayout));

            // Closing the document  
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        //Add Content
        protected PdfPTable Add_Content_To_PDF2(PdfPTable tableLayout)
        {



            PdfPTableHeader tableHeader = new PdfPTableHeader();


            string fontpath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, true);
            float[] headers = { 20, 20, 10, 30, 30, 20 }; //Header Widths  
            tableLayout.SetWidths(headers); //Set the pdf headers  
            tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
            tableLayout.HeaderRows = 4;
            //Add Title to the PDF file at the top  

            //List < Employee > UserLog = _context.UserLog.ToList < Employee > ();  
            List<UsersMangementViewModel> UserLog = new List<UsersMangementViewModel>();
            UserLog = (List<UsersMangementViewModel>)Session["CustomerLog"];

            DateTime dTime = DateTime.Now;


            //paragraphs
            Paragraph Title = new Paragraph("Tadamonbank Internet Banking - Customer Log",
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));

            Paragraph Date = new Paragraph("Date: " + dTime.ToString("dd-MMM-yyyy"),
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));

            Paragraph Time = new Paragraph("TIME:" + dTime.ToString("HH:mm:ss"),
                new Font(Font.FontFamily.HELVETICA, 8, 1, new BaseColor(0, 0, 0)));



            //Adding Cells
            tableLayout.AddCell(new PdfPCell(new Phrase(Title))
            {
                Colspan = 5,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Date))
            {
                Colspan = 1,
                PaddingRight = 10,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_RIGHT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase(Time))
            {
                Colspan = 2,
                PaddingLeft = 10,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            tableLayout.AddCell(new PdfPCell(new Phrase())
            {
                Colspan = 4,
                PaddingLeft = 60,
                Rowspan = 1,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_LEFT
            });



            ////Add header 

            AddCellToHeader2(tableLayout, "IP Address");
            AddCellToHeader2(tableLayout, "Login Time");
            AddCellToHeader2(tableLayout, "User Login");
            AddCellToHeader2(tableLayout, "user Pass");
            AddCellToHeader2(tableLayout, "Login Status");
            AddCellToHeader2(tableLayout, "User ID");

            ////Add body  

            foreach (var emp in UserLog)
            {

                AddCellToBody2(tableLayout, emp.IpAddress.ToString());
                AddCellToBody2(tableLayout, emp.LoginTime.ToString());
                AddCellToBody2(tableLayout, emp.UserLogin.ToString());
                AddCellToBody2(tableLayout, emp.UserPass.ToString());
                AddCellToBody2(tableLayout, emp.LoginStatus.ToString());
                AddCellToBody2(tableLayout, emp.UserID.ToString());

            }

            return tableLayout;
        }

        //Header Cells:
        // Method to add single cell to the Header  
        private static void AddCellToHeader2(PdfPTable tableLayout, string cellText)
        {

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE)))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                BackgroundColor = new iTextSharp.text.BaseColor(128, 128, 128)
            });
        }


        // Method to add single cell to the body  
        private static void AddCellToBody2(PdfPTable tableLayout, string cellText)
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
    }
}