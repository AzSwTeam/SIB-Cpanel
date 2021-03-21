using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace SFBCPanel
{
    public class User
    {
        
        public int UserID { get; set; }


        [Required(ErrorMessage = "Please enter the User Name")]  
        public String Username { get; set; }


        [Required(ErrorMessage = "Please enter the Password")]  
        public String Password { get; set; }

        public bool RememberMe { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }

    }
    public class Role
    {
        [Key]
        public short RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
    }

    public class UserRole
    {
        [Key]
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public short RoleId { get; set; }

        public virtual Role Role { get; set; }
    }



    public class LoginLogic
    {
        public String IPAdd;
        public String currentIP;
        public String LastLogin;
        public String PrimaryAccount;
        public string userTel;
        public int userRole = 1;
        public String useremail;
        public String userStatus;
        public String FirstLogin;
        public String name;
        public String loginStatus;
        private string conString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        //public object OracleType { get; private set; }

        public int checkUserLogin(User user, string current_IP)
        {
            String s = "";
            int res = 0;
            String ID="0";
            
            

            using (OracleConnection conObj = new OracleConnection(conString))
            {
                try
                {
                    
                    OracleCommand comObj = new OracleCommand("IB_Login", conObj);
                    comObj.CommandType = CommandType.StoredProcedure;
                    comObj.Parameters.Add(new OracleParameter("p_username", user.Username));
                    comObj.Parameters.Add(new OracleParameter("p_pwd", user.Password));

                    comObj.Parameters.Add("p_result", OracleType.Int32, 1);
                    comObj.Parameters["p_result"].Direction = ParameterDirection.Output;

                    comObj.Parameters.Add("p_roleid", OracleType.Int32, 1);
                    comObj.Parameters["p_roleid"].Direction = ParameterDirection.Output;

                    comObj.Parameters.Add("p_ipAdd", OracleType.VarChar, 50);
                    comObj.Parameters["p_ipAdd"].Direction = ParameterDirection.Output;

                    comObj.Parameters.Add("p_lastLogin", OracleType.VarChar, 50);
                    comObj.Parameters["p_lastLogin"].Direction = ParameterDirection.Output;

                    comObj.Parameters.Add("p_accNo", OracleType.VarChar, 50);
                    comObj.Parameters["p_accNo"].Direction = ParameterDirection.Output;

                    comObj.Parameters.Add("p_tel", OracleType.VarChar, 50);
                    comObj.Parameters["p_tel"].Direction = ParameterDirection.Output;

                    comObj.Parameters.Add("p_email", OracleType.VarChar, 50);
                    comObj.Parameters["p_email"].Direction = ParameterDirection.Output;

                    comObj.Parameters.Add("p_status", OracleType.VarChar, 50);
                    comObj.Parameters["p_status"].Direction = ParameterDirection.Output;

                    comObj.Parameters.Add("p_first_login", OracleType.VarChar, 50);
                    comObj.Parameters["p_first_login"].Direction = ParameterDirection.Output;


                    comObj.Parameters.Add("p_name", OracleType.VarChar, 50);
                    comObj.Parameters["p_name"].Direction = ParameterDirection.Output;

                    comObj.Parameters.Add("p_loginstat", OracleType.VarChar, 50);
                    comObj.Parameters["p_loginstat"].Direction = ParameterDirection.Output;
                    
                    //comObj.Parameters.Add(new OracleParameter(":p_result", OracleType.Number).Direction =
                    //    ParameterDirection.ReturnValue);
                    try
                    {
                        conObj.Open();
                        //return comObj.ExecuteNonQuery();
                        res = Convert.ToInt32(comObj.ExecuteScalar());

                        userRole = Convert.ToInt32(comObj.Parameters["p_roleid"].Value);
                        
                         ID            = comObj.Parameters["p_result"].Value.ToString();
                         IPAdd         = comObj.Parameters["p_ipAdd"].Value.ToString();
                         currentIP     = GetIPAddress();
                        LastLogin      = comObj.Parameters["p_lastLogin"].Value.ToString();
                        PrimaryAccount = comObj.Parameters["p_accNo"].Value.ToString();
                        userTel        = comObj.Parameters["p_tel"].Value.ToString();
                        useremail      = comObj.Parameters["p_email"].Value.ToString();
                        userStatus     = comObj.Parameters["p_status"].Value.ToString();
                        FirstLogin     = comObj.Parameters["p_first_login"].Value.ToString();
                        name = comObj.Parameters["p_name"].Value.ToString();
                        loginStatus = comObj.Parameters["p_loginstat"].Value.ToString();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        
                    }
                    
                    conObj.Close();
                }
                catch (Exception e)
                {
                    s = e.Message;
                    Console.WriteLine(e);
                }
                
            }
            res = Convert.ToInt32(ID);
            return res;
        }

        public string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        public string GetLanIPAddress()
        {
            //Get the Host Name
            string stringHostName = Dns.GetHostName();
            //Get The Ip Host Entry
            IPHostEntry ipHostEntries = Dns.GetHostEntry(stringHostName);
            //Get The Ip Address From The Ip Host Entry Address List
            IPAddress[] arrIpAddress = ipHostEntries.AddressList;
            return arrIpAddress[arrIpAddress.Length - 1].ToString();
        }

        ///////////////////////////////////////////////////////////////////////
        /// 
        public DataTable GetMenuData(int MenuParentID)
        {
            OracleDataAdapter da;
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            using (OracleConnection conObj = new OracleConnection(conString))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand("SELECT *  FROM menu_test WHERE menu_parent_id='" + MenuParentID + "'", conObj);
                    da = new OracleDataAdapter(cmd);
                    da.Fill(ds);
                    conObj.Open();
                    cmd.ExecuteNonQuery();
                    conObj.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    
                }
                
            }
            return ds.Tables[0];
        }


        public void PopulateMenu(DataTable dt, int Menu_Parent_ID, MenuItem Parent_MenuItem)
        {

            string currentPage = Path.GetFileName(HttpContext.Current.Request.Url.AbsolutePath);
            foreach (DataRow row in dt.Rows)
            {
                MenuItem menuItem = new MenuItem
                {
                    Value = row["menu_id"].ToString(),
                    Text = row["menu_name"].ToString(),
                    NavigateUrl = row["menu_page_url"].ToString(),
                    Selected = row["menu_page_url"].ToString().EndsWith(currentPage, StringComparison.CurrentCultureIgnoreCase)
                };
                if (Menu_Parent_ID == 0)
                {
                    //IBLogic.Items.Add(menuItem);
                    DataTable dtChildMenu = new DataTable();
                    dtChildMenu = this.GetMenuData(int.Parse(menuItem.Value));
                    PopulateMenu(dtChildMenu, int.Parse(menuItem.Value), menuItem);
                }
                else
                {
                    Parent_MenuItem.ChildItems.Add(menuItem);
                }
            }   
            
        }
        

        ///////////////////////////////////////////////////////////////////
        /// populate Dropdown list
        public List<SelectListItem> populateAccounts(string user_id)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            //string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            //private string conString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " SELECT acc_id,acc_no from user_acc_link where user_id =" + user_id;
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = dr["acc_no"].ToString(),
                                Value = dr["acc_no"].ToString()
                                //Value = dr["acc_id"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        //----------------------------------------------------ADD CARD SERVICE------------------------------------------------
        //Add Card
        public void AddCard(string cardName,string cardNo,string cardExp, int userID)
        {
            //var myLastID = "SELECT *FROM (select card_id from user_card_link) cards WHERE rownum <= 1 ORDER BY rownum DESC;";

            var LastCardID = "select max(card_id)+ 1 from user_card_link";
            int max_card_id;


            //var sql = "INSERT INTO user_card_link('card_id','card_name','card_number','card_exp','user_id') VALUES ('" +
            //          last_id + "', '" + cardName + "','" + cardNo + "','" + cardExp + "','" + userID + "')";

           // OracleDataReader dr;
            using (OracleConnection con = new OracleConnection(conString))
            {

                using (OracleCommand idCmd = new OracleCommand(LastCardID))
                {
                    idCmd.Connection = con;
                    con.Open();
                    using (OracleDataReader dr = idCmd.ExecuteReader())
                    {
                        dr.Read();
                        max_card_id = Convert.ToInt32(dr[0]) + 1;
                    }
                   
                    con.Close();

                }
                using (OracleCommand cmd = new OracleCommand(""))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "ADDCARD";

                    OracleParameter pr1 = new OracleParameter("status", OracleType.VarChar, 2000)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(pr1);
                    cmd.Parameters.Add("max_card_id", OracleType.Int32).Value = max_card_id;
                    cmd.Parameters.Add("card_name", OracleType.VarChar).Value = cardName;
                    cmd.Parameters.Add("card_number", OracleType.VarChar).Value = cardNo;
                    cmd.Parameters.Add("card_exp", OracleType.VarChar).Value = cardExp;
                    cmd.Parameters.Add("user_id", OracleType.VarChar).Value = userID;

                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                }
            }


        }




        //----------------------------------------------------ADD Favorite Meter SERVICE------------------------------------------------
        //Add Card
        public void AddMeter(string meterName, string meterNo, int userID)
        {
            //var myLastID = "SELECT *FROM (select card_id from user_card_link) cards WHERE rownum <= 1 ORDER BY rownum DESC;";

            /*var LastMeterID = "select max(meter_id)+ 1 from user_meter_link";*/
            var LastMeterID = "select max(meter_id) from user_meter_link";
            int max_meter_id;

            // OracleDataReader dr;
            using (OracleConnection con = new OracleConnection(conString))
            {

                using (OracleCommand idCmd = new OracleCommand(LastMeterID))
                {
                    idCmd.Connection = con;
                    con.Open();
                    using (OracleDataReader dr = idCmd.ExecuteReader())
                    {
                        dr.Read();
                        if (dr[0] is DBNull) //check if it is the first entry... then dr would be DBNull
                        {
                            max_meter_id = 1;
                        }
                        else
                        {
                            max_meter_id = Convert.ToInt32(dr[0]) + 1;
                        }
                        
                    }

                    con.Close();

                }

                //var sql = "INSERT INTO user_meter_link VALUES ('" + max_meter_id + "', '" + meterName + "','" + meterNo + "','" + userID + "')";

                using (OracleCommand cmd = new OracleCommand(""))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "ADDMETER";

                    cmd.Parameters.Add("max_meter_id", OracleType.Int32).Value = max_meter_id;
                    cmd.Parameters.Add("metername", OracleType.VarChar).Value = meterName;
                    cmd.Parameters.Add("meterno", OracleType.VarChar).Value = meterNo;
                    cmd.Parameters.Add("userid", OracleType.Int32).Value = userID;
                    OracleParameter pr1 = new OracleParameter("status", OracleType.VarChar, 2000);
                    pr1.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pr1);

                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                }
            }


        }



        //----------------------------------------------------ADD Favorite Phone SERVICE------------------------------------------------
        //Add Card
        public void AddPhone(string PhoneName, string PhoneNo, int userID)
        {
           

            /*var LastPhoneID = "select max(phone_id)+ 1 from user_phone_link";*/
            var LastPhoneID = "select max(phone_id) from user_phone_link";
            int max_phone_id;

            // OracleDataReader dr;
            using (OracleConnection con = new OracleConnection(conString))
            {

                using (OracleCommand idCmd = new OracleCommand(LastPhoneID))
                {
                    idCmd.Connection = con;
                    con.Open();
                    using (OracleDataReader dr = idCmd.ExecuteReader())
                    {
                        dr.Read();
                        if (dr[0] is DBNull) //check if it is the first entry... then dr would be DBNull
                        {
                            max_phone_id = 1;
                        }
                        else
                        {
                            max_phone_id = Convert.ToInt32(dr[0]) + 1;
                        }

                    }

                    con.Close();

                }

                //var sql = "INSERT INTO user_phone_link VALUES ('" + max_phone_id + "', '" + PhoneName + "','" + PhoneNo + "','" + userID + "')";

                using (OracleCommand cmd = new OracleCommand(""))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "ADDPHONE";

                    cmd.Parameters.Add("max_phone_id", OracleType.Int32).Value = max_phone_id;
                    cmd.Parameters.Add("phonename", OracleType.VarChar).Value = PhoneName;
                    cmd.Parameters.Add("phoneno", OracleType.VarChar).Value = PhoneNo;
                    cmd.Parameters.Add("userid", OracleType.Int32).Value = userID;
                    OracleParameter pr1 = new OracleParameter("status", OracleType.VarChar, 2000);
                    pr1.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pr1);

                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                }
            }


        }


        ///////////////////////////Run Query////////////////////////////////
        OracleDataReader RunQuery(String QueryString)
        {

            //Dim ConnectionString As String = spark.DbConnect.ConStr '"Data Source=orcl;User ID=private;Password=private"
            //Dim DBConnection As OracleConnection = New OracleConnection(ConnectionString)
            OracleCommand cmd;
            OracleDataReader dr;
            //If DBConnection.State = ConnectionState.Closed Then
            //    DBConnection.Open()
            //End If

            using (OracleConnection conObj = new OracleConnection(conString))
            {
                cmd = new OracleCommand(QueryString, conObj);
                try
                {
                    dr = cmd.ExecuteReader();
                    return dr;
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                    return null;
                }
            }
          
        }

        //----------------------------PopulateMenus----------------------------------------

        //void PopulateMenus(TreeNode node)
        //{
        //    //' Query for the product categories. These are the values
        //    //' for the second-level nodes.
        //    OracleDataReader reader = RunQuery("select * from MainMenu,usermenu where mainmenuid = menuid and userid =" + Session["userid"].ToString() + " order by menuid");
        //    //' Create the second-level nodes.
        //    try
        //    {
        //        if (!reader.Equals(null))
        //        {
        //            //' Iterate through and create a new node for each row in the query results.
        //            //' Notice that the query results are stored in the table of the DataSet.
        //            String flag = "";
        //            while (reader.Read())
        //            {
        //                // ' Create the new node. Notice that the CategoryId is stored in the Value property 
        //                //' of the node. This will make querying for items in a specific category easier when
        //                //' the third-level nodes are created. 
        //                if (flag.Equals(reader["menudesc"].ToString()))
        //                {
        //                    TreeNode newNode = new TreeNode();
        //                    newNode.Text = reader["menudesc"].ToString();
        //                    newNode.Value = reader["menuID"].ToString();
        //                    // ' Set the PopulateOnDemand property to true so that the child nodes can be 
        //                    //' dynamically populated.
        //                    newNode.PopulateOnDemand = true;
        //                    //' Set additional properties for the node.
        //                    newNode.SelectAction = TreeNodeSelectAction.Expand;
        //                    //' Add the new node to the ChildNodes collection of the parent node.
        //                    node.ChildNodes.Add(newNode);
        //                }
        //                flag = reader["menudesc"].ToString();
        //            }
        //            reader.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Write(ex.Message);
        //    }

        //}

        
    }
}
