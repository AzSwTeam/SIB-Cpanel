using cpanel.Models;
using Newtonsoft.Json.Linq;
using SFBCPanel.Context;
using SFBCPanel.Models;
using SIBCPanel.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SFBCPanel.Controllers
{
    public class MonitoringController : Controller
    {
        DataSource ds = new DataSource();
        Connecttocore connect = new Connecttocore();
        // GET: Monitoring
        public ActionResult Monitoring()
        {
            List<CustomerTransferReportViewModel> CurrentTransactions = ds.GetCurrentTransactions();
            foreach (CustomerTransferReportViewModel transaction in CurrentTransactions)
            {
                string[] words = transaction.TranFullReq.Split(',');
                transaction.TranFromAccount = words[0];
                string amorpm = transaction.TranDate.Substring(transaction.TranDate.Length - 2);
                transaction.TranDate = transaction.TranDate.Substring(0, 15) + " " + amorpm;
            }

            Session["currenttransactions"] = CurrentTransactions;

            string response = Connecttocore.GetHeartBeat();
            JObject jobj = new JObject();
            jobj = JObject.Parse(response);
            dynamic result = jobj;
            List<biller> connections = new List<biller>();
            connections.Add(new biller
            {
                name = result.B1.Name,
                connectivity = result.B1.status,
                last_cnnection = result.B1.Date
            });
            connections.Add(new biller
            {
                name = result.B2.Name,
                connectivity = result.B2.status,
                last_cnnection = result.B2.Date
            });

            //List<biller> connections = new List<biller>();
            //connections.Add(new biller
            //{
            //    name = "CoreBank",
            //    connectivity = "Connected",
            //    last_cnnection = "-"
            //});
            //connections.Add(new biller
            //{
            //    name = "NMSF",
            //    connectivity = "Connected",
            //    last_cnnection = "-"
            //});
            //connections.Add(new biller
            //{
            //    name = "EPORT",
            //    connectivity = "Not Connected",
            //    last_cnnection = "-"
            //});
            //connections.Add(new biller
            //{
            //    name = "EBS",
            //    connectivity = "Connected",
            //    last_cnnection = "-"
            //});

            Session["connections"] = connections;
            return View();
        }
    }
}