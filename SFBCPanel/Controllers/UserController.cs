using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SFBCPanel.Models;
using System.Data;
using SFBCPanel.Context;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Linq;
using System.Text;
using SIBCPanel.Context;

namespace SFBCPanel.Controllers
{
    public class UserController : Controller
    {
        DataSource ds = new DataSource();

        public FileResult CreatePdf()
        {
            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("UserReport" + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
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

            List<userlist> userlist = ds.GetAllusers();



            tableLayout.AddCell(new PdfPCell(new Phrase("Users Report", new Font(Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0)))) { Colspan = 12, Border = 0, PaddingBottom = 5, HorizontalAlignment = Element.ALIGN_CENTER });


            ////Add header
            AddCellToHeader(tableLayout, "UserID");
            AddCellToHeader(tableLayout, "Name");
            AddCellToHeader(tableLayout, "Branch");
            AddCellToHeader(tableLayout, "Role");


            ////Add body




            foreach (var user in userlist)
            {

                AddCellToBody(tableLayout, user.user_id.ToString());
                AddCellToBody(tableLayout, user.name);
                AddCellToBody(tableLayout, user.user_branch);
                AddCellToBody(tableLayout, user.rolename);


            }

            return tableLayout;
        }

        // Method to add single cell to the Header
        private static void AddCellToHeader(PdfPTable tableLayout, string cellText)
        {

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.YELLOW))) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(128, 0, 0) });
        }

        // Method to add single cell to the body
        private static void AddCellToBody(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.BLACK))) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255) });
        }

        public ActionResult Users()
        {
            if ((Session["cpanelLogin"] == null) || !Session["cpanelLogin"].ToString().Equals("true"))
            {
                return RedirectToAction("Login", "Login");
            }

            if (Session["userresult"] != null)
            {
                ViewBag.SuccessMessage = Session["userresult"].ToString();
                Session["userresult"] = null;
            }
            List<userlist> users = ds.GetAllusers();
            // ViewBag.UserList = dataset.Tables[0];
            return View(users);
        }

        //[HttpGet]
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
            userInsertModel model = new userInsertModel();
            String userbranch = Session["user_branch"].ToString();


            model.Branches = ds.PopulateBranchsForAdmins();
            model.Roles = ds.PopulatecpanelProfiles(userbranch);



            return View(model);

        }

        [HttpPost]
        public ActionResult Add(SFBCPanel.Models.userInsertModel insertmodel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (ds.checkadminusernameavailability(insertmodel.user_name))
                    {
                        int _records = ds.insert(insertmodel);
                        if (_records > 0)
                        {
                            TempData["success"] = "User Added successfully";
                            return RedirectToAction("Users", "User");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Can Not Insert");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Username already taken");
                    }
                }
            }
            catch (Exception e)
            {
                string message = "All Fields are required ";
                ModelState.AddModelError("", "Something is missing" + message);
            }
            userInsertModel model = new userInsertModel();
            String userbranch = Session["user_branch"].ToString();


            model.Branches = ds.PopulateBranchsForAdmins();
            model.Roles = ds.PopulatecpanelProfiles();
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {

            userUpdateModel model;
            model = ds.getuserdata(id);
            String userbranch = Session["user_branch"].ToString();


            model.Branches = ds.PopulateBranchsForAdmins();
            model.Roles = ds.PopulatecpanelProfiles();



            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(SFBCPanel.Models.userUpdateModel updatemodel, int id)
        {
            updatemodel.Roles = ds.PopulatecpanelProfiles();
            String userbranch = Session["user_branch"].ToString();


            updatemodel.Branches = ds.PopulateBranchs(userbranch);

            var selectedBranch = updatemodel.Branches.Find(p => p.Value == updatemodel.BranchCode.ToString());
            if (selectedBranch != null)
            {
                selectedBranch.Selected = true;

            }
            var selectedRole = updatemodel.Roles.Find(p => p.Value == updatemodel.roleid.ToString());
            if (selectedRole != null)
            {
                selectedRole.Selected = true;

            }
            if (ModelState.IsValid)
            {

                int _records = ds.Update(updatemodel);
                if (_records > 0)
                {
                    return RedirectToAction("Users", "User");
                }
                else
                {
                    ModelState.AddModelError("", "Can Not Update");
                }
            }
            else
            {
                ModelState.AddModelError("", "All Information Required");
            }
            return View(updatemodel);
        }

        public ActionResult Delete(int id)
        {
            int records = ds.deleteuser(id);
            if (records > 0)
            {
                return RedirectToAction("Users", "User");
            }
            else
            {
                ModelState.AddModelError("", "Can Not Delete");
                return View("Users");
            }
            // return View("Index");
        }

        public ActionResult Reset(int id)
        {
            int records = ds.resetpassworduser(id);
            if (records > 0)
            {
                String message = "User Reset Password  Successfully";
                Session["userresult"] = message;
                return RedirectToAction("Users", "User");
            }
            else
            {
                ModelState.AddModelError("", "Can Not Reset Password");
                return View("Users");
            }
            // return View("Index");
        }
    }


}
