using SFBCPanel.Context;
using SFBCPanel.Models;
using SIBCPanel.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SFBCPanel.Controllers
{
    public class CPanelProfileManagementController : Controller
    {

        DataSource data = new DataSource();

        // displaying profiles
        public ActionResult ProfileManagement()
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

            if (Session["userresult"] != null)
            {
                ViewBag.SuccessMessage = Session["userresult"].ToString();
                Session["userresult"] = null;
            }
            List<profilelist> profiles = data.GetAllProfiles();
            // ViewBag.UserList = dataset.Tables[0];
            return View(profiles);
        }

        //deleting item on profiles list
        public ActionResult Delete(int roleid)
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
            int userscount = data.getcpanelprofileuserscount(roleid);
            if (userscount > 0)
            {
                TempData["deletefail"] = "Profile Cannot be deleted while containing users";
                ViewBag.SuccessMessage = Session["addprofileresult"];
                return RedirectToAction("ProfileManagement");
            }
            else
            {
                int records = data.deletecpanelprofile(roleid);
                if (records > 0)
                {
                    Session["addprofileresult"] = "Profile deleted successfully";
                    return RedirectToAction("ProfileManagement");
                }
                else
                {
                    ModelState.AddModelError("", "Can Not Delete");
                    return View("ProfileManagement", "CPanelProfileManagement");
                }
            }
        }

        // GET:adding new cpanel profile 
        public ActionResult CPanelProfileManagement()
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
            if (Session["addprofileresult"] != null)
            {
                ViewBag.SuccessMessage = Session["addprofileresult"].ToString();
                Session["addprofileresult"] = null;

            }
            Session["profilelist"] = null;
            Session["menu_category"] = null;

            CPanel_ProfileManagement model = new CPanel_ProfileManagement();

            model.catgories = data.GetGatgories();
            return View(model);
        }

        [HttpPost]
        public ActionResult CPanelProfileManagement(CPanel_ProfileManagement model)
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
            model.catgories = data.GetGatgories();
            var selectedcategory = model.catgories.Find(p => p.Value == model.menu_category.ToString());
            if (selectedcategory != null)
            {
                selectedcategory.Selected = true;
            }
            if (model.menu_category != null)
            {
                Session["profilelist"] = true;
                Session["menu_category"] = model.menu_category;
                List<pageparameter> items = new List<pageparameter>();
                items = data.PopulateProfilemangement(model.menu_category);

                model.pages = items;
                return View(model);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Addprofile(CPanel_ProfileManagement model)
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
            String result = "", res = "";
            String message;
            try
            {
                model.catgories = data.CPanel_GetGatgories();
                if (ModelState.IsValidField(model.profilename))
                {

                    List<pageparameter> lHob = new List<pageparameter>();
                    lHob = model.pages;
                    foreach (var item in lHob)
                    {
                        if (item.IsSelected == true)
                        {
                            result = data.cpanel_addnewprofile(model.profilename, item.menuid, item.menuparentid);
                            res += " " + item.menuname + " : " + result;
                            Session["addprofileresult"] = "Profile Creation  Complete Successfully";
                        }
                    }
                    return RedirectToAction("CPanelProfileManagement");
                }
                else
                {
                    message = "All Fields are required ";
                    ModelState.AddModelError("", "Something is missing" + message);
                }
            }
            catch (Exception ex)
            {
                message = "Please Contact for Support";
                ModelState.AddModelError("", "Something is missing" + message);
            }
            return View("CPanelProfileManagement", model);
        }

        public ActionResult View(int roleid)
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
            List<Menu> pages = new List<Menu>();
            pages = data.GetRoleMenu(roleid.ToString());
            return View(pages);
        }

        public ActionResult Edit(int roleid)
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
            List<Menu> selectedpages = new List<Menu>();
            selectedpages = data.GetRoleMenu(roleid.ToString());
            List<pageparameter> allpages = new List<pageparameter>();
            string profilename = data.GetCpanelprofilename(roleid.ToString());
            allpages = data.PopulateProfilemangement("1");
            foreach (pageparameter page in allpages)
            {
                foreach (Menu item in selectedpages)
                {
                    if (item.MID.ToString() == page.menuid)
                    {
                        page.IsSelected = true;
                    }
                }
            }
            Session["roletoedit"] = roleid.ToString();
            CPanel_ProfileManagement model = new CPanel_ProfileManagement();
            model.pages = allpages;
            model.menu_category = "1";
            model.profilename = profilename;
            return View(model);
        }

        [HttpPost]
        public ActionResult DoEdit(CPanel_ProfileManagement model)
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
            String result = "", res = "";
            String message;
            try
            {
                model.catgories = data.CPanel_GetGatgories();
                string profileid = Session["roletoedit"].ToString();
                //data.cpancel_deleteexitingrole(profileid);
                string profilename = data.GetCpanelprofilename(profileid);
                model.profilename = profilename;
                if (ModelState.IsValidField(model.profilename))
                {
                    List<pageparameter> lHob = new List<pageparameter>();
                    lHob = model.pages;
                    data.cpancel_deleteexitingrole(profileid);
                    foreach (var item in lHob)
                    {
                        if (item.IsSelected == true)
                        {
                            result = data.cpanel_editprofile(profilename, item.menuid, item.menuparentid, profileid);
                            res += " " + item.menuname + " : " + result;
                            TempData["profileupdated"] = "Profile [" + profilename + "] has been updated";
                        }
                    }
                    return RedirectToAction("ProfileManagement");
                }
                else
                {
                    message = "All Fields are required ";
                    ModelState.AddModelError("", "Something is missing" + message);
                }
            }
            catch (Exception ex)
            {
                message = "Please Contact for Support";
                ModelState.AddModelError("", "Something is missing" + message);
            }
            return View("CPanelProfileManagement", model);
        }

    }
}