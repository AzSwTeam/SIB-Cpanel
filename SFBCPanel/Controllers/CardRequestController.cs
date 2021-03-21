using iTextSharp.text;
using iTextSharp.text.pdf;
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

namespace SFBCPanel.Controllers
{
    public class CardRequestController : Controller
    {
        DataSource ds = new DataSource();

        // GET: CardRequest
        public ActionResult CardRequest()
        {
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["chqmessage"] != null)
            {
                ViewBag.SuccessMessage = Session["chqmessage"].ToString();
                Session["chqmessage"] = null;
            }
            List<AtmCardModel> cards = new List<AtmCardModel>();
            cards = ds.GetCardsRequests(Session["user_branch"].ToString());
            return View(cards);
        }

        public ActionResult Accept(int id)
        {
            String message = "", sts = "Accpet";
            int response = ds.updatecard(id, sts);

            if (!response.Equals(-1))
            {


                message = "Request Accpet Successfully";
                Session["chqmessage"] = message;

            }
            else
            {
                message = "Sorry You Cannot process now, please try later  ";
                Session["chqmessage"] = message;

            }
            return RedirectToAction("CardRequest");

        }

        public ActionResult Reject(int id)
        {
            String message = "", sts = "Reject";
            int response = ds.updatecard(id, sts);
            if (!response.Equals(-1))
            {


                message = "Request Reject Successfully";
                Session["chqmessage"] = message;

            }
            else
            {
                message = "Sorry You Cannot process now, please try later  ";
                Session["chqmessage"] = message;

            }
            return RedirectToAction("CardRequest");
        }

        public FileResult CreatePdf()
        {
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("ChequesReport" + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
            Document doc = new Document();
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 5 columns
            PdfPTable tableLayout = new PdfPTable(5);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table

            //file will created in this path
            string strAttachment = Server.MapPath("~/Downloads/" + strPDFFileName);


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

            float[] headers = { 30, 30, 15, 15,10 };  //Header Widths
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 95;       //Set the PDF File witdh percentage
            tableLayout.HeaderRows = 1;
            //Add Title to the PDF file at the top

            string adminbranch = Session["user_branch"].ToString();
            List<AtmCardModel> cheques = ds.AtmCardsReport(adminbranch);


            string branchname = ds.getbranchnameenglish(adminbranch);
            tableLayout.AddCell(new PdfPCell(new Phrase(branchname, new Font(Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0)))) { Colspan = 12, Border = 0, PaddingBottom = 5, HorizontalAlignment = Element.ALIGN_CENTER });
            tableLayout.AddCell(new PdfPCell(new Phrase("AtmCardsReport", new Font(Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0)))) { Colspan = 12, Border = 0, PaddingBottom = 5, HorizontalAlignment = Element.ALIGN_CENTER });


            ////Add header
            AddCellToHeader(tableLayout, "Customer Name");
            AddCellToHeader(tableLayout, "Name On Card");
            AddCellToHeader(tableLayout, "Request Reason");
            AddCellToHeader(tableLayout, "Request Status");
            AddCellToHeader(tableLayout, "Request Date");
            ////Add body
            foreach (var cheque in cheques)
            {
                AddCellToBody(tableLayout, cheque.name);
                AddCellToBody(tableLayout, cheque.name_on_card);
                AddCellToBody(tableLayout, cheque.request_reason);
                AddCellToBody(tableLayout, cheque.request_status);
                AddCellToBody(tableLayout, cheque.request_date);
            }
            return tableLayout;
        }

        // Method to add single cell to the Header
        private static void AddCellToHeader(PdfPTable tableLayout, string cellText)
        {

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.WHITE))) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(0, 90, 73) });
        }

        // Method to add single cell to the body
        private static void AddCellToBody(PdfPTable tableLayout, string cellText)
        {
            string fontpath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, true);
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(basefont, 8, 1, iTextSharp.text.BaseColor.BLACK))) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255) });
        }
    }
}