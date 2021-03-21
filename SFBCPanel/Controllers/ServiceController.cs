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
using System.Text;
using SIBCPanel.Context;

namespace SFBCPanel.Controllers
{
    public class ServiceController : Controller
    {
        DataSource ds = new DataSource();

        //
        // GET: /Service/
        public ActionResult Index()
        {
            if (Session["user_name"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Session["user_branch"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if ((Session["cpanelLogin"] == null) || !Session["cpanelLogin"].ToString().Equals("true"))
            {
                return RedirectToAction("Login", "Login");
            }

            if (Session["Serviceresult"] != null)
            {
                ViewBag.SuccessMessage = Session["Serviceresult"].ToString();
                Session["Serviceresult"] = null;
            }
            List<Servielist> Services = ds.GetAllServices();
            // ViewBag.UserList = dataset.Tables[0];
            return View(Services);
        }

        public ActionResult Add()
        {
           // ServiceInsertModel model = new ServiceInsertModel();
            
            return View( );

        }
        [HttpPost]
        public ActionResult Add(SFBCPanel.Models.ServiceInsertModel insertmodel)
        {
            if (ModelState.IsValid)
            {
                int _records = ds.insertservice(insertmodel);
                if (_records > 0)
                {
                    return RedirectToAction("Index", "Service");
                }
                else
                {
                    ModelState.AddModelError("", "Can Not Insert");
                }

            }
            return View(insertmodel);
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {

            ServiceUpdateModel model;
            model = ds.getServiccedata(id);
            model.statuses = ds.Populatecpanelstatuses();

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(SFBCPanel.Models.ServiceUpdateModel updatemodel, int id)
        {   
            updatemodel.statuses = ds.Populatecpanelstatuses();

            var selectedstatus = updatemodel.statuses.Find(p => p.Value == updatemodel.service_status_code.ToString());

            if (selectedstatus != null)
            {
                selectedstatus.Selected = true;
            }

            if (ModelState.IsValid)
            {
                int _records = ds.UpdateService(updatemodel);
                if (_records > 0)
                {
                    return RedirectToAction("Index", "Service");
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
            int records = ds.deleteservice(id);
            if (records > 0)
            {
                return RedirectToAction("Index", "Service");
            }
            else
            {
                ModelState.AddModelError("", "Can Not Delete");
                return View("Users");
            }
            // return View("Index");
        }

    }
}