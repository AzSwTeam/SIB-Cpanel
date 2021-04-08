using SFBCpanel.Models;
using SFBCPanel.Context;
using SFBCPanel.Models;
using SFBCPanel.Repository;
using SIBCPanel.Context;
using SIBCPanel.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SFBCPanel.Controllers
{
    public class HomeController : Controller
    {
        DataSource ds = new DataSource();
        LoginLogic obj = new LoginLogic();
        LoginLogic userBL = new LoginLogic();

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
            if (Session["Homemessage"] != null)
            {
                ViewBag.SuccessMessage = Session["Homemessage"].ToString();
                Session["Homemessage"] = null;
            }

            string branchcode = Session["user_branch"].ToString();
            string adminname = Session["user_name"].ToString();
            ViewData["adminname"] = adminname;

            // charts and online / offline customers panel

            List<int> list = ds.GetOnlineOfflineUsers(branchcode);
            string online = list[0].ToString(); ViewBag.Online = online;
            string offline = list[4].ToString(); ViewBag.Offline = offline;

            // getting all transactions log

            List<Charter> usersperbranchscount = ds.getUsersBranchsCount();
            Session["usersperbranchscount"] = usersperbranchscount;
            List<Charter> usersstatuses = ds.getAllStatuses();
            Session["usersstatuses"] = usersstatuses;
            List<Charter> branchstransactionscount = ds.getBranchsTransactionsCount();
            Session["branchstransactionscount"] = branchstransactionscount;


            return View();

        }

        public ActionResult TransactionsStatuses()
        {
            string branchcode = Session["user_branch"].ToString();
            List<TransactionStatusesModel> transactionsstatuses = new List<TransactionStatusesModel>();
            transactionsstatuses = ds.GetTransactionStatusesDetails(branchcode);
            return View(transactionsstatuses);


        }

        public ActionResult NumberOfAccounts()
        {
            string user_id = Session["UserID"].ToString();

            string count = ds.GetAccountsCount(user_id);

            ViewBag.AccCount = count;


            return PartialView();
        }


        public ActionResult NumberOfTransfers()
        {
            string user_id = Session["UserID"].ToString();

            string count = ds.GetTransferCount(user_id);

            ViewBag.TranCount = count;


            return PartialView();
        }
        public virtual ActionResult TopMenu()
        {
            int myRole = obj.userRole;
            if (myRole == 1)
            {
                //Roles.AddUserToRole("user", "Admin");
            }
            if (Session["username"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.username = Session["username"].ToString();

            IEnumerable<Menu> Menu = null;

            if (Session["_Menu"] != null)
            {
                Menu = (IEnumerable<Menu>)Session["_Menu"];
            }
            else
            {
                //return RedirectToAction("Login", "Login");
                string user_id = Session["UserID"].ToString();
                string user_role = Session["user_roleid"].ToString();
                Menu = MenuData.GetMenus(user_id, user_role);// pass employee id here
                Session["_Menu"] = Menu;
            }
            return PartialView(Menu);
        }

        public ActionResult Logout()
        {
            Session["cpanelLogin"] = "0";
            Session["cpanel_Menu"] = null;
            Session["UserID"] = null;
            Session["user_roleid"] = null;
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            return RedirectToAction("Login", "Login");
        }

        public ActionResult test2()
        {
            if (Session["Homemessage"] != null)
            {
                ViewBag.SuccessMessage = Session["Homemessage"].ToString();
                Session["Homemessage"] = null;
            }
            return View();
        }
        public ActionResult Test()
        {
            ViewBag.Message = "Your Test page.";

            return View();
        }
    }

}
