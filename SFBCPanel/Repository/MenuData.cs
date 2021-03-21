using SFBCPanel.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;

namespace SFBCPanel.Repository
{
    public class MenuData
    {
        public static IList<Menu> GetMenus(string usernumber, string rolenumber)
        {
            /* using ado.net code */
            using (OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString()))
            {
                List<Menu> menuList = new List<Menu>();
                OracleCommand cmd = new OracleCommand("usp_GetMenuData2", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("usernumber", usernumber);
                cmd.Parameters.AddWithValue("rolenumber", rolenumber);
                cmd.Parameters.Add("menucur", OracleType.Cursor).Direction = System.Data.ParameterDirection.Output;

                con.Open();
                IDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    Menu menu = new Menu();
                    menu.MID = Convert.ToInt32(sdr["MID"].ToString());
                    menu.MenuName = sdr["MenuName"].ToString();
                    menu.MenuURL = sdr["MenuURL"].ToString();
                    menu.MenuIMG = sdr["MenuIMG"].ToString();
                    menu.MenuParentID = Convert.ToInt32(sdr["MenuParentID"].ToString());
                    menu.subMenuParentID = Convert.ToInt32(sdr["subMenuParentID"].ToString());
                    menuList.Add(menu);
                }
                return menuList;
            }
        }
    }
}