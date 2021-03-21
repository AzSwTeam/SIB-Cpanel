using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SFBCPanel.Context;
using SFBCPanel.Models;
using Microsoft.Ajax.Utilities;
using Org.BouncyCastle.Utilities;
using SIBCPanel.Context;

namespace SFBCPanel.Controllers
{
    public class CustomerTransferReportController : Controller
    {
        DataSource ds = new DataSource();
        //
        // GET: /CustomerTransferReport/
        public ActionResult TransferReport()
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
            CustomerTransferReportViewModel model = new CustomerTransferReportViewModel();
            model.Branches = ds.PopulateBranchs(userbranch);
            model.AccTypes = ds.PopulateAccountTypes();
            model.Currencies = ds.PopulateCurrencies();
            return View(model);
        }

        [HttpPost]
        public ActionResult TransferReport(CustomerTransferReportViewModel model)
        {
            string message = "";

            try
            {
                String userbranch = Session["user_branch"].ToString();


                model.Branches = ds.PopulateBranchs(userbranch);
                model.AccTypes = ds.PopulateAccountTypes();
                model.Currencies = ds.PopulateCurrencies();

                var selectedBranch = model.Branches.Find(p => p.Value == model.BranchCode.ToString());
                var selectedAccType = model.AccTypes.Find(p => p.Value == model.AccountTypecode.ToString());
                var selectedCurrency = model.Currencies.Find(p => p.Value == model.CurrencyCode.ToString());

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

                if (ModelState.IsValid)
                {
                    //String CustFullAccount = "13" + model.BranchCode + model.AccountTypecode + model.CurrencyCode +
                    //                         model.AccountNumber;
                    String CustFullAccount = "18" + model.BranchCode + model.AccountTypecode + model.CurrencyCode + model.AccountNumber;

                    string CustID = ds.getCustIDFromAcc(CustFullAccount);

                    if (CustID.IsNullOrWhiteSpace() || CustID.Equals("0"))
                    {
                        message = "This Customer is Un-Available Please make sure to write the RIGHT Account Number ";
                        ModelState.AddModelError("", message);
                        return View(model);
                    }

                    List<CustomerTransferReportViewModel> accass = new List<CustomerTransferReportViewModel>();
                    accass = ds.GetTransferReport(CustID);
                    if (accass.Count > 0)
                    {
                        Session["TransferReport"] = accass;



                        return RedirectToAction("ViewTransferReport");
                    }
                    else
                    {
                        message = "No Transaction tor this account ";
                        ModelState.AddModelError("", message);
                        return View(model);
                    }
                }
            }
            catch (Exception e)
            {
                // Console.WriteLine(e);
                message = "Contact Your support";
                ModelState.AddModelError("", message);
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult TransferReportporcess(CustomerTransferReportViewModel passedmodel)
        {
            CustomerTransferReportViewModel model = new CustomerTransferReportViewModel();
            if (passedmodel.Branch != null)
            {
                model = new CustomerTransferReportViewModel();
                String userbranch = Session["user_branch"].ToString();
                model = ds.GetUserReportData(passedmodel.Branch);

                model.Branches = ds.PopulateBranchs(model.BranchCode, passedmodel.Branch);
                model.AccTypes = ds.PopulateAccountTypes(passedmodel.Branch);
                model.Currencies = ds.PopulateCurrencies();

                model.catgories = ds.GetGatgories();
                return View("TransferReport", model);
            }
            else
            {
                String userbranch = Session["user_branch"].ToString();
                model = new CustomerTransferReportViewModel();
                model.Branches = ds.PopulateBranchs(userbranch);
                model.AccTypes = ds.PopulateAccountTypes();
                model.Currencies = ds.PopulateCurrencies();
                return View("TransferReport", model);
            }
        }

        public ActionResult ViewTransferReport()
        {
            List<CustomerTransferReportViewModel> accass = new List<CustomerTransferReportViewModel>();

            accass = (List<CustomerTransferReportViewModel>)Session["TransferReport"];


            /* foreach (var item in accass)
             {

                 if (item.TranFullReq.Contains(','))
                 {

                 }
             }
 */

            return View(accass);
        }
    }
}