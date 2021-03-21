using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SFBCPanel.Models;
using SFBCPanel.Context;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Text;
using System.IO;
using SIBCPanel.Context;

namespace SFBCPanel.Controllers
{
    public class ChqRequestController : Controller
    {
        DataSource ds = new DataSource();
        //
        // GET: /ChqRequest/
        public ActionResult View()
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
            ChqRequest model = new ChqRequest();
            List<ChqRequest> requests = new List<ChqRequest>();
            requests = ds.Chqrequest(Session["user_branch"].ToString());
          /*  if (!customer.Count.Equals(0))
            {
                foreach (var item in customer)
                {
                    model.request_id = item.request_id;
                    model.accountmap = item.accountmap;
                    model.name = item.name;
                    model.booksize = item.booksize;
                    model.date = item.date;

                }
            }*/
            //select c.request_id,branch_name||'-'||curr_name||'-'||act_name||'-'|| SUBSTR(c.account_no,14),c.requested_size,c.req_date,u.user_name from cheque_reqs c,users u,branchs, currency,act_types where req_status='process' and u.user_id=c.user_id and   SUBSTR(c.account_no,3,3)='004' and branchs.branch_code=SUBSTR(c.account_no,3,3) and act_types.act_type_code=SUBSTR(c.account_no,6,5)and currency.curr_code=SUBSTR(c.account_no,11,3) order by c.request_id

            return View(requests);
        }



         
        public ActionResult Accept(int id)
        {
            String message="",sts = "Accpet";
            int response = ds.updatechqsts(id, sts);

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
         return RedirectToAction("View");

        }
      
        public ActionResult Reject(int id)
        {
            String message="", sts = "Reject";
            int response = ds.updatechqsts(id, sts);
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

            // ModelState.AddModelError("", message );
            return RedirectToAction("View");
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
            PdfPTable tableLayout = new PdfPTable(4);
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

            float[] headers = { 50, 24, 45, 35 };  //Header Widths
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 100;       //Set the PDF File witdh percentage
            tableLayout.HeaderRows = 1;
            //Add Title to the PDF file at the top

            string adminbranch = Session["user_branch"].ToString();
            List<ChqRequest> cheques = ds.ChqrequestReport(adminbranch);


            string branchname = ds.getbranchnameenglish(adminbranch);
            tableLayout.AddCell(new PdfPCell(new Phrase(branchname, new Font(Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0)))) { Colspan = 12, Border = 0, PaddingBottom = 5, HorizontalAlignment = Element.ALIGN_CENTER });
            tableLayout.AddCell(new PdfPCell(new Phrase("Cheques Report", new Font(Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0)))) { Colspan = 12, Border = 0, PaddingBottom = 5, HorizontalAlignment = Element.ALIGN_CENTER });


            ////Add header
            AddCellToHeader(tableLayout, "Customer");
            AddCellToHeader(tableLayout, "Requested Size");
            AddCellToHeader(tableLayout, "Request Date");
            AddCellToHeader(tableLayout, "Request Status");
            ////Add body
            foreach (var cheque in cheques)
            {
                AddCellToBody(tableLayout, cheque.name);
                AddCellToBody(tableLayout, cheque.booksize);
                AddCellToBody(tableLayout, cheque.date);
                AddCellToBody(tableLayout, cheque.status);
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