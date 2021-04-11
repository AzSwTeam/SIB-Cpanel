using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web.Mvc;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using cpanel.Models;
using System.Data.OracleClient;
using SFBCPanel.Models;
using SFBCPanel;
using SFBCpanel.Models;
using SIBCPanel.Models;

namespace SIBCPanel.Context
{
    public class DataSource
    {
        //ConnectionString....
        private string conString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        //-----------------------GET chq------------------------------------------------------
        //
        public int updatechqsts(int id, string sts)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "update  cheque_reqs set req_status='" + sts + "'  where request_id=" + id;

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();
                int result = -1;
                result = cmd.ExecuteNonQuery();
                return result;
            }
        }

        public List<Menu> GetCustomerRoleMenu(string rolenumber)
        {
            /* using ado.net code */
            using (OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString()))
            {
                List<Menu> menuList = new List<Menu>();
                OracleCommand cmd = new OracleCommand("GETCUSTOMERROLEMENUDATA", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("rolenumber", rolenumber);
                cmd.Parameters.Add("menucur", OracleType.Cursor).Direction = System.Data.ParameterDirection.Output;

                con.Open();
                IDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    Menu menu = new Menu();
                    menu.MID = int.Parse(sdr["MID"].ToString());
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

        public string Getcustomerprofilename(string roleid)
        {
            string profilename = "N/A";
            List<CustomerTransferReportViewModel> items = new List<CustomerTransferReportViewModel>();
            using (OracleConnection con = new OracleConnection(conString))
            {

                string query = "select name from tbl_rolemaster where roleid = '" + roleid + "'";

                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            profilename = dr["name"].ToString();
                        }
                        con.Close();
                    }
                }
            }
            return profilename;
        }

        public int deleteprofile(int roleid)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("delete from tbl_rolemaster where roleid = '" + roleid + "'", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }

                return cmd.ExecuteNonQuery();
            }
        }

        internal string tbl_deleteexitingrole(string roleid)
        {
            string lblconfirm = "";
            OracleDataReader dr;
            OracleCommand cmd_acc_lnk;
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd;
                try
                {
                    con.Open();
                    cmd = new OracleCommand("delete from tbl_rolemenumapping where roleid = '" + roleid + "'", con);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                catch (Exception ex)
                {
                    lblconfirm = "System Error - " + ex.Message;
                }
            }
            return lblconfirm;
        }

        public List<CustomerTransferReportViewModel> GetCurrentTransactions()
        {
            List<CustomerTransferReportViewModel> transactions = new List<CustomerTransferReportViewModel>();

            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select users.user_name,trans_log.tran_req,tran_name,tran_amount,tran_status,tran_token,tran_resp_date from trans_log inner join users on trans_log.user_id = users.user_id where rownum <= 30 order by trans_log.tran_resp_date desc", con);
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    transactions.Add(new CustomerTransferReportViewModel
                    {
                        CustomerName = dr["user_name"].ToString(),
                        TranFullReq = dr["tran_req"].ToString(),
                        TranName = dr["tran_name"].ToString(),
                        TranReqAmount = dr["tran_amount"].ToString(),
                        TranStatus = dr["tran_status"].ToString(),
                        TranToken = dr["tran_token"].ToString(),
                        TranDate = dr["tran_resp_date"].ToString()
                    });
                }
                dr.Close();
                con.Close();
            }
            return transactions;
        }

        internal string tbl_editprofile(string profilename, string menuid, string parnetid, string profileid)
        {
            String lblconfirm = "System Error";
            bool FLAG;
            OracleCommand cmd;
            OracleDataReader dr;
            using (OracleConnection con = new OracleConnection(conString))
            {
                try
                {
                    cmd = new OracleCommand("select * from tbl_rolemaster where name='" + profilename + "'", con);
                    OracleCommand cmd2;
                    OracleCommand cmd_acc_lnk;

                    con.Open();
                    cmd = new OracleCommand("select   max(to_number(nvl(id,0))) maxid from tbl_rolemenumapping", con);

                    String id = cmd.ExecuteScalar().ToString();
                    int newid = int.Parse(id);
                    newid = newid + 1;
                    cmd = new OracleCommand("select id,roleid,menuid,active from tbl_rolemenumapping where  menuid='" + parnetid + "' and roleid='" + profileid + "'", con);
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        cmd_acc_lnk = new OracleCommand("INSERT INTO tbl_rolemenumapping (ID,ROLEID,MENUID,ACTIVE)VALUES('"
                           + newid + "','" + profileid + "','" + menuid + "','1')", con);
                        cmd_acc_lnk.ExecuteNonQuery();
                        lblconfirm = "Account Added Successfully";
                    }
                    else
                    {
                        cmd_acc_lnk = new OracleCommand("INSERT INTO tbl_rolemenumapping (ID,ROLEID,MENUID,ACTIVE)VALUES('"
                            + newid + "','" + profileid + "','" + parnetid + "','1')", con);
                        cmd_acc_lnk.ExecuteNonQuery();
                        int nextid = newid + 1;
                        cmd_acc_lnk = new OracleCommand("INSERT INTO tbl_rolemenumapping (ID,ROLEID,MENUID,ACTIVE)VALUES('"
                           + nextid + "','" + profileid + "','" + menuid + "','1')", con);
                        cmd_acc_lnk.ExecuteNonQuery();
                        lblconfirm = "Account Added Successfully";
                    }
                    con.Close();
                }
                catch (Exception ex)
                {
                    lblconfirm = "System Error";
                }
            }
            return lblconfirm;
        }

        public string GetCpanelprofilename(string roleid)
        {
            string profilename = "N/A";
            List<CustomerTransferReportViewModel> items = new List<CustomerTransferReportViewModel>();
            using (OracleConnection con = new OracleConnection(conString))
            {

                string query = "select name from cpanel_rolemaster where roleid = '" + roleid + "'";

                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            profilename = dr["name"].ToString();
                        }
                        con.Close();
                    }
                }
            }

            return profilename;
        }

        internal string cpanel_editprofile(string profilename, string menuid, string parnetid, string profileid)
        {
            String lblconfirm = "System Error";
            bool FLAG;
            OracleCommand cmd;
            OracleDataReader dr;
            using (OracleConnection con = new OracleConnection(conString))
            {
                try
                {
                    cmd = new OracleCommand("select * from cpanel_rolemaster where name='" + profilename + "'", con);
                    OracleCommand cmd2;
                    OracleCommand cmd_acc_lnk;

                    con.Open();
                    cmd = new OracleCommand("select   max(to_number(nvl(id,0))) maxid from cpanel_rolemenumapping", con);

                    String id = cmd.ExecuteScalar().ToString();
                    int newid = int.Parse(id);
                    newid = newid + 1;
                    cmd = new OracleCommand("select id,roleid,menuid,active from cpanel_rolemenumapping where  menuid='" + parnetid + "' and roleid='" + profileid + "'", con);
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        cmd_acc_lnk = new OracleCommand("INSERT INTO cpanel_rolemenumapping (ID,ROLEID,MENUID,ACTIVE)VALUES('"
                           + newid + "','" + profileid + "','" + menuid + "','1')", con);
                        cmd_acc_lnk.ExecuteNonQuery();
                        lblconfirm = "Account Added Successfully";

                    }
                    else
                    {
                        cmd_acc_lnk = new OracleCommand("INSERT INTO cpanel_rolemenumapping (ID,ROLEID,MENUID,ACTIVE)VALUES('"
                            + newid + "','" + profileid + "','" + parnetid + "','1')", con);
                        cmd_acc_lnk.ExecuteNonQuery();
                        int nextid = newid + 1;
                        cmd_acc_lnk = new OracleCommand("INSERT INTO cpanel_rolemenumapping (ID,ROLEID,MENUID,ACTIVE)VALUES('"
                           + nextid + "','" + profileid + "','" + menuid + "','1')", con);
                        cmd_acc_lnk.ExecuteNonQuery();
                        lblconfirm = "Account Added Successfully";
                    }
                    con.Close();



                }
                catch (Exception ex)
                {
                    lblconfirm = "System Error";
                }
            }
            return lblconfirm;
        }

        internal string cpancel_deleteexitingrole(string roleid)
        {
            string lblconfirm = "";
            OracleDataReader dr;
            OracleCommand cmd_acc_lnk;
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd;
                try
                {
                    con.Open();
                    cmd = new OracleCommand("delete from cpanel_rolemenumapping where roleid = '" + roleid + "'", con);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                catch (Exception ex)
                {
                    lblconfirm = "System Error - " + ex.Message;
                }
            }
            return lblconfirm;
        }

        public List<Menu> GetRoleMenu(string rolenumber)
        {
            /* using ado.net code */
            using (OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString()))
            {
                List<Menu> menuList = new List<Menu>();
                OracleCommand cmd = new OracleCommand("GETROLEMENUDATA", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("rolenumber", rolenumber);
                cmd.Parameters.Add("menucur", OracleType.Cursor).Direction = System.Data.ParameterDirection.Output;

                con.Open();
                IDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    Menu menu = new Menu();
                    menu.MID = int.Parse(sdr["MID"].ToString());
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

        public List<SelectListItem> tbl_GetGatgories()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            int i = 0;

            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select  cat_id,cat_name from CATEGORY";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {

                            items.Add(new SelectListItem
                            {
                                Text = "-- Select Customer category --",
                                Value = "0",
                            });
                            while (sdr.Read())
                            {

                                items.Add(new SelectListItem
                                {
                                    Text = sdr[1].ToString(),
                                    Value = sdr[0].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
            }

            return items;

        }

        public int getprofileuserscount(int roleid)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select count(user_log) as userscount from users where roleid = '" + roleid + "'", con);
                int count = 0;
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                    OracleDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        count = int.Parse(dr[0].ToString());
                    }
                }
                return count;
            }
        }

        public int updatecard(int id, string sts)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "update  card_reqs set req_status='" + sts + "'  where request_id=" + id;

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();
                int result = -1;
                result = cmd.ExecuteNonQuery();
                return result;
            }
        }

        public Boolean refreshcustomer(int userid, string customername, string phonenumber)
        {
            Boolean response = false;
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "update users set user_name = '" + customername + "',user_mobile = '" + phonenumber + "' where user_id = '" + userid + "'";
                OracleCommand cmd = new OracleCommand(query, con);
                con.Open();
                int result = -1;
                result = cmd.ExecuteNonQuery();
                if (result == -1)
                {
                    response = false;
                }
                else
                {
                    response = true;
                }
            }
            return response;
        }

        public string GetCurrencyName(string CurrencyCode)
        {
            string branchs = "", CurrencyName = "";

            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("", con);
                cmd.CommandText = "select curr_name from currency where curr_code = '" + CurrencyCode + "'";

                con.Open();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {


                        if (dataReader["curr_name"] != DBNull.Value)
                        {
                            CurrencyName = (string)dataReader["curr_name"];

                        }
                    }
                    //branchs = branchs.Substring(1);
                    return CurrencyName;
                }
            }
        }

        public string getgroupmaxid()
        {
            string maxcount = "";
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("GETGROUPMAXID", con);
                cmd.CommandType = CommandType.StoredProcedure;
                if (con.State == ConnectionState.Closed)
                { con.Open(); }

                OracleParameter p3 = new OracleParameter("status", OracleType.VarChar, 2000);
                p3.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(p3);
                OracleParameter p1 = new OracleParameter("maxid", OracleType.VarChar, 2000);
                p1.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add("p_cr", OracleType.Cursor).Direction = ParameterDirection.Output;

                OracleDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    maxcount = dr["MAX(ID)"].ToString();
                }

                return maxcount;
            }
        }

        public int insertservice(ServiceInsertModel model)
        {

            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("INSERT INTO SERVICES  (service_id,service_name,service_code,service_status)VALUES(SERVSEQ.nextval,'" + model.service_name + "', RPAD(SERVSEQ.currval, 5, '0'),'A')", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }

                return cmd.ExecuteNonQuery();
            }
        }
        public int insert(userInsertModel model)
        {
            OracleCommand cmd;
            using (OracleConnection con = new OracleConnection(conString))
            {
                if (model.roleid == "2")
                {
                    cmd = new OracleCommand("INSERT INTO security_master (USER_LOG,USER_PASS,USER_NAME,USER_LASTLOGIN,USER_LASTWORK,USER_ID,ROLEID,USER_BRANCH,USER_PAS,USER_STAT)VALUES('" + model.user_name + "','R6K2zyfxJqbwmXqixfkRMw==','" + model.name + "','T',NULL,CP_USERID.nextval,'" + model.roleid + "','000',NULL,'A')", con);
                }
                else
                {
                    cmd = new OracleCommand("INSERT INTO security_master (USER_LOG,USER_PASS,USER_NAME,USER_LASTLOGIN,USER_LASTWORK,USER_ID,ROLEID,USER_BRANCH,USER_PAS,USER_STAT)VALUES('" + model.user_name + "','R6K2zyfxJqbwmXqixfkRMw==','" + model.name + "','T',NULL,CP_USERID.nextval,'" + model.roleid + "','" + model.BranchCode + "',NULL,'A')", con);
                }
                if (con.State == ConnectionState.Closed)
                { con.Open(); }

                return cmd.ExecuteNonQuery();
            }
        }
        public int Update(userUpdateModel model)
        {

            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("Update security_master set user_log ='" + model.user_name + "',user_name ='" + model.name + "',roleid='" + model.roleid + "',user_branch='" + model.BranchCode + "' where  user_id='" + model.user_id + "'", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }

                return cmd.ExecuteNonQuery();
            }
        }
        public int UpdateService(ServiceUpdateModel model)
        {

            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("Update SERVICES  set service_name ='" + model.service_name + "',service_status = '" + model.service_status + "' where  service_id='" + model.service_id + "'", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }

                return cmd.ExecuteNonQuery();
            }
        }
        public userUpdateModel getuserdata(int id)
        {
            userUpdateModel updatemodel = new userUpdateModel();

            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select  user_id, user_log, user_name,roleid,user_branch  from security_master where user_id=" + id, con);
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {

                    updatemodel.user_id = Convert.ToInt32(dr["user_id"].ToString());
                    updatemodel.roleid = dr["roleid"].ToString();
                    updatemodel.BranchCode = dr["user_branch"].ToString();
                    updatemodel.user_name = dr["user_log"].ToString();
                    updatemodel.name = dr["user_name"].ToString();
                }
                else
                {
                    dr.Close();
                }
                dr.Close();
                con.Close();
            }
            return updatemodel;
        }
        public ServiceUpdateModel getServiccedata(int id)
        {
            ServiceUpdateModel updatemodel = new ServiceUpdateModel();

            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select service_id,service_name,service_code,service_status from SERVICES  where service_id=" + id, con);
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {

                    updatemodel.service_id = dr["service_id"].ToString();
                    updatemodel.service_code = dr["service_code"].ToString();
                    updatemodel.service_name = dr["service_name"].ToString();
                    updatemodel.service_status = dr["service_status"].ToString();

                }
                else
                {
                    dr.Close();
                }
                dr.Close();
                con.Close();
            }
            return updatemodel;
        }
        public List<SelectListItem> Populatecpanelstatuses()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //using (OracleConnection con = new OracleConnection(conString))
            //{
            //    string query = "select service_id,service_status from services";

            //    using (OracleCommand cmd = new OracleCommand(query))
            //    {
            //        cmd.Connection = con;
            //        con.Open();
            //        using (OracleDataReader sdr = cmd.ExecuteReader())
            //        {
            //            while (sdr.Read())
            //            {
            //                items.Add(new SelectListItem
            //                {
            //                    Text = sdr["service_status"].ToString(),
            //                    Value = sdr["service_id"].ToString(),
            //                });
            //            }
            //        }
            //        con.Close();
            //    }
            //}
            items.Add(new SelectListItem
            {
                Text = "A",
                Value = "1",
            });
            items.Add(new SelectListItem
            {
                Text = "DE",
                Value = "2",
            });
            return items;
        }

        public List<UsersChartsModel> GetChartsData(string branchcode)
        {
            List<UsersChartsModel> list = new List<UsersChartsModel>();
            List<float> numberslist = new List<float>();
            float sum = 0; int count = 0;
            using (OracleConnection con = new OracleConnection(conString))
            {
                //string query = "select cat_name,count(users.user_id) as users from users inner join category on users.catogry = category.cat_id inner join security_master on SUBSTR(users.account,3,3) = '" + branchcode + "' group by cat_name";
                //temp query
                string query = "select cat_name,count(users.user_id) as users from users inner join category on users.catogry = category.cat_id group by cat_name";

                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            list.Add(new UsersChartsModel
                            {
                                category = sdr["cat_name"].ToString(),
                                userscount = int.Parse(sdr["users"].ToString()),
                            });
                            numberslist.Add(int.Parse(sdr["users"].ToString()));
                        }
                    }
                    con.Close();
                }
                foreach (var item in numberslist)
                {
                    sum = sum + item;
                }

                for (int i = 0; i < numberslist.Count; i++)
                {
                    numberslist[i] = numberslist[i] * 100 / sum;
                }

                foreach (var item in list)
                {
                    item.userscount = numberslist[count];
                    count++;
                }
            }
            return list;
        }

        public List<TransactionsDetailsModel> GetTransactionsDetails(string branchcode)
        {
            List<TransactionsDetailsModel> result = new List<TransactionsDetailsModel>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                //string query = "select distinct trans_log.tran_name as transaction_name,count(trans_log.tran_name) as transactions_count from trans_log inner join users on trans_log.user_id = users.user_id where SUBSTR(users.def_acc,3,3) = '" + branchcode + "' group by tran_name";
                //temp query 
                string query = "select distinct trans_log.tran_name as transaction_name,count(trans_log.tran_name) as transactions_count from trans_log inner join users on trans_log.user_id = users.user_id group by tran_name";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            result.Add(new TransactionsDetailsModel
                            {
                                transactiontype = sdr["Transaction_name"].ToString(),
                                transactioncount = int.Parse(sdr["Transactions_count"].ToString())
                            });
                        }
                    }
                    con.Close();
                }
            }
            return result;
        }

        public List<TransactionStatusesModel> GetTransactionStatusesDetails(string branchcode)
        {
            List<TransactionStatusesModel> result = new List<TransactionStatusesModel>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                //string query = "select tran_status as status,count(tran_status) as statuscount from trans_log inner join users on trans_log.user_id = users.user_id where SUBSTR(users.def_acc,3,3) = '" + branchcode + "' group by tran_status";
                //temp query 
                string query = "select tran_status as status,count(tran_status) as statuscount from trans_log inner join users on trans_log.user_id = users.user_id group by tran_status";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            result.Add(new TransactionStatusesModel
                            {
                                status = sdr["status"].ToString(),
                                count = int.Parse(sdr["statuscount"].ToString())
                            });
                        }
                    }
                    con.Close();
                }
            }
            return result;
        }

        public List<int> GetOnlineOfflineUsers(string branchcode)
        {
            string query;
            List<int> userslist = new List<int>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                if (branchcode != "000")
                {
                    query = "select count(users.user_id) as users from users inner join security_master on SUBSTR(users.account,3,3) = security_master.user_branch where security_master.user_branch = '" + branchcode + "' and users.login_status = 1 union all select count(users.user_id) as users from users inner join security_master on SUBSTR(users.account,3,3) = security_master.user_branch where security_master.user_branch = '" + branchcode + "' and users.login_status = 1 and users.catogry = 1 union all select count(users.user_id) as users from users inner join security_master on SUBSTR(users.account,3,3) = security_master.user_branch where security_master.user_branch = '" + branchcode + "' and users.login_status = 1 and users.catogry = 2 union all select count(users.user_id) as users from users inner join security_master on SUBSTR(users.account,3,3) = security_master.user_branch where security_master.user_branch = '" + branchcode + "' and users.login_status = 1 and users.catogry = 3 union all select count(users.user_id) as users from users inner join security_master on SUBSTR(users.account,3,3) = security_master.user_branch where security_master.user_branch = '" + branchcode + "' and users.login_status = 0 union all select count(users.user_id) as users from users inner join security_master on SUBSTR(users.account,3,3) = security_master.user_branch where security_master.user_branch = '" + branchcode + "' and users.login_status = 0 and users.catogry = 1 union all select count(users.user_id) as users from users inner join security_master on SUBSTR(users.account,3,3) = security_master.user_branch where security_master.user_branch = '" + branchcode + "' and users.login_status = 0 and users.catogry = 2 union all select count(users.user_id) as users from users inner join security_master on SUBSTR(users.account,3,3) = security_master.user_branch where security_master.user_branch = '" + branchcode + "' and users.login_status = 0 and users.catogry = 3";
                }
                else
                {
                    query = "select count(users.user_id) as users from users where users.login_status = 1 union all select count(users.user_id) as users from users where  users.login_status = 1 and users.catogry = 1 union all select count(users.user_id) as users from users where  users.login_status = 1 and users.catogry = 2 union all select count(users.user_id) as users from users where  users.login_status = 1 and users.catogry = 3 union all select count(users.user_id) as users from users where  users.login_status = 0 union all select count(users.user_id) as users from users where  users.login_status = 0 and users.catogry = 1 union all select count(users.user_id) as users from users where  users.login_status = 0 and users.catogry = 2 union all select count(users.user_id) as users from users where users.login_status = 0 and users.catogry = 3";
                }

                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            userslist.Add(int.Parse(sdr["users"].ToString()));
                        }
                    }
                    con.Close();
                }
            }
            return userslist;
        }

        //-----------------Get User with Branch, Category and Status
        public List<Custreport> GetBranchUsers(string branch, string category, string status)
        {
            String sqlbranch = "", sqlstatus = "", sqlcategory = "";
            List<Custreport> users = new List<Custreport>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                if (branch != "0")
                    sqlbranch = "  and   substr(def_acc,3,3)='" + branch + "'";

                if (category != "0")
                    sqlcategory = "  and catogry = '" + category + "'";

                if (status != "0")
                    sqlstatus = "  and user_status = '" + status + "'";
                string query = "select user_name,b.branch_name||'-'||t.act_name||'-'||c.curr_name||'-'||SUBSTR(def_acc,14),status_name from  users,branchs b ,act_types t , currency c,customerstatus where  c.CURR_STS='1' and     SUBSTR(def_acc,3,3)=b.branch_code and   SUBSTR(def_acc,6,5)=t.act_type_code and SUBSTR(def_acc,11,3)=c.CURR_CODE  and user_status=status_code   " +
           " " + sqlbranch + sqlcategory + sqlstatus;

                OracleCommand cmd = new OracleCommand(query, con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }


                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {

                        users.Add(new Custreport
                        {
                            CustomerName = dr[0].ToString(),
                            AccountNumber = dr[1].ToString(),
                            //user_id = Convert.ToInt32(dr[1].ToString()),
                            CustStatus = dr[2].ToString(),


                        });
                    }
                }


            }
            return users;
        }
        public List<userlist> GetAllusers()
        {
            List<userlist> users = new List<userlist>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                //OracleCommand cmd = new OracleCommand("SELECT user_name, user_id,r.name,b.branch_name   FROM security_master u , branchs b ,cpanel_rolemaster r where u.roleid=r.roleid and u.user_branch=b.branch_code and u.user_stat = 'A'", con);
                //OracleCommand cmd = new OracleCommand("SELECT user_name, user_id, r.name, b.branch_name, decode(user_stat, 'A', 'Active', 'DE', 'Deleted', 'D', 'Deactive') as user_status FROM security_master u, branchs b, cpanel_rolemaster r where u.roleid = r.roleid and u.user_branch = b.branch_code", con);
                OracleCommand cmd = new OracleCommand("SELECT user_name, user_id,r.name,b.branch_name,decode(user_stat,'A','Active','DE','Deleted','D','Deactive') as user_status FROM security_master u, branchs b, cpanel_rolemaster r where u.roleid = r.roleid and u.user_stat = 'A'", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }


                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {


                        users.Add(new userlist
                        {
                            name = dr[0].ToString(),
                            user_id = Convert.ToInt32(dr[1].ToString()),
                            user_branch = dr[3].ToString(),
                            rolename = dr[2].ToString()

                        });
                    }
                }


            }
            return users;
        }

        public List<profilelist> GetAllProfiles()
        {
            List<profilelist> profiles = new List<profilelist>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select cpanel_rolemaster.roleid,cpanel_rolemaster.name,cpanel_rolemaster.inserted_date,count(security_master.user_log)as usercount from cpanel_rolemaster left outer join security_master on cpanel_rolemaster.roleid = security_master.roleid group by cpanel_rolemaster.roleid,cpanel_rolemaster.name,cpanel_rolemaster.inserted_date", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }


                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {


                        profiles.Add(new profilelist
                        {
                            name = dr[1].ToString(),
                            role_id = Convert.ToInt32(dr[0].ToString()),
                            inserted_date = dr[3].ToString(),
                            users_count = dr[2].ToString()

                        });
                    }
                }


            }
            return profiles;
        }

        public List<profilelist> GetAllCustomerProfiles()
        {
            List<profilelist> profiles = new List<profilelist>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select tbl_rolemaster.name,tbl_rolemaster.roleid,count(users.user_log)as usercount from tbl_rolemaster left outer join users on tbl_rolemaster.roleid = users.roleid group by tbl_rolemaster.roleid,tbl_rolemaster.name", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }


                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {


                        profiles.Add(new profilelist
                        {
                            name = dr[0].ToString(),
                            role_id = Convert.ToInt32(dr[1].ToString()),
                            //inserted_date = dr[3].ToString(),
                            users_count = dr[2].ToString()

                        });
                    }
                }


            }
            return profiles;
        }

        public int getcpanelprofileuserscount(int roleid)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select count(user_log) as userscount from security_master where roleid = '" + roleid + "'", con);
                int count = 0;
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                    OracleDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        count = int.Parse(dr[0].ToString());
                    }
                }
                return count;
            }
        }

        public List<SelectListItem> PopulateBranchs()
        {
            string query;
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["Constring"].ConnectionString;
            using (OracleConnection con = new OracleConnection(conString))
            {
                query = " select branch_code,branch_name from branchs where branch_sts = '1'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {

                            items.Add(new SelectListItem
                            {
                                Text = "All Branchs",
                                Value = "000",
                            });
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["branch_name"].ToString(),
                                    Value = sdr["branch_code"].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }

        public List<CustomerTransferReportViewModel> TotalTransactionsAmountsPerBranch(string branch_code)
        {
            string sqlbranch = "";
            if (branch_code != "000")
                sqlbranch = " where substr(users.def_acc,3,3) = '"+branch_code+"' ";

            List<CustomerTransferReportViewModel> transactions = new List<CustomerTransferReportViewModel>();

            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select tran_name as transaction_type,count(tran_name) as count,sum(regexp_substr(tran_amount,'[^SDG]+', 1, level)) as amount from trans_log inner join users on users.user_id = trans_log.user_id "+sqlbranch+" group by tran_name connect by regexp_substr(tran_amount, '[^SDG]+', 1, level) is not null", con);

                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    transactions.Add(new CustomerTransferReportViewModel
                    {
                        TranResult = dr["transaction_type"].ToString(),
                        CurrencyCode = dr["count"].ToString(),
                        TranReqAmount = dr["amount"].ToString()
                    });
                }
                dr.Close();
                con.Close();
            }
            return transactions;
        }

        public List<CustomerTransferReportViewModel> GetTransactionPerBranch(string transaction_name)
        {
            string sqltransactionname = "";
            if (transaction_name != "All")
                sqltransactionname = " and tran_name = '" + transaction_name + "' ";

            List<CustomerTransferReportViewModel> transactions = new List<CustomerTransferReportViewModel>();

            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select branch_name,count(tran_amount) as count,sum(regexp_substr(tran_amount,'[^SDG]+', 1, level)) as amount from branchs,trans_log inner join users on users.user_id = trans_log.user_id where substr(users.account,3,3) = branch_code " + sqltransactionname + " group by branch_name connect by regexp_substr(tran_amount, '[^SDG]+', 1, level) is not null", con);
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    transactions.Add(new CustomerTransferReportViewModel
                    {
                        TranResult = dr["branch_name"].ToString(),
                        CurrencyCode = dr["count"].ToString(),
                        TranReqAmount = dr["amount"].ToString()
                    });
                }
                dr.Close();
                con.Close();
            }
            return transactions;
        }

        public List<CustomerTransferReportViewModel> GetAllTransactions()
        {
            List<CustomerTransferReportViewModel> transactions = new List<CustomerTransferReportViewModel>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select * from trans_log order by tran_resp_date desc", con);
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    transactions.Add(new CustomerTransferReportViewModel
                    {
                        TranFullReq = dr["tran_req"].ToString(),
                        TranStatus = dr["tran_status"].ToString(),
                        TranName = dr["tran_name"].ToString(),
                        TranReqAmount = dr["tran_amount"].ToString(),
                        TranDate = dr["tran_resp_date"].ToString(),
                        TranToken = dr["tran_token"].ToString()
                    });
                }
                dr.Close();
                con.Close();
            }
            return transactions;
        }

        public List<CustomerTransferReportViewModel> GetAllTransactions(string branch, string status, string transaction_name)
        {
            string sqlbranch = "", sqlstatus = "", sqlname = "";

            if (branch != "000")
                sqlbranch = " and substr(users.def_acc,3,3) = '" + branch + "' ";
            if (status != "All")
                sqlstatus = " and tran_status = '"+status+"' ";
            if (transaction_name != "All")
                sqlname = " and tran_name = '"+transaction_name+"' ";

            List<CustomerTransferReportViewModel> transactions = new List<CustomerTransferReportViewModel>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select * from trans_log inner join users on trans_log.user_id = users.user_id where users.user_id > 0  " +sqlbranch+ "  "+sqlstatus+"  "+sqlname+"  order by tran_resp_date desc", con);
                
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    transactions.Add(new CustomerTransferReportViewModel
                    {
                        TranFullReq = dr["tran_req"].ToString(),
                        TranStatus = dr["tran_status"].ToString(),
                        TranName = dr["tran_name"].ToString(),
                        TranReqAmount = dr["tran_amount"].ToString(),
                        TranDate = dr["tran_resp_date"].ToString(),
                        TranToken = dr["tran_token"].ToString()
                    });
                }
                dr.Close();
                con.Close();
            }
            return transactions;
        }

        public List<SelectListItem> populatetransactionsnames()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select distinct tran_name from trans_log";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {

                            items.Add(new SelectListItem
                            {
                                Text = "All",
                                Value = "All",
                            });
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["tran_name"].ToString(),
                                    Value = sdr["tran_name"].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        public List<SelectListItem> populatetransactionsstatuses()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select distinct tran_status from trans_log";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {

                            items.Add(new SelectListItem
                            {
                                Text = "All",
                                Value = "All",
                            });
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["tran_status"].ToString(),
                                    Value = sdr["tran_status"].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        public List<SelectListItem> populateadmins()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select * from security_master";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {

                            items.Add(new SelectListItem
                            {
                                Text = "All",
                                Value = "All",
                            });
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["user_name"].ToString(),
                                    Value = sdr["user_log"].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        public List<CustomerReportModel> GetCustomersByAdmin(string admin)
        {
            OracleCommand cmd;
            OracleDataReader dr;
            String query1;
            string sqladmin = "";

            if (admin != "All")
                sqladmin = " where created_by = '" + admin + "' ";

            List<CustomerReportModel> customers = new List<CustomerReportModel>();
            query1 = "select user_name,user_log,user_email,user_mobile,user_adrs,decode(user_status,'A','Active','U','Authorized','P','Pending','D','Deactive','B','Blocked','DE','Deleted') as status,def_acc,created_by from users " + sqladmin + " ";

            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        customers.Add(new CustomerReportModel
                        {
                            CustomerName = dr[0].ToString(),
                            CustomerLog = dr[1].ToString(),
                            Email = dr[2].ToString(),
                            mobile = dr[3].ToString(),
                            address = dr[4].ToString(),
                            CustStatus = dr[5].ToString(),
                            AccountNumber = dr[6].ToString(),
                            created_by = dr[7].ToString()
                        });
                    }
                }
            }
            return customers;
        }

        public List<CustomerTransferReportViewModel> GetCreditAPITransaction()
        {
            List<CustomerTransferReportViewModel> transactions = new List<CustomerTransferReportViewModel>();

            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select * from trans_log where tran_name = 'AccountToCardTransfer'", con);
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    transactions.Add(new CustomerTransferReportViewModel
                    {
                        TranFullReq = dr["tran_req"].ToString(),
                        TranFullResp = dr["tran_resp"].ToString(),
                        TranDate = dr["tran_resp_date"].ToString(),
                        TranStatus = dr["tran_status"].ToString(),
                        TranResult = dr["tran_resp_result"].ToString(),
                        TranReqAmount = dr["tran_amount"].ToString()
                    });
                }
                dr.Close();
                con.Close();
            }
            return transactions;
        }

        public List<CustomerTransferReportViewModel> DateFilteredGetCreditAPITransaction(string fromdate, string todate)
        {
            List<CustomerTransferReportViewModel> transactions = new List<CustomerTransferReportViewModel>();

            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select * from trans_log where tran_name = 'AccountToCardTransfer' and to_date(substr(TRAN_RESP_DATE,0,9),'dd-mon-yy') >= to_date('" + fromdate + "','mm/dd/yyyy') and to_date(substr(TRAN_RESP_DATE,0,9),'dd-mon-yy') <= to_date('" + todate + "','mm/dd/yyyy')", con);
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    transactions.Add(new CustomerTransferReportViewModel
                    {
                        TranFullReq = dr["tran_req"].ToString(),
                        TranFullResp = dr["tran_resp"].ToString(),
                        TranDate = dr["tran_resp_date"].ToString(),
                        TranStatus = dr["tran_status"].ToString(),
                        TranResult = dr["tran_resp_result"].ToString(),
                        TranReqAmount = dr["tran_amount"].ToString()
                    });
                }

                dr.Close();
                con.Close();
            }
            return transactions;
        }

        public int deletecpanelprofile(int roleid)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("delete from cpanel_rolemaster where roleid = '" + roleid + "'", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }

                return cmd.ExecuteNonQuery();
            }
        }

        public int resetpassworduser(int user_id)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("Update security_master set  USER_LASTLOGIN='F',USER_PAS='admin123',USER_STAT ='A', USER_PASS='R6K2zyfxJqbwmXqixfkRMw==' where  user_id='" + user_id + "'", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }

                return cmd.ExecuteNonQuery();
            }
        }
        public int deleteuser(int user_id)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("Update security_master set USER_STAT ='DE' where  user_id='" + user_id + "'", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }

                return cmd.ExecuteNonQuery();
            }
        }
        public int deleteservice(int service_id)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("Update SERVICES  set service_status ='DE' where  service_id='" + service_id + "'", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }

                return cmd.ExecuteNonQuery();
            }
        }
        public List<ChqRequest> Chqrequest(String bracode)
        {
            OracleCommand cmd;
            OracleDataReader dr;

            int requestid;
            String name, act, date, booksize, reqdate;
            String query1, result;
            List<ChqRequest> customer = new List<ChqRequest>();
            if (!bracode.Equals("000"))
            {
                query1 = "select c.request_id,branch_name||'-'||curr_name||'-'||act_name||'-'|| SUBSTR(c.account_no,14) account_no,c.requested_size,c.req_date,u.user_name from cheque_reqs c,users u,branchs, currency,act_types where req_status='process' and u.user_id=c.user_id and   SUBSTR(c.account_no,3,3)='" + bracode + "' and branchs.branch_code=SUBSTR(c.account_no,3,3) and act_types.act_type_code=SUBSTR(c.account_no,6,5) and  currency.CURR_STS='1' and  currency.curr_code=SUBSTR(c.account_no,11,3) order by c.request_id";
            }
            else
            {
                query1 = "select c.request_id,branch_name||'-'||curr_name||'-'||act_name||'-'|| SUBSTR(c.account_no,14) account_no,c.requested_size,c.req_date,u.user_name from cheque_reqs c,users u,branchs, currency,act_types where req_status='process' and u.user_id=c.user_id  and branchs.branch_code=SUBSTR(c.account_no,3,3) and act_types.act_type_code=SUBSTR(c.account_no,6,5) and currency.CURR_STS='1' and  currency.curr_code=SUBSTR(c.account_no,11,3)  order by c.request_id";
            }

            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        requestid = Convert.ToInt32(dr[0].ToString());

                        act = dr[1].ToString();
                        booksize = dr[2].ToString();
                        date = dr[3].ToString();

                        name = dr[4].ToString();


                        customer.Add(new ChqRequest
                        {
                            request_id = requestid,
                            accountmap = act,
                            booksize = booksize,
                            name = name,
                            date = date
                        });
                    }
                }


            }


            return customer;

        }

        public List<ChqRequest> ChqrequestReport(String bracode)
        {
            OracleCommand cmd;
            OracleDataReader dr;

            int requestid;
            String name, act, date, booksize, reqdate, status;
            String query1, result;
            List<ChqRequest> customer = new List<ChqRequest>();
            if (!bracode.Equals("000"))
            {
                query1 = "select users.user_name||' - '||SUBSTR(cheque_reqs.account_no,14,7) as customer,cheque_reqs.requested_size,cheque_reqs.req_date,cheque_reqs.req_status from cheque_reqs inner join users on users.user_id = cheque_reqs.user_id where cheque_reqs.req_status <> 'process' and SUBSTR(cheque_reqs.account_no,3,3) = '" + bracode + "'";
            }
            else
            {
                query1 = "select users.user_name||' - '||SUBSTR(cheque_reqs.account_no,14,7) as customer,cheque_reqs.requested_size,cheque_reqs.req_date,cheque_reqs.req_status from cheque_reqs inner join users on users.user_id = cheque_reqs.user_id where cheque_reqs.req_status <> 'process'";
            }

            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        name = dr[0].ToString();
                        booksize = dr[1].ToString();
                        date = dr[2].ToString();
                        status = dr[3].ToString();

                        customer.Add(new ChqRequest
                        {
                            accountmap = name,
                            booksize = booksize,
                            name = name,
                            date = date,
                            status = status
                        });
                    }
                }
            }
            return customer;

        }


        public Boolean checkadminusernameavailability(string username)
        {
            Boolean result = true;
            int count = 1;

            using (OracleConnection con = new OracleConnection(conString))
            {

                string query = "select count(user_log) as count from security_master where user_log = '" + username + "'";

                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            count = int.Parse(sdr["count"].ToString());
                        }
                    }
                    con.Close();
                }
            }

            if (count > 0)
            {
                result = false;
            }
            return result;
        }

        public List<AtmCardModel> GetCardsRequests(String bracode)
        {
            OracleCommand cmd;
            OracleDataReader dr;
            int requestid;
            String name, act, date, booksize, reqdate, nameoncard, reason;
            String query1, result;
            List<AtmCardModel> card = new List<AtmCardModel>();
            if (!bracode.Equals("000"))
            {
                //query1 = "select c.request_id,branch_name||'-'||curr_name||'-'||act_name||'-'|| SUBSTR(c.account_no,14) account_no,c.requested_size,c.req_date,u.user_name from cheque_reqs c,users u,branchs, currency,act_types where req_status='process' and u.user_id=c.user_id and   SUBSTR(c.account_no,3,3)='" + bracode + "' and branchs.branch_code=SUBSTR(c.account_no,3,3) and act_types.act_type_code=SUBSTR(c.account_no,6,5) and  currency.CURR_STS='1' and  currency.curr_code=SUBSTR(c.account_no,11,3) order by c.request_id";
                query1 = "select c.request_id,branch_name||'-'||curr_name||'-'||act_name||'-'|| SUBSTR(c.account_no,14) account_no,c.name_on_card,c.req_date,c.req_reason,u.user_name from card_reqs c,users u,branchs, currency,act_types where req_status='process' and u.user_id=c.user_id and branchs.branch_code=SUBSTR(c.account_no,3,3) and act_types.act_type_code=SUBSTR(c.account_no,6,5) and currency.CURR_STS='1' and currency.curr_code=SUBSTR(c.account_no,11,3) and SUBSTR(c.account_no,3,3)='" + bracode + "' order by c.request_id";
            }
            else
            {
                query1 = "select c.request_id,branch_name||'-'||curr_name||'-'||act_name||'-'|| SUBSTR(c.account_no,14) account_no,c.name_on_card,c.req_date,c.req_reason,u.user_name from card_reqs c,users u,branchs, currency,act_types where req_status='process' and u.user_id=c.user_id and branchs.branch_code=SUBSTR(c.account_no,3,3) and act_types.act_type_code=SUBSTR(c.account_no,6,5) and currency.CURR_STS='1' and currency.curr_code=SUBSTR(c.account_no,11,3) order by c.request_id";
            }
            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        requestid = Convert.ToInt32(dr[0].ToString());
                        act = dr[1].ToString();
                        nameoncard = dr[2].ToString();
                        date = dr[3].ToString();
                        reason = dr[4].ToString();
                        name = dr[5].ToString();


                        card.Add(new AtmCardModel
                        {
                            request_id = requestid.ToString(),
                            account_number = act,
                            name = name,
                            request_date = date,
                            name_on_card = nameoncard,
                            request_reason = reason
                        });
                    }
                }
            }
            return card;
        }

        public List<AtmCardModel> AtmCardsReport(String bracode)
        {
            OracleCommand cmd;
            OracleDataReader dr;
            int requestid;
            String name, act, date, booksize, reqdate, nameoncard, reason, status;
            String query1, result;
            List<AtmCardModel> card = new List<AtmCardModel>();
            if (!bracode.Equals("000"))
            {
                //query1 = "select c.request_id,branch_name||'-'||curr_name||'-'||act_name||'-'|| SUBSTR(c.account_no,14) account_no,c.requested_size,c.req_date,u.user_name from cheque_reqs c,users u,branchs, currency,act_types where req_status='process' and u.user_id=c.user_id and   SUBSTR(c.account_no,3,3)='" + bracode + "' and branchs.branch_code=SUBSTR(c.account_no,3,3) and act_types.act_type_code=SUBSTR(c.account_no,6,5) and  currency.CURR_STS='1' and  currency.curr_code=SUBSTR(c.account_no,11,3) order by c.request_id";
                query1 = "select users.user_name||' - '||SUBSTR(card_reqs.account_no,14,7) as customer,card_reqs.req_date,card_reqs.req_status,card_reqs.req_reason,card_reqs.name_on_card from card_reqs inner join users on users.user_id = card_reqs.user_id where card_reqs.req_status <> 'process' and SUBSTR(card_reqs.account_no,3,3) = '" + bracode + "'";
            }
            else
            {
                query1 = "select users.user_name||' - '||SUBSTR(card_reqs.account_no,14,7) as customer,card_reqs.req_date,card_reqs.req_status,card_reqs.req_reason,card_reqs.name_on_card from card_reqs inner join users on users.user_id = card_reqs.user_id where card_reqs.req_status <> 'process'";
            }
            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        name = dr[0].ToString();
                        date = dr[1].ToString();
                        status = dr[2].ToString();
                        reason = dr[3].ToString();
                        nameoncard = dr[4].ToString();

                        card.Add(new AtmCardModel
                        {
                            name = name,
                            request_date = date,
                            name_on_card = nameoncard,
                            request_reason = reason,
                            request_status = status
                        });
                    }
                }
            }
            return card;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public String getaccount(string user_id)
        {
            String Accounts = "";
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " SELECT acc_id,acc_no from user_acc_link where user_id =" + user_id;

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();


                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {

                        if (dataReader["acc_id"] != DBNull.Value)
                        {

                            if (dataReader["acc_no"] != DBNull.Value)
                            {
                                Accounts = Accounts + "-" + (string)dataReader["acc_no"];
                                //Accounts = Accounts.Substring(2);
                            }

                        }
                    }
                    Accounts = Accounts.Substring(1);
                    return Accounts;

                }

            }

        }
        //---------------------------------------------------------get act --------------------------------//
        public String getspfaccount(string user_id, string act)
        {
            String Accounts = "";
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " SELECT acc_id,acc_no from user_acc_link where user_id =" + user_id + " and substr(acc_no,14)=" + act;

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();


                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {

                        if (dataReader["acc_id"] != DBNull.Value)
                        {

                            if (dataReader["acc_no"] != DBNull.Value)
                            {
                                Accounts = (string)dataReader["acc_no"];
                                //Accounts = Accounts.Substring(2);
                            }

                        }
                    }

                    return Accounts;

                }

            }

        }

        //-----------------------DropDownGET Branchs------------------------------------------------------
        //
        public List<CustomerRegBankinfo> GetBranchs()
        {
            string branchs = "";
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select branch_code,branch_name from branchs where branch_sts = '1'";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();

                List<CustomerRegBankinfo> list = new List<CustomerRegBankinfo>();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        CustomerRegBankinfo obj = new CustomerRegBankinfo();

                        if (dataReader["branch_code"] != DBNull.Value)
                        {
                            obj.BranchCode = (string)dataReader["branch_code"];

                            if (dataReader["branch_name"] != DBNull.Value)
                            {
                                obj.Branch = (string)dataReader["branch_name"];

                            }

                            list.Add(obj);

                        }
                    }
                    //branchs = branchs.Substring(1);
                    return list;
                }
            }
        }

        //----------------------DropDownGet Account Type---------------------------------
        public List<CustomerRegBankinfo> GetAccountType()
        {
            string branchs = "";
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select act_type_code,act_name from Act_types";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();

                List<CustomerRegBankinfo> list = new List<CustomerRegBankinfo>();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        CustomerRegBankinfo obj = new CustomerRegBankinfo();

                        if (dataReader["act_type_code"] != DBNull.Value)
                        {
                            obj.AccountTypecode = (string)dataReader["act_type_code"];

                            if (dataReader["act_name"] != DBNull.Value)
                            {
                                obj.AccountType = (string)dataReader["act_name"];

                            }

                            list.Add(obj);

                        }
                    }
                    //branchs = branchs.Substring(1);
                    return list;
                }
            }
        }

        //------------------DropDown Get Currency---------------------------
        public List<CustomerRegBankinfo> GetCurrency()
        {
            string branchs = "";
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select curr_code,curr_name from currency where CURR_STS='1' ";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();

                List<CustomerRegBankinfo> list = new List<CustomerRegBankinfo>();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        CustomerRegBankinfo obj = new CustomerRegBankinfo();

                        if (dataReader["curr_code"] != DBNull.Value)
                        {
                            obj.CurrencyCode = (string)dataReader["curr_code"];

                            if (dataReader["curr_name"] != DBNull.Value)
                            {
                                obj.Currency = (string)dataReader["curr_name"];

                            }

                            list.Add(obj);

                        }
                    }
                    //branchs = branchs.Substring(1);
                    return list;
                }
            }
        }


        //-----------------------GET AccountTypes------------------------------------------------------
        //
        public String getaccounttype(string acctype)
        {
            String acctypename = "NULL";
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select act_name from act_types where act_type_code=" + acctype;
                string query2 = "select act_name from invact_types where act_type_code=" + acctype;



                OracleCommand cmd = new OracleCommand(query, con);
                OracleCommand cmd2 = new OracleCommand(query2, con);
                con.Open();


                OracleDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        if (dataReader["act_name"] != DBNull.Value)
                        {
                            acctypename = (string)dataReader["act_name"];
                        }

                    }
                }
                else
                {
                    dataReader = cmd2.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            if (dataReader["act_name"] != DBNull.Value)
                            {
                                acctypename = (string)dataReader["act_name"];
                            }

                        }
                    }
                    else
                    { acctypename = "Account Type Not Found"; }
                }



                return acctypename;

            }

        }


        //-----------------------GET BRANCH NAME English------------------------------------------------------
        //
        public String getbranchnameenglish(string brcode)
        {
            String brname = "NULL";
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select  branch_name from branchs where branch_sts='1' and branch_code=" + brcode;

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();


                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {



                        if (dataReader["branch_name"] != DBNull.Value)
                        {
                            brname = (string)dataReader["branch_name"];
                        }

                    }
                }
                if (brcode == "000")
                {
                    return "Admin";
                }
                return brname;

            }

        }

        public String getcurrencyname(string currcode)
        {
            String curr_name = "NULL";
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select  curr_name from currency where CURR_STS='1' and   curr_code=" + currcode;

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();


                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {



                        if (dataReader["curr_name"] != DBNull.Value)
                        {
                            curr_name = (string)dataReader["curr_name"];
                        }

                    }
                }

                return curr_name;

            }

        }



        //-------------------------------------DropClient for ChequeStatus Controller DropDownList--------------------------------
        //
        public List<CustomerRegBankinfo> DropClient(string user_id)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " SELECT acc_id,acc_no from user_acc_link where user_id =" + user_id;

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();

                List<CustomerRegBankinfo> list = new List<CustomerRegBankinfo>();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        CustomerRegBankinfo obj = new CustomerRegBankinfo();
                        if (dataReader["acc_id"] != DBNull.Value)
                        {
                            if (dataReader["acc_id"] != DBNull.Value)
                            {
                                //obj.AccountID = (int)dataReader["acc_id"];
                                obj.CustomerID = dataReader["acc_id"].ToString();
                            }
                            if (dataReader["acc_no"] != DBNull.Value)
                            {
                                obj.AccountNumber = (string)dataReader["acc_no"];
                            }
                            list.Add(obj);
                        }
                    }

                    return list;

                }

            }

        }


        public List<CustomerRegBankinfo> checkaccount(string act)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select user_id,user_name from users where DEF_ACC='" + act + "'";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();

                List<CustomerRegBankinfo> list = new List<CustomerRegBankinfo>();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        CustomerRegBankinfo obj = new CustomerRegBankinfo();
                        if (dataReader["user_id"] != DBNull.Value)
                        {
                            if (dataReader["user_id"] != DBNull.Value)
                            {
                                //obj.AccountID = (int)dataReader["acc_id"];
                                obj.CustomerID = dataReader["user_id"].ToString();
                            }
                            if (dataReader["user_name"] != DBNull.Value)
                            {
                                obj.CustomerName = (string)dataReader["user_name"];
                            }
                            list.Add(obj);
                        }
                    }

                    return list;

                }

            }

        }


        //---------------------test pr--------------------------------------------------------------//
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="FILE_NAME"></param>
        /// <returns></returns>
        public int insertfilesalary(string user_id, string FILE_NAME)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "INSERT INTO salary_files (FILE_ID,FILE_NAME,NO_OF_ROWS,STATUS,NO_OF_PROCESS_ROWS,FILE_DATE,USER_ID,FILE_TOTAL) " +
                               "VALUES(salaryfile.nextval,'" + FILE_NAME + "','0','P','0',sysdate," + user_id + ",'0')";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();
                int result = -1;
                result = cmd.ExecuteNonQuery();



                return result;

            }



        }

        //----------------------- INSERT FiLE SALARY ITEMS----------------------------------------------
        //
        public int insertfilesalaryitems(string user_id, string FILE_NAME, string acc, string amount, string acccomp)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "INSERT INTO salary_temp (SALARY_ID,SALARY_USER_ID,SALARY_ACCOUNT_NO,SALARY_AMOUNT,SALARY_STATUS,SALARY_FILE_NAME,SALARY_COMP_ACT,SALARY_PROCESS_DATE,SALARY_REQ_DATE)" +
                               "VALUES(salarytemp.nextval," + user_id + ",'" + acc + "','" + amount + "','P','" + FILE_NAME + "','" + acccomp + "',sysdate,sysdate)";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();
                int result = -1;
                result = cmd.ExecuteNonQuery();



                return result;

            }



        }

        //----------------------- update FiLE SALARY ITEMS----------------------------------------------
        //
        public int updatesalaryitems(string user_id, string FILE_NAME, string acc, string sts)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "update salary_temp  set SALARY_STATUS ='" + sts + "', SALARY_PROCESS_DATE=sysdate where SALARY_USER_ID=" + user_id + " and SALARY_ACCOUNT_NO='" + acc + "' and SALARY_FILE_NAME='" + FILE_NAME + "'";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();
                int result = -1;
                result = cmd.ExecuteNonQuery();



                return result;

            }



        }


        /// updates file sallary
        /// items in a table
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="fileName"></param>
        /// <param name="countrow"></param>
        /// <param name="totalamount"></param>
        /// <param name="modelAccountNumber"></param>
        /// <returns></returns>
        /// //-----------------------------------updatefilesalaryitems---------------------
        public int updatefilesalaryitems(string userId, string fileName, int countrow, double totalamount, string modelAccountNumber)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "update  salary_files  set  NO_OF_ROWS=" + countrow + ",STATUS='RWS',FILE_TOTAL=" + totalamount +
                               " where FILE_NAME='" + fileName + "' and user_id=" + userId;

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();
                int result = -1;
                result = cmd.ExecuteNonQuery();



                return result;

            }
        }


        //-----------------------------------InsertTranLog---------------------
        /// <summary>
        /// Insert into Log 
        /// all the info about each transaction
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="tranName"></param>
        /// <param name="req"></param>
        /// <param name="resp"></param>
        /// <param name="status"></param>
        /// <param name="respResult"></param>
        /// <returns></returns>
        public int InsertTranLog(string user_id, string tranName, string req, string resp, string status, string respResult)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "INSERT INTO trans_log (TRAN_ID,TRAN_REQ,TRAN_RESP,TRAN_REQ_DATE,TRAN_RESP_DATE,TRAN_STATUS,TRAN_RESP_RESULT,USER_ID,TRAN_NAME)" +
                               "VALUES(tranlog.nextval,'" + req + "','" + resp + "',sysdate,sysdate,'" + status + "','" + respResult + "','" + user_id + "','" + tranName + "')";
                //string query = "INSERT INTO trans_log (TRAN_ID,TRAN_REQ,TRAN_RESP,TRAN_REQ_DATE,TRAN_RESP_DATE)" +
                //               "VALUES(tranlog.nextval,'" + req + "','"+ resp +"',sysdate,sysdate,sysdate)";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();
                int result = -1;

                result = cmd.ExecuteNonQuery();



                return result;

            }
        }


        public int InsertChequeReq(string user_id, string accountNo, string size)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "INSERT INTO cheque_reqs (REQUEST_ID,ACCOUNT_NO,USER_ID,REQUESTED_SIZE,REQ_DATE,REQ_STATUS,REQ_REASON) " +
                               "VALUES(cheque_req_seq.nextval,'" + accountNo + "','" + user_id + "','" + size + "',sysdate,'process', '')";


                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();
                int result = -1;

                result = cmd.ExecuteNonQuery();



                return result;

            }
        }



        public String custregcheck(String branchcode, String acttype, String acc_no, String acc_curr, String category, string subno, string subgl)
        {
            Boolean FLAG;
            String lblconfirm;
            OracleCommand cmd;
            OracleDataReader dr;
            int counter;

            String query1 = "select count(*) from users  where DEF_ACC='11" + branchcode + acttype + acc_no + subno + acc_curr + subgl + "'   and catogry ='" + category + "'";

            String query2 = "select count(*) from user_acc_link where acc_no='11" + branchcode + acttype + acc_no + subno + acc_curr + subgl + "' and catogry ='" + category + "'";
            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();

                dr = cmd.ExecuteReader();
                dr.Read();

                counter = Convert.ToInt32(dr[0].ToString());
                dr.Close();
                con.Close();
                if ((counter == 0))
                {
                    cmd = new OracleCommand(query2, con);

                    con.Open();

                    dr = cmd.ExecuteReader();
                    dr.Read();

                    counter = Convert.ToInt32(dr[0].ToString());
                    dr.Close();
                    con.Close();
                    if ((counter != 0))
                    {
                        lblconfirm = "This Account is linked with another user";

                        return lblconfirm;
                    }
                    else
                    {
                        lblconfirm = "This Account is available";
                    }
                }
                else
                {
                    lblconfirm = "This Account is Already exist";
                }

            }
            return lblconfirm;
        }


        public String custregcheck2(String Account, String category)
        {
            Boolean FLAG;
            String lblconfirm;
            OracleCommand cmd;
            OracleDataReader dr;
            int counter;
            string operatoraccount = Account + "O";
            string authorizoraccount = Account + "A";
            String query1 = "select count(*) from users where user_log = '" + Account + "' or user_log = '" + operatoraccount + "' or user_log = '" + authorizoraccount + "'";

            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();

                dr = cmd.ExecuteReader();
                dr.Read();

                counter = Convert.ToInt32(dr[0].ToString());
                dr.Close();
                con.Close();
                if ((counter == 0))
                {

                    lblconfirm = "This Account is available";

                }
                else
                {
                    lblconfirm = "This Account is Already exist";
                }

            }
            return lblconfirm;
        }


        ////////////populate List//////////////////////////////////////////////////////
        ///

        public List<SelectListItem> PopulateBranchsForAdmins()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["Constring"].ConnectionString;
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select branch_code,branch_name from branchs where branch_sts = '1'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {

                            //items.Add(new SelectListItem
                            //{
                            //    Text = "-- Select Branch --",
                            //    Value = "0",
                            //});
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["branch_name"].ToString(),
                                    Value = sdr["branch_code"].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }

        public List<SelectListItem> PopulateBranchs(string branchcode)
        {
            string query;
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["Constring"].ConnectionString;
            using (OracleConnection con = new OracleConnection(conString))
            {
                if (branchcode == "000")
                {
                    query = " select branch_code,branch_name from branchs where branch_sts = '1'";
                }
                else
                {
                    query = " select branch_code,branch_name from branchs where branch_sts = '1' and BRANCH_CODE_NO ='" + branchcode + "' ";
                }
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {

                            //items.Add(new SelectListItem
                            //{
                            //    Text = "-- Select Branch --",
                            //    Value = "0",
                            //});
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["branch_name"].ToString(),
                                    Value = sdr["branch_code"].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }

        public List<Charter> getUsersBranchsCount()
        {
            List<Charter> users = new List<Charter>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select branch_name,count(user_id) as count from users inner join branchs on substr(account,3,3) = branch_code group by branch_name", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }


                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        users.Add(new Charter
                        {
                            name = dr[0].ToString(),
                            value = dr[1].ToString()
                        });
                    }
                }
            }
            return users;
        }

        public List<Charter> getAllStatuses()
        {
            List<Charter> users = new List<Charter>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select decode(user_status,'U','Authorized','P','Pending','D','Deactivated','A','Active','B','Blocked','DE','Deleted') as status,count(user_id) as count from users group by user_status", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }


                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        users.Add(new Charter
                        {
                            name = dr[0].ToString(),
                            value = dr[1].ToString()
                        });
                    }
                }
            }
            return users;
        }

        public List<Charter> getBranchsTransactionsCount()
        {
            List<Charter> users = new List<Charter>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select branchs.branch_name,count(tm.tran_id) as count from ( select * from users inner join trans_log on users.user_id = trans_log.user_id ) tm inner join branchs on substr(account,3,3) = branch_code group by branchs.branch_name", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }


                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        users.Add(new Charter
                        {
                            name = dr[0].ToString(),
                            value = dr[1].ToString()
                        });
                    }
                }
            }
            return users;
        }

        public List<SelectListItem> PopulateCurrencies()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["Constring"].ConnectionString;
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select curr_code,curr_name from currency where CURR_STS='1'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["curr_name"].ToString(),
                                Value = sdr["curr_code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }

        public List<SelectListItem> PopulateCurrencies(string currency_code)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["Constring"].ConnectionString;
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select curr_code,curr_name from currency where CURR_STS='1' and curr_code = '" + currency_code + "'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["curr_name"].ToString(),
                                Value = sdr["curr_code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }

        public List<SelectListItem> PopulateAccountTypes()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["Constring"].ConnectionString;
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select act_type_code,act_name from Act_types";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["act_name"].ToString(),
                                Value = sdr["act_type_code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }







        internal List<SelectListItem> PopulateProfiles()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["Constring"].ConnectionString;
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select roleid,name  from TBL_ROLEMASTER where active='1'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["name"].ToString(),
                                Value = sdr["roleid"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }
        internal List<SelectListItem> PopulatecpanelProfiles(string user_branch)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "";
                if (user_branch == "000")
                {
                    query = "select roleid,name  from cpanel_rolemaster where active='1'";
                }
                else
                {
                    query = "select roleid,name  from cpanel_rolemaster where active='1' and name <> 'Admin'";
                }

                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["name"].ToString(),
                                Value = sdr["roleid"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }


        internal List<SelectListItem> PopulateProfiles(int userid)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["Constring"].ConnectionString;
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select TBL_ROLEMASTER.roleid,TBL_ROLEMASTER.name from TBL_ROLEMASTER inner join users on TBL_ROLEMASTER.ROLEID = users.roleid where TBL_ROLEMASTER.active='1' and user_id = '" + userid + "' ";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["name"].ToString(),
                                Value = sdr["roleid"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }

        internal List<SelectListItem> PopulatecpanelProfiles()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select roleid,name  from cpanel_rolemaster where active='1'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["name"].ToString(),
                                Value = sdr["roleid"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }

        public Boolean usernameavailabilitycheck(string CustomerUsername)
        {
            Boolean result = true;
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select * from users where user_log = '" + CustomerUsername + "'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            result = false;
                        }
                    }
                    con.Close();
                }
            }

            return result;
        }

        public int custreg(string CustomerID, string CustomerName, string account, string username, string address, string CustomerPhone, string email, string customerprofile, string customercatgory, string CUSTOMERSERVICE, string created_by)
        {
            int result = -1;
            if (email == null) { email = "N/A"; }

            if (customercatgory == "2")
            {
                username = username + "O";
            }
            else if (customercatgory == "3")
            {
                username = username + "A";
            }

            Random random = new Random();

            if (usernameavailabilitycheck(username))
            {
                using (OracleConnection con = new OracleConnection(conString))
                {
                    //String re = CreatePassword(8);  generating 8 random characters
                    String re = random.Next(10000000, 99999999).ToString(); // generating 8 random numbers

                    String enc_pwd = Encrypt(re);

                    //                string query = "INSERT INTO users (USER_ID,USER_NAME,USER_LOG,USER_PWD,USER_EMAIL,USER_MOBILE,USER_FAX,USER_ADRS,USER_STATUS,DEF_ACC,LAST_LOGIN,LAST_LOG_IP,FAILD_LOGINS,USER_CUSTID,FIRST_LOGIN,CATOGRY,USER_PAS,USER_TRANSFER,ROLEID,ACCOUNT,ACTIVE)" +
                    //"VALUES((select max( to_number(user_id))+1 from users),'" + CustomerName + "','" + username + "','" + enc_pwd + "','" + email + "','" + CustomerPhone + "','" + CustomerPhone + "','al-khaleejbank','P','" + account + "',sysdate,'127.0.0.1',0,'"+CustomerID+"','T','"+customercatgory+"','" + re + "','True','" + customerprofile + "','" + account + "','1')";

                    //                OracleCommand cmd = new OracleCommand(query, con);

                    //                con.Open();

                    //                result = cmd.ExecuteNonQuery();
                    OracleCommand cmd = new OracleCommand();
                    cmd.CommandText = "insertnewcustomer";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Connection = con;
                    con.Open();

                    cmd.Parameters.Add("CustomerName", OracleType.VarChar).Value = CustomerName;
                    cmd.Parameters.Add("username", OracleType.VarChar).Value = username;
                    cmd.Parameters.Add("enc_pwd", OracleType.VarChar).Value = enc_pwd;
                    cmd.Parameters.Add("email", OracleType.VarChar).Value = email;
                    cmd.Parameters.Add("CustomerPhone", OracleType.VarChar).Value = CustomerPhone;
                    cmd.Parameters.Add("useraccount", OracleType.VarChar).Value = account;
                    cmd.Parameters.Add("CustomerID", OracleType.VarChar).Value = CustomerID;
                    //cmd.Parameters.Add("CustomerPhone", OracleType.VarChar).Value = CustomerPhone;
                    //cmd.Parameters.Add("useraccount", OracleType.VarChar).Value = account;
                    cmd.Parameters.Add("customercatgory", OracleType.VarChar).Value = customercatgory;
                    cmd.Parameters.Add("re", OracleType.VarChar).Value = re;
                    cmd.Parameters.Add("customerprofile", OracleType.VarChar).Value = customerprofile;
                    cmd.Parameters.Add("CUSTOMERSERVICE", OracleType.VarChar).Value = CUSTOMERSERVICE;
                    cmd.Parameters.Add("Customeraddress", OracleType.VarChar).Value = address;
                    cmd.Parameters.Add("createdby", OracleType.VarChar).Value = created_by;
                    cmd.Parameters.Add("res", OracleType.Int32).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("errcode", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("errmsg", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    String res = cmd.Parameters["res"].Value.ToString();
                    String errormsg = cmd.Parameters["errmsg"].Value.ToString();
                    String errorcode = cmd.Parameters["errcode"].Value.ToString();
                    result = Int32.Parse(res);
                }
                return result;
            }
            else
            {
                result = 2;
                return result;
            }
        }

        public List<CustomerAuthorization> PendingCustomer(String bracode)
        {
            OracleCommand cmd;
            OracleDataReader dr;

            String userid, username, useract;
            String query1, result;
            List<CustomerAuthorization> customer = new List<CustomerAuthorization>();
            if (!bracode.Equals("000"))
            {
                query1 = "select user_id,user_name,b.branch_name||' - '||t.act_name||' - '||c.curr_name||' - '||SUBSTR(def_acc,14,7) from  users,branchs b ,act_types t , currency c where  c.CURR_STS='1' and user_status = 'P' and SUBSTR(def_acc,3,3)=b.branch_code and   SUBSTR(def_acc,6,5)=t.act_type_code and SUBSTR(def_acc,11,3)=c.CURR_CODE and substr(def_acc,3,3)='" + bracode + "'";
            }
            else
            {
                query1 = "select user_id,user_name,b.branch_name||' - '||t.act_name||' - '||c.curr_name||' - '||SUBSTR(def_acc,14,7) from  users,branchs b ,act_types t , currency c where  c.CURR_STS='1' and user_status = 'P' and SUBSTR(def_acc,3,3)=b.branch_code and   SUBSTR(def_acc,6,5)=t.act_type_code and SUBSTR(def_acc,11,3)=c.CURR_CODE";
            }

            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        userid = dr[0].ToString();
                        username = dr[1].ToString();
                        useract = dr[2].ToString();


                        customer.Add(new CustomerAuthorization
                        {
                            CustomerID = userid,
                            Customername = username,
                            Customeraccount = useract
                        });
                    }
                }


            }


            return customer;
        }

        public int insertadminslog(string userid, string username, string branch, string userrole, string userstatus, string action, string actiononuser, string timestamp)
        {
            int result = -1;

            if (usernameavailabilitycheck(username))
            {
                using (OracleConnection con = new OracleConnection(conString))
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.CommandText = "INSERTADMINSLOG";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    con.Open();

                    cmd.Parameters.Add("user_id", OracleType.VarChar).Value = userid;
                    cmd.Parameters.Add("username", OracleType.VarChar).Value = username;
                    cmd.Parameters.Add("user_role", OracleType.VarChar).Value = userrole;
                    cmd.Parameters.Add("user_status", OracleType.VarChar).Value = userstatus;
                    cmd.Parameters.Add("action", OracleType.VarChar).Value = action;
                    cmd.Parameters.Add("action_on_user", OracleType.VarChar).Value = actiononuser;
                    cmd.Parameters.Add("timedate", OracleType.VarChar).Value = timestamp;
                    cmd.Parameters.Add("user_branch", OracleType.VarChar).Value = branch;
                    OracleParameter p1 = new OracleParameter("status", OracleType.VarChar, 2000);
                    p1.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p1);

                    result = cmd.ExecuteNonQuery();
                }
                return result;
            }
            else
            {
                result = 2;
                return result;
            }
        }



        public List<CustomerAuthorizationinfo> CustomerAuthorizationinfo(String userid)
        {
            OracleCommand cmd;
            OracleDataReader dr;

            String username, useract;
            String query1, result;
            List<CustomerAuthorizationinfo> customer = new List<CustomerAuthorizationinfo>();
            OracleDataReader dr3;
            OracleCommand cmd3;
            string sqstr;
            string msg = "";
            string br, Sessioncurr = "";
            String acc = "";
            String acc_type = "";
            String acc_no;
            String curr;
            String curr_name = "";
            String lang;
            String brname = "";
            String acctype = "";
            String roleid = "", profilename = "";
            query1 = "select *  from users where user_id='" + userid + "'";
            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd3 = new OracleCommand(query1, con);

                con.Open();


                dr3 = cmd3.ExecuteReader();
                if (dr3.Read())
                {
                    // 'lb_cust_name.Text = dr3(1)
                    OracleDataReader dr2;
                    OracleDataReader dr4;
                    OracleDataReader dr5;
                    OracleDataReader dr6;
                    OracleCommand cmd2;
                    OracleCommand cmd4;
                    OracleCommand cmd5;
                    OracleCommand cmd6;


                    acc = dr3[9].ToString();
                    roleid = dr3[18].ToString();
                    br = acc.Substring(2, 3);
                    acc_type = acc.Substring(5, 5);
                    Sessioncurr = acc.Substring(10, 3);
                    acc_no = acc.Substring(13);

                    cmd4 = new OracleCommand(("select BRANCH_NAME from BRANCHS where BRANCH_CODE_NO='" + br + "'"), con);
                    dr4 = cmd4.ExecuteReader();
                    if (dr4.Read())
                    {
                        brname = dr4[0].ToString();

                    }

                    dr4.Close();
                    //cmd5 = new OracleCommand(("select act_name from act_types where act_type_code ='" + (acc_type + "'")), con);
                    //dr5 = cmd5.ExecuteReader();
                    //if (dr5.Read())
                    //{
                    //    acctype = dr5[0].ToString();

                    //}

                    //dr5.Close();
                    cmd5 = new OracleCommand(("select act_name from act_types where act_type_code ='" + (acc_type + "'")), con);
                    dr5 = cmd5.ExecuteReader();
                    if (dr5.HasRows)
                    {
                        dr5.Read();
                        acctype = dr5[0].ToString();

                    }
                    else
                    {
                        cmd5 = new OracleCommand(("select act_name from invact_types where act_type_code='" + (acc_type + "'")), con);
                        dr5 = cmd5.ExecuteReader();
                        if (dr5.HasRows)
                        {
                            dr5.Read();
                            acctype = dr5[0].ToString();

                        }
                        else
                            acctype = "Account Type Not Found";

                    }

                    dr5.Close();
                    cmd2 = new OracleCommand(("select name  from tbl_rolemaster where roleid='" + roleid + "'  "), con);
                    dr2 = cmd2.ExecuteReader();
                    if (dr2.Read())
                    {
                        profilename = dr2[0].ToString();

                    }


                    dr2.Close();
                    cmd6 = new OracleCommand(("select CURR_NAME from CURRENCY where CURR_STS='1' and  CURR_CODE = '" + Sessioncurr + "'"), con);
                    dr6 = cmd6.ExecuteReader();
                    if (dr6.Read())
                    {
                        curr_name = dr6[0].ToString();

                    }

                    dr6.Close();
                }
                customer.Add(new CustomerAuthorizationinfo
                {
                    userid = dr3[0].ToString(),
                    Branch = brname,
                    AccountType = acctype,
                    Customername = dr3[1].ToString(),
                    Currency = curr_name,
                    Customeraccount = acc.Substring(12, 7),
                    UserName = dr3[2].ToString(),
                    Address = dr3[7].ToString(),
                    CustomerPhone = dr3[5].ToString(),
                    Email = dr3[4].ToString(),
                    Profile = profilename,
                });

                dr3.Close();
            }
            return customer;

        }



        public int updatecustomer(String userid, String status)
        {
            OracleCommand cmd;
            int result = -1;


            String query1;
            query1 = "update users set USER_STATUS='" + status + "' where user_id='" + userid + "'";
            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();
                result = cmd.ExecuteNonQuery();
            }
            return result;
        }
        public int updatecustomerusingact(String account, String status)
        {
            OracleCommand cmd;
            int result = -1;


            String query1;
            query1 = "update USERS set USER_STATUS ='" + status + "',FAILD_LOGINS=0 where DEF_ACC ='" + account + "'";
            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();
                result = cmd.ExecuteNonQuery();
            }
            return result;
        }

        public List<Custreport> getbranchcustomers(string branchcode)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query;
                if (branchcode == "000")
                {
                    query = "select * from users";
                }

                query = "select * from users where SUBSTR(users.def_acc,3,3) = '" + branchcode + "'";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();

                List<Custreport> customers = new List<Custreport>();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Custreport obj = new Custreport();
                        obj.CustomerID = dataReader["USER_ID"].ToString();
                        obj.customerfullname = dataReader["USER_NAME"].ToString();
                        obj.CustomerName = dataReader["USER_LOG"].ToString();
                        obj.customeremail = dataReader["USER_EMAIL"].ToString();
                        obj.phonenumber = dataReader["USER_MOBILE"].ToString();
                        obj.address = dataReader["USER_ADRS"].ToString();
                        obj.CustStatus = dataReader["USER_STATUS"].ToString();
                        obj.AccountNumber = dataReader["DEF_ACC"].ToString();
                        obj.lastlogin = dataReader["LAST_LOGIN"].ToString();
                        obj.lastip = dataReader["LAST_LOG_IP"].ToString();
                        obj.faildlogincount = dataReader["FAILD_LOGINS"].ToString();
                        obj.category = dataReader["CATOGRY"].ToString();
                        obj.createdby = dataReader["CREATED_BY"].ToString();
                        customers.Add(obj);
                    }
                    return customers;
                }
            }
        }


        public Loginmodelresult checkuserlogin(String usrname, String password, String UserHostAddress)
        {
            Loginmodelresult model = new Loginmodelresult();
            string encpass;

            encpass = Encrypt(password);
            OracleCommand cmd;
            OracleDataReader dr;
            string Sqlstr;
            Sqlstr = "Select user_id,user_name,user_branch, USER_LASTLOGIN,roleid,user_stat from security_master where user_LOG= '"
                        + usrname + "' and user_pass= '" + encpass + "' and user_stat = 'A'";
            model.Login = false;
            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(Sqlstr, con);
                try
                {
                    con.Open();
                    dr = cmd.ExecuteReader();
                    OracleCommand cmd2;
                    OracleCommand cmd3;

                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {

                            cmd2 = new OracleCommand(("insert into Users_login values('"
                                            + (UserHostAddress + ("','"
                                            + (DateTime.Today.ToString() + ("','"
                                            + (usrname + "','-', 'S')")))))), con);
                            cmd2.ExecuteNonQuery();

                            model.UserId = dr[0].ToString();
                            model.user_name = dr[1].ToString();
                            model.user_branch = dr[2].ToString();
                            model.USER_LASTLOGIN = dr[3].ToString();
                            model.user_roleid = dr[4].ToString();
                            model.status = dr[5].ToString();

                            model.user_log = usrname;
                            model.Login = true;


                            if ((model.USER_LASTLOGIN == "F"))
                            {
                                cmd3 = new OracleCommand(("update SECURITY_MASTER set  user_pas='',user_lastlogin='T' where user_id='"
                                                + (model.UserId + "' ")), con);
                                cmd3.ExecuteNonQuery();
                                model.lblconfirm = "change_pass";
                            }
                            else
                            {
                                model.lblconfirm = "home";
                            }
                        }
                    }
                    else
                    {
                        model.lblconfirm = "There is wrong into username or password";
                        cmd2 = new OracleCommand(("insert into Users_login values('"
                                        + (UserHostAddress + ("','"
                                        + (DateTime.Today.ToString() + ("','"
                                        + (usrname + "','-', 'F')")))))), con);
                        cmd2.ExecuteNonQuery();
                        model.lblconfirm = "Wrong input username or password";
                    }

                }
                catch (Exception ex)
                {
                    model.lblconfirm = "System Error" + ex;
                }
            }
            return model;
        }

        protected string GetIPAddress()
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


        protected string Encrypt(string clearText)
        {
            //string EncryptionKey = "IBAZ2TWTQS77898";
            //byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            //using (Aes encryptor = Aes.Create())
            //{
            //    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            //    encryptor.Key = pdb.GetBytes(32);
            //    encryptor.IV = pdb.GetBytes(16);
            //    using (MemoryStream ms = new MemoryStream())
            //    {
            //        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
            //        {
            //            cs.Write(clearBytes, 0, clearBytes.Length);
            //            cs.Close();
            //        }
            //        clearText = Convert.ToBase64String(ms.ToArray());
            //    }
            //}
            CryptLib _crypt = new CryptLib();

            String key = "b16920894899c7780b5fc7161560a412";//CryptLib.SHA256("my secret key", 32); //32 bytes = 256 bit

            String iv = "e77886746a9b416d";
            //String iv = CryptLib.GenerateRandomIV(16); //16 bytes = 128 bits
            //string key = CryptLib.getHashSha256("my secret key", 31); //32 bytes = 256 bits
            String cypherText = _crypt.encrypt(clearText, key, iv);

            //Console.WriteLine("Plain text =" + _crypt.decrypt(cypherText, key, iv));
            return cypherText;
        }

        protected string Decrypt(string cipherText)
        {
            string EncryptionKey = "IBAZ2TWTQS77898";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public String changepass(String usrname, String oldpass, String newpass)
        {
            String encpass;
            String new_encpass;
            String lblconfirm = "System Error";
            encpass = Encrypt(oldpass);
            OracleCommand cmd;
            OracleDataReader dr;
            String Sqlstr;

            Sqlstr = "Select * from security_master where user_LOG= '"
                       + usrname + "' and user_pass= '"
                       + encpass + "'";
            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(Sqlstr, con);
                try
                {
                    con.Open();
                    dr = cmd.ExecuteReader();
                    OracleCommand cmd2;
                    OracleDataReader dr2;
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            new_encpass = Encrypt(newpass);
                            cmd2 = new OracleCommand("update security_master set user_pass='"
                                            + new_encpass + "' where user_log= '"
                                            + usrname + "'", con);
                            cmd2.ExecuteNonQuery();
                            lblconfirm = "Your Password was Changed Successfully";
                        }
                    }
                    else
                    {
                        lblconfirm = "Your Password was Not Changed successfully";
                    }

                }
                catch (Exception ex)
                {
                    // lblconfirm.Text = ex.Message
                    lblconfirm = "System Error";
                }
            }
            return lblconfirm;
        }


        public List<addaccount> Populatecustacts()
        {
            int i = 0; ;
            List<addaccount> items = new List<addaccount>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select  acc_no,branch_name,act_name,curr_name from USER_ACC_LINK acc,branchs br ,CURRENCY cur ,act_types cty" +
                    " where cur.CURR_STS='1' and  substr(acc.acc_no,3,3)= br.branch_code and substr(acc.acc_no,6,5)=cty.ACT_TYPE_CODE" +
                    " and substr(acc.acc_no,11,3)=cur.curr_code ";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new addaccount
                            {
                                AccountID = i + 1,
                                AccountNumber = sdr["acc_no"].ToString().Substring(12, 7),
                                AccountNumbercomplete = sdr["acc_no"].ToString(),
                                Branch = sdr["branch_name"].ToString(),
                                AccountType = sdr["act_name"].ToString(),
                                Currency = sdr["curr_name"].ToString(),
                                IsSelected = false,
                            });
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }
        public String addnewacount(String act, String account, String category, int userid)
        {
            String lblconfirm = "System Error", user_id = null;
            bool FLAG;
            OracleCommand cmd;
            OracleDataReader dr;
            using (OracleConnection con = new OracleConnection(conString))
            {
                try
                {
                    cmd = new OracleCommand("select acc_no from user_acc_link  where acc_no='" + account + "' and user_id = '" + userid + "'", con);
                    OracleCommand cmd2;
                    OracleCommand cmd_acc_lnk;
                    con.Open();
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        FLAG = false;
                        lblconfirm = "These Account Already exist";
                        con.Close();
                        return lblconfirm;
                    }
                    else
                    {
                        FLAG = true;
                    }

                    if (FLAG == true)
                    {

                        //string query = "select user_id,user_name from users where DEF_ACC='" + act + "'and CATOGRY='" + category + "'";

                        //OracleCommand cmd3 = new OracleCommand(query, con);
                        //OracleDataReader drr = cmd3.ExecuteReader();
                        //if (drr.Read())
                        //{
                        //    user_id = drr[0].ToString();
                        //}

                        cmd = new OracleCommand("select count(*) from user_acc_link where acc_no='" + account + "' and user_id='" + userid + "'", con);
                        dr = cmd.ExecuteReader();
                        dr.Read();
                        int counter;
                        counter = Convert.ToInt32(dr[0].ToString());
                        dr.Close();
                        cmd.Dispose();
                        if (counter == 0)
                        {
                            String dp_branch, dp_acc_tybe, dp_acc_curr;
                            dp_acc_tybe = account.Substring(5, 5);
                            dp_branch = account.Substring(2, 3);
                            dp_acc_curr = account.Substring(10, 3);
                            String sql2 = "select  nvl(max (acc_id),0) from user_acc_link where user_id=" + userid;
                            cmd2 = new OracleCommand(sql2, con);
                            dr = cmd2.ExecuteReader();
                            dr.Read();
                            int ACC_ID;
                            ACC_ID = Convert.ToInt32(dr[0].ToString());
                            dr.Close();
                            cmd2.Dispose();
                            ACC_ID = ACC_ID + 1;
                            cmd_acc_lnk = new OracleCommand("INSERT INTO user_acc_link (BRANCH_CODE,ACT_TYPE,USER_ID,ACC_NO,ACC_STS,ACC_CURR,ACC_LANG,ACC_STATUS,ACC_ID,CATOGRY) values ('"
                                             + dp_branch + "','" + dp_acc_tybe + "','" + userid + "','" + account + "','P','" + dp_acc_curr + "','AR','P','" + ACC_ID + "',  '" + category + "')", con);
                            cmd_acc_lnk.ExecuteNonQuery();
                            lblconfirm = "Account Added Successfully";
                        }
                        else
                        {
                            lblconfirm = "These Account Already exist";
                        }

                        con.Close();
                    }


                }
                catch (Exception ex)
                {
                    lblconfirm = "System Error";
                }
            }
            return lblconfirm;
        }

        public List<int> getaccountsids(string accountnumber)
        {
            List<int> ids = new List<int>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select user_id from users where def_acc = '" + accountnumber + "'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            ids.Add(int.Parse(sdr["user_id"].ToString()));
                        }
                    }
                    con.Close();
                }
            }
            return ids;
        }

        public string checkaccountifbound(string accountnumber, string userid)
        {
            OracleCommand cmd;
            OracleDataReader dr;
            using (OracleConnection con = new OracleConnection(conString))
            {
                con.Open();
                cmd = new OracleCommand("select count(*) from user_acc_link where acc_no='" + accountnumber + "' and user_id='" + userid + "'", con);
                dr = cmd.ExecuteReader();
                dr.Read();
                int counter;
                counter = Convert.ToInt32(dr[0].ToString());
                con.Close();
                dr.Close();
                if (counter == 1)
                {
                    return "Account already exists";
                }
                else
                {
                    return "Account available";
                }
            }
        }

        public string getcustomerfullname(string primaryaccount)
        {
            string customername = "N/A";
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select user_name from users where def_acc = '" + primaryaccount + "'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            customername = sdr["user_name"].ToString();
                        }
                    }
                    con.Close();
                }
            }
            return customername;
        }

        public List<pendingacts> Pendingacounts(String bracode)
        {
            OracleCommand cmd;
            OracleDataReader dr;

            String userid, username, useract, newuseract, newuseractcomplete;
            String query1, result;

            List<pendingacts> customer = new List<pendingacts>();

            if (!bracode.Equals("000"))
            {
                query1 = "select user_acc_link.user_id,user_name,b.branch_name||' - '||t.act_name||' - '||c.curr_name||' - '||SUBSTR(def_acc,14,7) def_acc,(select branch_name  from branchs where branch_code =SUBSTR(ACC_NO,3,3))||'-'||(select act_name  from act_types where act_type_code =SUBSTR(ACC_NO,6,5))||'-'||(select curr_name  from currency where  CURR_STS='1' and  CURR_CODE =SUBSTR(ACC_NO,11,3))||'-'||SUBSTR(ACC_NO,14), ACC_NO from users , user_acc_link ,branchs b ,act_types t , currency c where   c.CURR_STS='1' and   SUBSTR(def_acc,3,3)=b.branch_code and SUBSTR(def_acc,6,5)=t.act_type_code and SUBSTR(def_acc,11,3)=c.CURR_CODE and ACC_STATUS='P' and user_acc_link.user_id=users.user_id order by user_id  and substr(def_acc,3,3)='" + bracode + "' order by user_id";
            }
            else
            {
                query1 = "select user_acc_link.user_id,user_name,b.branch_name||' - '||t.act_name||' - '||c.curr_name||' - '||SUBSTR(def_acc,14,7) def_acc,(select branch_name  from branchs where branch_code =SUBSTR(ACC_NO,3,3))||'-'||(select act_name  from act_types where act_type_code =SUBSTR(ACC_NO,6,5))||'-'||(select curr_name  from currency where  CURR_STS='1' and  CURR_CODE =SUBSTR(ACC_NO,11,3))||'-'||SUBSTR(ACC_NO,14), ACC_NO from users , user_acc_link ,branchs b ,act_types t , currency c where   c.CURR_STS='1' and   SUBSTR(def_acc,3,3)=b.branch_code and SUBSTR(def_acc,6,5)=t.act_type_code and SUBSTR(def_acc,11,3)=c.CURR_CODE and ACC_STATUS='P' and user_acc_link.user_id=users.user_id order by user_id";
            }

            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        userid = dr[0].ToString();
                        username = dr[1].ToString();
                        useract = dr[2].ToString();
                        newuseract = dr[3].ToString();
                        newuseractcomplete = dr[4].ToString();

                        customer.Add(new pendingacts
                        {
                            USER_ID = userid,
                            USER_NAME = username,
                            DEF_ACC = useract,
                            ACC_NO = newuseract,
                            ACC_NO1 = newuseractcomplete
                        });
                    }
                }


            }


            return customer;
        }



        public List<actAuthorizationinfo> newactAuthorizationinfo(string userid, string act)
        {

            OracleCommand cmd;
            OracleDataReader dr;

            String username, useract;
            String query1, result;
            List<actAuthorizationinfo> customer = new List<actAuthorizationinfo>();
            OracleDataReader dr3;
            OracleCommand cmd3;
            string sqstr;
            string msg = "";
            string br, Sessioncurr = "";
            String acc = "";
            String acc_type = "";
            String acc_no;
            String curr;
            String curr_name = "";
            String lang;
            String brname = "";
            String acctype = "";
            String roleid = "", profilename = "";
            query1 = "select user_acc_link.user_id,user_name,def_acc,ACC_NO from users , user_acc_link where user_acc_link.user_id='" + userid + "' and users.user_id='" + userid + "' and ACC_NO='" + act + "' and user_acc_link.acc_status='P'";
            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd3 = new OracleCommand(query1, con);

                con.Open();


                dr3 = cmd3.ExecuteReader();
                if (dr3.HasRows)
                {
                    while (dr3.Read())
                    {
                        // 'lb_cust_name.Text = dr3(1)
                        OracleDataReader dr2;
                        OracleDataReader dr4;
                        OracleDataReader dr5;
                        OracleDataReader dr6;
                        OracleCommand cmd2;
                        OracleCommand cmd4;
                        OracleCommand cmd5;
                        OracleCommand cmd6;


                        acc = dr3[3].ToString();
                        br = acc.Substring(2, 3);
                        acc_type = acc.Substring(5, 5);
                        Sessioncurr = acc.Substring(10, 3);
                        acc_no = acc.Substring(13);

                        cmd4 = new OracleCommand(("select BRANCH_NAME from BRANCHS where BRANCH_CODE_NO='" + br + "'"), con);
                        dr4 = cmd4.ExecuteReader();
                        if (dr4.Read())
                        {
                            brname = dr4[0].ToString();

                        }

                        dr4.Close();
                        cmd5 = new OracleCommand(("select act_name from act_types where act_type_code ='" + (acc_type + "'")), con);
                        dr5 = cmd5.ExecuteReader();
                        if (dr5.HasRows)
                        {
                            dr5.Read();
                            acctype = dr5[0].ToString();

                        }
                        else
                        {
                            cmd5 = new OracleCommand(("select act_name from invact_types where act_type_code='" + (acc_type + "'")), con);
                            dr5 = cmd5.ExecuteReader();
                            if (dr5.HasRows)
                            {
                                dr5.Read();
                                acctype = dr5[0].ToString();

                            }
                            else
                                acctype = "Account Type Not Found";

                        }

                        dr5.Close();

                        cmd6 = new OracleCommand(("select CURR_NAME from CURRENCY where  CURR_STS='1' and  CURR_CODE = '" + Sessioncurr + "'"), con);
                        dr6 = cmd6.ExecuteReader();
                        if (dr6.Read())
                        {
                            curr_name = dr6[0].ToString();

                        }

                        dr6.Close();

                        customer.Add(new actAuthorizationinfo
                        {
                            userid = dr3[0].ToString(),
                            Branch = brname,
                            AccountType = acctype,
                            Customername = dr3[1].ToString(),
                            Currency = curr_name,
                            Customeraccount = acc.Substring(12, 7),
                            completeact = acc,
                        });
                    }
                    dr3.Close();
                }
            }
            return customer;
        }

        public int updateAccount(String userid, String account, String status)
        {
            OracleCommand cmd;
            int result = -1;


            String query1;
            query1 = "update user_acc_link set ACC_STATUS='" + status + "', ACC_STS='" + status + "' where  ACC_no='" + account + "' and  user_id ='" + userid + "'";
            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();
                result = cmd.ExecuteNonQuery();
            }
            return result;
        }
        public List<GETpassword> getpassword(String account)
        {
            OracleCommand cmd;
            OracleDataReader dr;
            String acttypename = "", acttype;
            List<GETpassword> list = new List<GETpassword>();
            string enc_pwd = "", br, branchname = "", lblconfirm = "System Error", pass, name = "";
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd4, cmd5;

                OracleDataReader dr4, dr5;

                try
                {

                    br = account.Substring(2, 3);
                    acttype = account.Substring(5, 5);


                    cmd4 = new OracleCommand("select BRANCH_NAME from BRANCHS where BRANCH_CODE_NO='" + br + "'", con);
                    con.Open();
                    dr4 = cmd4.ExecuteReader();
                    if (dr4.Read())
                    {
                        branchname = dr4[0].ToString();
                    }

                    cmd5 = new OracleCommand("select act_name from act_types  where act_type_code='" + acttype + "'", con);
                    dr5 = cmd5.ExecuteReader();
                    if (dr5.Read())
                    {
                        acttypename = dr5[0].ToString();
                    }


                    cmd = new OracleCommand("select USER_PAS,DEF_ACC ,USER_NAME from users where DEF_ACC='" + account + "'", con);
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        enc_pwd = dr[0].ToString();
                        pass = enc_pwd;
                        account = branchname + "-" + acttypename + "-" + dr[1].ToString().Substring(11, 7);
                        name = dr[2].ToString();
                        lblconfirm = "Successfully";
                    }
                    else
                    {
                        lblconfirm = "Wrong Customer Account";
                    }

                }
                catch (Exception ex)
                {
                    lblconfirm = "System Error";
                }
                list.Add(new GETpassword
                {
                    pass = enc_pwd,
                    name = name,
                    account = account,
                    lblconfirm = lblconfirm,
                    branchname = branchname,
                });
                return list;
            }
        }

        public List<resetpass> resetpassword(String account)
        {


            List<resetpass> list = new List<resetpass>();
            string enc_pwd = "", br, branchname = "", lblconfirm = "System Error", pass, name = "";
            OracleCommand cmd, cmd1;
            OracleDataReader dr, dr1;
            String Sqlstr, sqlstr1;
            String re = "", enc_pwd2, str;
            Random random = new Random();
            using (OracleConnection con = new OracleConnection(conString))
            {

                try
                {
                    //re = CreatePassword(8); random 8 characters
                    re = random.Next(10000000, 99999999).ToString();

                    enc_pwd = Encrypt(re);
                    enc_pwd2 = enc_pwd;
                    br = account.Substring(2, 3);

                    OracleCommand cmd4;

                    OracleDataReader dr4;

                    cmd4 = new OracleCommand("select BRANCH_NAME from BRANCHS where BRANCH_CODE_NO='" + br + "'", con);
                    con.Open();
                    dr4 = cmd4.ExecuteReader();
                    if (dr4.Read())
                    {
                        branchname = dr4[0].ToString();
                    }


                    sqlstr1 = "select DEF_ACC ,USER_NAME from users where def_acc='" + account + "'";
                    cmd1 = new OracleCommand(sqlstr1, con);
                    dr1 = cmd1.ExecuteReader();
                    if (dr1.Read())
                    {

                        account = dr1[0].ToString();
                        name = dr1[1].ToString();
                        cmd = new OracleCommand("update users set USER_STATUS='A',user_pwd='" + enc_pwd.ToString() + "' , first_login='T',USER_PAS='" + re.ToString() + "' where  def_acc='" + account + "'", con);
                        cmd.ExecuteNonQuery();
                        lblconfirm = "Successfully";


                    }
                    else
                    {
                        lblconfirm = "Pleace Check Your Account";
                    }
                }
                catch (Exception ex)
                {
                    lblconfirm = "System Error";
                }
                list.Add(new resetpass
                {
                    pass = re,
                    name = name,
                    account = account,
                    lblconfirm = lblconfirm,
                    branchname = branchname,
                });
                return list;
            }
        }



        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public String addnewacount(String act, String account, String category)
        {
            String lblconfirm = "System Error", user_id = null;
            bool FLAG;
            OracleCommand cmd;
            OracleDataReader dr;
            using (OracleConnection con = new OracleConnection(conString))
            {
                try
                {
                    cmd = new OracleCommand("select acc_no from user_acc_link  where acc_no='" + account + "' and CATOGRY='" + category + "'", con);
                    OracleCommand cmd2;
                    OracleCommand cmd_acc_lnk;
                    con.Open();
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        FLAG = false;
                        lblconfirm = "These Account Already exist";
                        con.Close();
                        return lblconfirm;
                    }
                    else
                    {
                        FLAG = true;
                    }

                    if (FLAG == true)
                    {

                        string query = "select user_id,user_name from users where DEF_ACC='" + act + "'and CATOGRY='" + category + "'";

                        OracleCommand cmd3 = new OracleCommand(query, con);
                        OracleDataReader drr = cmd3.ExecuteReader();
                        if (drr.Read())
                        {
                            user_id = drr[0].ToString();
                        }
                        cmd = new OracleCommand("select count(*) from user_acc_link where acc_no='" + account + "' and user_id='" + user_id + "'", con);
                        dr = cmd.ExecuteReader();
                        dr.Read();
                        int counter;
                        counter = Convert.ToInt32(dr[0].ToString());
                        dr.Close();
                        cmd.Dispose();
                        if (counter == 0)
                        {
                            String dp_branch, dp_acc_tybe, dp_acc_curr;
                            dp_acc_tybe = account.Substring(5, 5);
                            dp_branch = account.Substring(2, 3);
                            dp_acc_curr = account.Substring(10, 3);
                            String sql2 = "select  nvl(max (acc_id),0) from user_acc_link where user_id=" + user_id;
                            cmd2 = new OracleCommand(sql2, con);
                            dr = cmd2.ExecuteReader();
                            dr.Read();
                            int ACC_ID;
                            ACC_ID = Convert.ToInt32(dr[0].ToString());
                            dr.Close();
                            cmd2.Dispose();
                            ACC_ID = ACC_ID + 1;
                            cmd_acc_lnk = new OracleCommand("INSERT INTO user_acc_link (BRANCH_CODE,ACT_TYPE,USER_ID,ACC_NO,ACC_STS,ACC_CURR,ACC_LANG,ACC_STATUS,ACC_ID,CATOGRY) values ('"
                                             + dp_branch + "','" + dp_acc_tybe + "','" + user_id + "','" + account + "','P','" + dp_acc_curr + "','AR','P','" + ACC_ID + "',  '" + category + "')", con);
                            cmd_acc_lnk.ExecuteNonQuery();
                            lblconfirm = "Account Added Successfully";
                        }
                        else
                        {
                            lblconfirm = "These Account Already exist";
                        }
                        con.Close();
                    }


                }
                catch (Exception ex)
                {
                    lblconfirm = "System Error";
                }
            }
            return lblconfirm;
        }

        public custinfo getcustinfo(String branchcode, String acttype, String acc_no, String acc_curr, String category, String accountnumber)
        {
            Boolean FLAG;
            String lblconfirm = "";
            OracleCommand cmd;
            OracleDataReader dr;
            int counter;
            custinfo model = new custinfo();
            //String query1 = "select  u.user_id, u.user_name,u.user_log,u.user_pwd,u.user_email,u.user_mobile,u.user_adrs,m.name,u.user_status" +
            // " from users u, tbl_rolemaster m  where u.roleid=m.roleid and u.DEF_ACC='13" + branchcode + acttype + acc_curr + acc_no + "' and  catogry ='"+category+"'";
            string operatornumber = accountnumber + "O";
            string authorizornumber = accountnumber + "A";
            String query1 = "select * from users,tbl_rolemaster where users.ROLEID = tbl_rolemaster.roleid and user_log = '" + accountnumber + "' or def_acc = '" + accountnumber + "' or user_log = '" + operatornumber + "' or user_log = '" + authorizornumber + "'";


            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);
                con.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {

                        //model.user_id = dr[0].ToString();
                        //model.user_name = dr[1].ToString();
                        //model.user_log = dr[2].ToString();
                        //model.user_pwd = dr[3].ToString();
                        //model.user_email = dr[4].ToString();
                        //model.user_adrs = dr[6].ToString();
                        //model.user_mobile = dr[5].ToString();
                        //model.name = dr[7].ToString();
                        //model.status = dr[8].ToString();
                        model.user_id = dr["USER_ID"].ToString();
                        model.user_name = dr["USER_NAME"].ToString();
                        model.user_log = dr["USER_LOG"].ToString();
                        model.user_pwd = dr["USER_PWD"].ToString();
                        model.user_email = dr["USER_EMAIL"].ToString();
                        model.user_adrs = dr["USER_ADRS"].ToString();
                        model.user_mobile = dr["USER_MOBILE"].ToString();
                        model.name = dr["USER_NAME"].ToString();
                        model.status = dr["USER_STATUS"].ToString();
                        model.lblconfirm = "This Account is Already exist";
                    }
                }
                else
                {
                    model.lblconfirm = "This Account is available";
                }
            }
            return model;
        }

        public int Updatecustomer(String sts, custinfo model)
        {
            int result = -1;

            if (sts.Equals("U"))
            {
                OracleCommand cmd;
                OracleDataReader dr;
                string Sqlstr;
                string sql2;
                string re;
                try
                {

                    Sqlstr = "update USERS set USER_NAME ='" + model.user_name + "',USER_EMAIL ='"
                                + model.user_email + "',user_log ='"
                                + model.user_log + "',USER_ADRS ='"
                                + model.user_adrs + " ', ROLEID='"
                                + model.profileCode + "' where USER_ID ='" + model.user_id + "'";
                    OracleCommand cmd1;
                    OracleCommand cmd2;
                    if (model.Channel == "1")
                    {
                        sql2 = "update user_channel set USER_EBANK = 'T',USER_EMOBILE ='F' where USERID = '" + model.user_id + "'";
                        using (OracleConnection con = new OracleConnection(conString))
                        {
                            cmd = new OracleCommand(sql2, con);

                            con.Open();


                            result = cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                    else if (model.Channel == "2")
                    {
                        sql2 = "update user_channel set USER_EBANK = 'F',USER_EMOBILE ='T' where USERID = '" + model.user_id + "'";
                        using (OracleConnection con = new OracleConnection(conString))
                        {
                            cmd = new OracleCommand(sql2, con);

                            con.Open();


                            result = cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                    else if (model.Channel == "3")
                    {
                        sql2 = "update user_channel set USER_EBANK = 'T',USER_EMOBILE ='T' where USERID = '" + model.user_id + "'";
                        using (OracleConnection con = new OracleConnection(conString))
                        {
                            cmd = new OracleCommand(sql2, con);

                            con.Open();


                            result = cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                    OracleDataReader dr1;
                    using (OracleConnection con = new OracleConnection(conString))
                    {
                        cmd = new OracleCommand(Sqlstr, con);

                        con.Open();


                        result = cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    result = -2;
                }
            }
            else
            {
                try
                {

                    String Sqlstr = "update users set USER_STATUS='DE'  where USER_ID ='" + model.user_id + "'";
                    using (OracleConnection con = new OracleConnection(conString))
                    {

                        OracleCommand cmd = new OracleCommand(Sqlstr, con);
                        con.Open();
                        result = cmd.ExecuteNonQuery();
                        Sqlstr = "update USER_ACC_LINK set ACC_STS='0'  where USER_ID ='" + model.user_id + "'";
                        cmd = new OracleCommand(Sqlstr, con);
                        cmd.ExecuteNonQuery();

                        result = -3;
                    }
                }
                catch (Exception ex)
                {
                    result = -2;
                }
            }
            return result;
        }
        public List<pageparameter> PopulateProfilemangement(String categoryid)
        {
            int i = 0; ;
            List<pageparameter> items = new List<pageparameter>();
            using (OracleConnection con = new OracleConnection(conString))
            {

                string query = "select  t.menuid ,t.menuname,t.menu_ar_name , tm.menuname  parnet_name,tm.menu_ar_name parnet_name_ar,tm.menuid  parnet_id from (select  menu_ar_name,  menuname,menuid  from cpanel_menumaster where MENUPARENTId=0  ) tm ,cpanel_menumaster t    where t.MENUPARENTID<>0 and t.menuparentid=tm.menuid  and menu_category in ('" + categoryid + "','1')  order by menuid ,menuparentid";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new pageparameter
                            {
                                menuid = sdr[0].ToString(),
                                menuname = sdr[1].ToString(),
                                menuname_ar = sdr[2].ToString(),
                                Parent_menuname = sdr[3].ToString(),
                                Parent_menuname_ar = sdr[4].ToString(),

                                menuparentid = sdr[5].ToString(),
                                IsSelected = false,
                            });
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }

        public List<pageparameter> PopulateCustomerProfilemangement(String categoryid)
        {
            int i = 0; ;
            List<pageparameter> items = new List<pageparameter>();
            using (OracleConnection con = new OracleConnection(conString))
            {

                string query = "select  t.menuid ,t.menuname,t.menu_ar_name , tm.menuname  parnet_name,tm.menu_ar_name parnet_name_ar,tm.menuid  parnet_id from (select  menu_ar_name,  menuname,menuid  from tbl_menumaster where MENUPARENTId=0  ) tm ,tbl_menumaster t    where t.MENUPARENTID<>0 and t.menuparentid=tm.menuid  and menu_category in ('" + categoryid + "','1')  order by menuid ,menuparentid";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new pageparameter
                            {
                                menuid = sdr[0].ToString(),
                                menuname = sdr[1].ToString(),
                                menuname_ar = sdr[2].ToString(),
                                Parent_menuname = sdr[3].ToString(),
                                Parent_menuname_ar = sdr[4].ToString(),

                                menuparentid = sdr[5].ToString(),
                                IsSelected = false,
                            });
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }


        public List<SelectListItem> GetGatgories()

        {
            List<SelectListItem> items = new List<SelectListItem>();



            int i = 0;

            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select  cat_id,cat_name from category";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {

                            items.Add(new SelectListItem
                            {
                                Text = "-- Select Customer category --",
                                Value = "0",
                            });
                            while (sdr.Read())
                            {

                                items.Add(new SelectListItem
                                {
                                    Text = sdr[1].ToString(),
                                    Value = sdr[0].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
            }

            return items;

        }


        public List<SelectListItem> GetGatgorieses()

        {
            List<SelectListItem> items = new List<SelectListItem>();



            int i = 0;

            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select  cat_id,cat_name from category";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {

                            //items.Add(new SelectListItem
                            //{
                            //    Text = "-- Select Customer category --",
                            //    Value = "0",
                            //});
                            while (sdr.Read())
                            {

                                items.Add(new SelectListItem
                                {
                                    Text = sdr[1].ToString(),
                                    Value = sdr[0].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
            }

            return items;

        }

        public String deleteaccount(String act, String account, String category)
        {
            String lblconfirm = "System Error", user_id = null;
            bool FLAG;
            OracleCommand cmd;
            OracleDataReader dr;
            OracleCommand cmd2;
            OracleCommand cmd_acc_lnk;
            OracleCommand delete_cmd;
            using (OracleConnection con = new OracleConnection(conString))
            {
                try
                {
                    con.Open();
                    user_id = getCustIDFromAcc(act);
                    cmd = new OracleCommand("select count(*) from user_acc_link where acc_no='" + account + "' and user_id='" + user_id + "'", con);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    int counter;
                    counter = Convert.ToInt32(dr[0].ToString());
                    dr.Close();
                    cmd.Dispose();
                    if (counter == 1)
                    {
                        String dp_branch, dp_acc_tybe, dp_acc_curr;
                        dp_acc_tybe = account.Substring(5, 5);
                        dp_branch = account.Substring(2, 3);
                        dp_acc_curr = account.Substring(10, 3);
                        String sql2 = "select  nvl(max (acc_id),0) from deleted_user_acc_link where user_id=" + user_id;
                        cmd2 = new OracleCommand(sql2, con);
                        dr = cmd2.ExecuteReader();
                        dr.Read();
                        int ACC_ID;
                        ACC_ID = Convert.ToInt32(dr[0].ToString());
                        dr.Close();
                        cmd2.Dispose();
                        ACC_ID = ACC_ID + 1;
                        delete_cmd = new OracleCommand("delete from user_acc_link where user_id = '" + user_id + "' and acc_no = '" + account + "'", con);
                        delete_cmd.ExecuteNonQuery();
                        cmd_acc_lnk = new OracleCommand("INSERT INTO deleted_user_acc_link (BRANCH_CODE,ACT_TYPE,USER_ID,ACC_NO,ACC_STS,ACC_CURR,ACC_LANG,ACC_STATUS,ACC_ID,CATOGRY) values ('"
                                            + dp_branch + "','" + dp_acc_tybe + "','" + user_id + "','" + account + "','D','" + dp_acc_curr + "','AR','D','" + ACC_ID + "',  '" + category + "')", con);
                        cmd_acc_lnk.ExecuteNonQuery();
                        lblconfirm = "Account Deleted Successfully";
                    }
                    else
                    {
                        lblconfirm = "These Account Already exist";
                    }
                    con.Close();
                }
                catch (Exception ex)
                {
                    lblconfirm = "System Error";
                }
            }
            return lblconfirm;
        }

        public Boolean checkaccountbelongstouser(string userid, string accountnumber)
        {
            Boolean result = false;
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select * from user_acc_link where user_id = '" + userid + "' and acc_no = '" + accountnumber + "'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            result = true;
                        }
                    }
                    con.Close();
                }
            }
            return result;
        }

        public custinfo getcustinfo(String branchcode, String acttype, String acc_no, String acc_curr, String category)
        {
            Boolean FLAG;
            String lblconfirm = "";
            OracleCommand cmd;
            OracleDataReader dr;
            int counter;
            custinfo model = new custinfo();
            String query1 = "select  u.user_id, u.user_name,u.user_log,u.user_pwd,u.user_email,u.user_mobile,u.user_adrs,m.name,u.user_status" +
             " from users u, tbl_rolemaster m  where u.roleid=m.roleid and u.DEF_ACC='11" + branchcode + acttype + acc_no + "00" + acc_curr + "000' and  catogry ='" + category + "'";

            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {

                        model.user_id = dr[0].ToString();
                        model.user_name = dr[1].ToString();
                        model.user_log = dr[2].ToString();
                        model.user_pwd = dr[3].ToString();
                        model.user_email = dr[4].ToString();
                        model.user_adrs = dr[6].ToString();
                        model.user_mobile = dr[5].ToString();
                        model.name = dr[7].ToString();
                        model.status = dr[8].ToString();
                        model.lblconfirm = "This Account is Already exist";

                    }

                }


                else
                {
                    model.lblconfirm = "This Account is available";
                }




            }
            return model;
        }

        public Customerinfopass GetUserinfoData(string idorname)
        {
            Customerinfopass usermodel = new Customerinfopass();
            //char[] chararray = idorname.ToCharArray();
            //if (char.IsDigit(chararray[0]) && idorname.Length == 12)
            //{
            using (OracleConnection con = new OracleConnection(conString))
            {
                //string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_name, SUBSTR(def_acc,6,5) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,5)) as account_type,SUBSTR(def_acc,11,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name from users where user_id = '" + int.Parse(idorname) + "'";
                string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_name, SUBSTR(def_acc,6,5) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,5)) as account_type,SUBSTR(def_acc,14,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name ,(select user_name from users where user_log = '" + idorname + "' or user_mobile = '" + idorname + "' or  SUBSTR(users.def_acc,14,5) = '" + idorname + "') as user_name from users where user_log = '" + idorname + "' or user_mobile = '" + idorname + "' or  SUBSTR(users.def_acc,14,5) = '" + idorname + "'";
                OracleCommand cmd = new OracleCommand(query, con);
                con.Open();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        usermodel.BranchCode = dataReader["branch_code"].ToString();
                        usermodel.Branch = dataReader["branch_name"].ToString();
                        usermodel.CurrencyCode = dataReader["currency_code"].ToString();
                        usermodel.Currency = dataReader["currency_name"].ToString();
                        usermodel.AccountTypecode = dataReader["account_type_code"].ToString();
                        usermodel.AccountType = dataReader["account_type"].ToString();
                        usermodel.AccountNumber = dataReader["account_number"].ToString();
                        usermodel.CategoryCode = dataReader["category_id"].ToString();
                        usermodel.category = dataReader["category_name"].ToString();
                        usermodel.CustomerName = dataReader["user_name"].ToString();
                        usermodel.CustomerID = idorname;
                    }
                }
                return usermodel;
            }
            //}
            //else if (char.IsDigit(chararray[0]))
            //{
            //    using (OracleConnection con = new OracleConnection(conString))
            //    {
            //        //string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_name, SUBSTR(def_acc,6,5) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,5)) as account_type,SUBSTR(def_acc,11,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name from users where user_id = '" + int.Parse(idorname) + "'";
            //        string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,21,2)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,21,2)) as currency_name, SUBSTR(def_acc,6,6) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,6)) as account_type,SUBSTR(def_acc,12,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name,SUBSTR(def_acc,19,2) as subno, SUBSTR(def_acc,23,3) as subgl from users where user_id = '" + idorname + "'";
            //        OracleCommand cmd = new OracleCommand(query, con);
            //        con.Open();
            //        using (IDataReader dataReader = cmd.ExecuteReader())
            //        {
            //            while (dataReader.Read())
            //            {
            //                usermodel.BranchCode = dataReader["branch_code"].ToString();
            //                usermodel.Branch = dataReader["branch_name"].ToString();
            //                usermodel.CurrencyCode = dataReader["currency_code"].ToString();
            //                usermodel.Currency = dataReader["currency_name"].ToString();
            //                usermodel.AccountTypecode = dataReader["account_type_code"].ToString();
            //                usermodel.AccountType = dataReader["account_type"].ToString();
            //                usermodel.AccountNumber = dataReader["account_number"].ToString();
            //                usermodel.CategoryCode = dataReader["category_id"].ToString();
            //                usermodel.category = dataReader["category_name"].ToString();
            //                usermodel.SUBNO = dataReader["subno"].ToString();
            //                usermodel.SUBGL = dataReader["subgl"].ToString();
            //                usermodel.CustomerID = idorname;
            //            }
            //        }
            //        return usermodel;
            //    }
            //}
            //else
            //{
            //    using (OracleConnection con = new OracleConnection(conString))
            //    {
            //        //string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_name, SUBSTR(def_acc,6,5) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,5)) as account_type,SUBSTR(def_acc,11,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name from users where user_log = '" + idorname + "'";
            //        string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,21,2)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,21,2)) as currency_name, SUBSTR(def_acc,6,6) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,6)) as account_type,SUBSTR(def_acc,12,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name,SUBSTR(def_acc,19,2) as subno, SUBSTR(def_acc,23,3) as subgl from users where user_log = '" + idorname + "'";
            //        OracleCommand cmd = new OracleCommand(query, con);
            //        con.Open();
            //        using (IDataReader dataReader = cmd.ExecuteReader())
            //        {
            //            while (dataReader.Read())
            //            {
            //                usermodel.BranchCode = dataReader["branch_code"].ToString();
            //                usermodel.Branch = dataReader["branch_name"].ToString();
            //                usermodel.CurrencyCode = dataReader["currency_code"].ToString();
            //                usermodel.Currency = dataReader["currency_name"].ToString();
            //                usermodel.AccountTypecode = dataReader["account_type_code"].ToString();
            //                usermodel.AccountType = dataReader["account_type"].ToString();
            //                usermodel.AccountNumber = dataReader["account_number"].ToString();
            //                usermodel.CategoryCode = dataReader["category_id"].ToString();
            //                usermodel.category = dataReader["category_name"].ToString();
            //                usermodel.SUBNO = dataReader["subno"].ToString();
            //                usermodel.SUBGL = dataReader["subgl"].ToString();
            //                usermodel.CustomerID = idorname;
            //            }
            //        }
            //        return usermodel;
            //    }
            //}
        }

        public List<SelectListItem> CPanel_GetGatgories()
        {
            List<SelectListItem> items = new List<SelectListItem>();



            int i = 0;

            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select  cat_id,cat_name from CPANEL_CATEGORY";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {

                            items.Add(new SelectListItem
                            {
                                Text = "-- Select Customer category --",
                                Value = "0",
                            });
                            while (sdr.Read())
                            {

                                items.Add(new SelectListItem
                                {
                                    Text = sdr[1].ToString(),
                                    Value = sdr[0].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
            }

            return items;

        }

        public Custreport GetCustomerReportData(string idorname)
        {
            Custreport usermodel = new Custreport();

            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name,(select curr_name from currency where curr_code =  SUBSTR(def_acc,21,2)) as currency_name, (select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,6)) as account_type,SUBSTR(def_acc,11,7) as account_number, (select cat_name from category where cat_id = users.catogry) as category_name,user_name,user_log,user_email,user_mobile,decode(user_status,'A','Active','D','Deactive','R','Rejected','P','Pending','DE','Deleted','U','Unauthorized') as user_status,last_login,wrong_password from users where user_id = '" + int.Parse(idorname) + "' or user_log = '" + idorname + "' or user_mobile = '" + idorname + "' or  SUBSTR(users.def_acc,14,5) = '" + idorname + "'";
                OracleCommand cmd = new OracleCommand(query, con);
                con.Open();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        usermodel.Branch = dataReader["branch_name"].ToString();
                        usermodel.Currency = dataReader["currency_name"].ToString();
                        usermodel.AccountType = dataReader["account_type"].ToString();
                        usermodel.AccountNumber = dataReader["account_number"].ToString();
                        usermodel.category = dataReader["category_name"].ToString();
                        usermodel.CustomerName = dataReader["user_name"].ToString();
                        usermodel.username = dataReader["user_log"].ToString();
                        usermodel.user_email = dataReader["user_email"].ToString();
                        usermodel.phonenumber = dataReader["user_mobile"].ToString();
                        usermodel.CustStatus = dataReader["user_status"].ToString();
                        usermodel.lastlogin = dataReader["last_login"].ToString();
                        usermodel.wrong_passwords = dataReader["wrong_password"].ToString();
                        usermodel.CustomerID = idorname;
                    }
                }
                return usermodel;
            }
        }

        public List<String> GetCustomerLinkedAccounts(string CustomerID)
        {
            List<string> stringlist = new List<string>();

            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select decode(acc_sts,'P','Pending','A','Active','B','Blocked','N/A') as status,'-' as separator,(select branch_name from branchs where branch_code = SUBSTR(user_acc_link.acc_no,3,3)) as branch_name,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(user_acc_link.acc_no,6,6)) as account_type,SUBSTR(acc_no,12,7) as account_number from user_acc_link where user_id = '" + int.Parse(CustomerID) + "'";
                OracleCommand cmd = new OracleCommand(query, con);
                con.Open();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        stringlist.Add(dataReader["status"].ToString() + " - " + dataReader["branch_name"].ToString() + " - " + dataReader["account_type"].ToString() + " - " + dataReader["account_number"].ToString());
                    }
                }
                return stringlist;
            }
        }

        public string GetCustomerChannels(string CustomerID)
        {
            string mobileholder = "F", ibankingholder = "F";

            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select user_ebank,user_emobile from user_channel where userid = '" + int.Parse(CustomerID) + "'";
                OracleCommand cmd = new OracleCommand(query, con);
                con.Open();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        mobileholder = dataReader["user_ebank"].ToString();
                        ibankingholder = dataReader["user_emobile"].ToString();
                    }
                }
            }

            if (mobileholder == "T" && ibankingholder == "T")
            {
                return "3";
            }
            else if (mobileholder == "F" && ibankingholder == "T")
            {
                return "2";
            }
            else if (mobileholder == "T" && ibankingholder == "F")
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }

        public List<profilesparameter> GetProfiles()
        {
            int i = 0; ;
            List<profilesparameter> items = new List<profilesparameter>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select t.roleid,name,DECODE (t.active,'1','Active','DeActive') status  from tbl_rolemaster t where t.name!='Admin' order by t.roleid";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new profilesparameter
                            {
                                profielid = sdr[0].ToString(),
                                profilename = sdr[1].ToString(),
                                profilestatus = sdr[2].ToString(),

                                IsSelected = false,
                            });
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }
        public List<SelectListItem> PopulateCustStatus()
        {
            List<SelectListItem> items = new List<SelectListItem>();



            int i = 0;

            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select  STATUS_CODE,STATUS_NAME from CUSTOMERSTATUS where ACTIVE='1'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {

                            items.Add(new SelectListItem
                            {
                                Text = "-- Select Customer Status --",
                                Value = "0",
                            });
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr[1].ToString(),
                                    Value = sdr[0].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
            }

            return items;

        }

        public List<SelectListItem> PopulateCustStatus(string idotusername)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            char[] chararray = idotusername.ToCharArray();
            if (char.IsDigit(chararray[0]))
            {
                using (OracleConnection con = new OracleConnection(conString))
                {
                    string query = "select CUSTOMERSTATUS.STATUS_CODE,CUSTOMERSTATUS.STATUS_NAME from CUSTOMERSTATUS inner join users on CUSTOMERSTATUS.STATUS_CODE = users.USER_STATUS where CUSTOMERSTATUS.ACTIVE='1' and users.user_id = '" + idotusername + "' or users.user_log = '" + idotusername + "' or users.user_mobile = '" + idotusername + "' or  SUBSTR(users.def_acc,14,5) = '" + idotusername + "'";
                    using (OracleCommand cmd = new OracleCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (OracleDataReader sdr = cmd.ExecuteReader())
                        {
                            if (sdr.HasRows)
                            {
                                //items.Add(new SelectListItem
                                //{
                                //    Text = "-- Select Customer Status --",
                                //    Value = "0",
                                //});
                                while (sdr.Read())
                                {
                                    items.Add(new SelectListItem
                                    {
                                        Text = sdr[1].ToString(),
                                        Value = sdr[0].ToString()
                                    });
                                }
                            }
                        }
                        con.Close();
                    }
                }
            }
            else
            {
                using (OracleConnection con = new OracleConnection(conString))
                {
                    string query = "select CUSTOMERSTATUS.STATUS_CODE,CUSTOMERSTATUS.STATUS_NAME from CUSTOMERSTATUS inner join users on CUSTOMERSTATUS.STATUS_CODE = users.USER_STATUS where CUSTOMERSTATUS.ACTIVE='1' and users.user_log = '" + idotusername + "' ";
                    using (OracleCommand cmd = new OracleCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (OracleDataReader sdr = cmd.ExecuteReader())
                        {
                            if (sdr.HasRows)
                            {
                                //items.Add(new SelectListItem
                                //{
                                //    Text = "-- Select Customer Status --",
                                //    Value = "0",
                                //});
                                while (sdr.Read())
                                {
                                    items.Add(new SelectListItem
                                    {
                                        Text = sdr[1].ToString(),
                                        Value = sdr[0].ToString()
                                    });
                                }
                            }
                        }
                        con.Close();
                    }
                }
            }

            return items;
        }


        internal string addnewprofile(string profilename, string menuid, string parnetid)
        {
            String lblconfirm = "System Error", profileid = null;
            bool FLAG;
            OracleCommand cmd;
            OracleDataReader dr;
            using (OracleConnection con = new OracleConnection(conString))
            {
                try
                {
                    cmd = new OracleCommand("select * from tbl_rolemaster where name='" + profilename + "'", con);
                    OracleCommand cmd2;
                    OracleCommand cmd_acc_lnk;

                    con.Open();
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        profileid = dr[0].ToString();
                        FLAG = true;
                        lblconfirm = "These Account Already exist";

                    }
                    else
                    {

                        cmd = new OracleCommand("select max(to_number(nvl(roleid,0)))+1 from tbl_rolemaster", con);

                        profileid = cmd.ExecuteScalar().ToString();

                        cmd_acc_lnk = new OracleCommand(" INSERT INTO tbl_rolemaster (ROLEID,NAME,ACTIVE) VALUES ('"
                                            + profileid + "','" + profilename + "','1' )", con);
                        cmd_acc_lnk.ExecuteNonQuery();
                        lblconfirm = "Account Added Successfully";
                        FLAG = true;
                    }

                    if (FLAG == true)
                    {

                        cmd = new OracleCommand("select   max(to_number(nvl(id,0)))+ 1 maxid from tbl_rolemenumapping", con);

                        String id = cmd.ExecuteScalar().ToString();
                        cmd = new OracleCommand("select id,roleid,menuid,active from tbl_rolemenumapping where  menuid='" + parnetid + "' and roleid='" + profileid + "'", con);
                        dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            cmd_acc_lnk = new OracleCommand("INSERT INTO tbl_rolemenumapping (ID,ROLEID,MENUID,ACTIVE)VALUES('"
                               + id + "','" + profileid + "','" + menuid + "','1')", con);
                            cmd_acc_lnk.ExecuteNonQuery();
                            lblconfirm = "Account Added Successfully";

                        }
                        else
                        {
                            cmd_acc_lnk = new OracleCommand("INSERT INTO tbl_rolemenumapping (ID,ROLEID,MENUID,ACTIVE)VALUES('"
                                + id + "','" + profileid + "','" + parnetid + "','1')", con);
                            cmd_acc_lnk.ExecuteNonQuery();
                            cmd_acc_lnk = new OracleCommand("INSERT INTO tbl_rolemenumapping (ID,ROLEID,MENUID,ACTIVE)VALUES('"
                               + Convert.ToInt32(id) + 1 + "','" + profileid + "','" + menuid + "','1')", con);
                            cmd_acc_lnk.ExecuteNonQuery();
                            lblconfirm = "Account Added Successfully";
                        }
                    }
                    else
                    {
                        lblconfirm = "These Account Already exist";
                    }
                    con.Close();



                }
                catch (Exception ex)
                {
                    lblconfirm = "System Error";
                }
            }
            return lblconfirm;

        }

        internal string cpanel_addnewprofile(string profilename, string menuid, string parnetid)
        {
            String lblconfirm = "System Error", profileid = null;
            bool FLAG;
            OracleCommand cmd;
            OracleDataReader dr;
            using (OracleConnection con = new OracleConnection(conString))
            {
                try
                {
                    cmd = new OracleCommand("select * from cpanel_rolemaster where name='" + profilename + "'", con);
                    OracleCommand cmd2;
                    OracleCommand cmd_acc_lnk;

                    con.Open();
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        profileid = dr[0].ToString();
                        FLAG = true;
                        lblconfirm = "These Account Already exist";

                    }
                    else
                    {
                        DateTime datetoday = DateTime.Now;
                        cmd = new OracleCommand("select max(to_number(nvl(roleid,0)))+1 from cpanel_rolemaster", con);

                        profileid = cmd.ExecuteScalar().ToString();

                        cmd_acc_lnk = new OracleCommand(" INSERT INTO cpanel_rolemaster (ROLEID,NAME,ACTIVE,INSERTED_DATE) VALUES ('"
                                            + profileid + "','" + profilename + "','1','" + datetoday.ToString() + "' )", con);
                        cmd_acc_lnk.ExecuteNonQuery();
                        lblconfirm = "Account Added Successfully";
                        FLAG = true;
                    }

                    if (FLAG == true)
                    {

                        cmd = new OracleCommand("select   max(to_number(nvl(id,0)))+ 1 maxid from cpanel_rolemenumapping", con);

                        String id = cmd.ExecuteScalar().ToString();
                        cmd = new OracleCommand("select id,roleid,menuid,active from cpanel_rolemenumapping where  menuid='" + parnetid + "' and roleid='" + profileid + "'", con);
                        dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            cmd_acc_lnk = new OracleCommand("INSERT INTO cpanel_rolemenumapping (ID,ROLEID,MENUID,ACTIVE)VALUES('"
                               + id + "','" + profileid + "','" + menuid + "','1')", con);
                            cmd_acc_lnk.ExecuteNonQuery();
                            lblconfirm = "Account Added Successfully";

                        }
                        else
                        {
                            cmd_acc_lnk = new OracleCommand("INSERT INTO cpanel_rolemenumapping (ID,ROLEID,MENUID,ACTIVE)VALUES('"
                                + id + "','" + profileid + "','" + parnetid + "','1')", con);
                            cmd_acc_lnk.ExecuteNonQuery();
                            cmd_acc_lnk = new OracleCommand("INSERT INTO cpanel_rolemenumapping (ID,ROLEID,MENUID,ACTIVE)VALUES('"
                               + Convert.ToInt32(id) + 1 + "','" + profileid + "','" + menuid + "','1')", con);
                            cmd_acc_lnk.ExecuteNonQuery();
                            lblconfirm = "Account Added Successfully";
                        }
                    }
                    else
                    {
                        lblconfirm = "These Account Already exist";
                    }
                    con.Close();



                }
                catch (Exception ex)
                {
                    lblconfirm = "System Error";
                }
            }
            return lblconfirm;

        }

        public List<channel> Channels()
        {
            List<channel> AvailableItems = new List<channel>();
            using (OracleConnection con = new OracleConnection(conString))
            {

                using (OracleCommand cmd = new OracleCommand())
                {
                    cmd.CommandText = "getchannel";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Connection = con;
                    con.Open();

                    cmd.Parameters.Add("channel_Cursor", OracleType.Cursor).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("res", OracleType.Int32).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("errcode", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("errmsg", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    String res = cmd.Parameters["res"].Value.ToString();
                    String errormsg = cmd.Parameters["errmsg"].Value.ToString();
                    String errorcode = cmd.Parameters["errcode"].Value.ToString();

                    using (OracleDataReader sdr = (OracleDataReader)cmd.Parameters["channel_Cursor"].Value)
                    {
                        while (sdr.Read())
                        {
                            AvailableItems.Add(new channel()
                            {
                                ID = sdr[0].ToString(),
                                Name = sdr[1].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return AvailableItems;
        }
        public List<UsersMangementViewModel> GetCustomerLog(String UserName, String loginType)
        {
            List<UsersMangementViewModel> userLog = new List<UsersMangementViewModel>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "";

                if (loginType.Equals("1"))
                {
                    query = "select ipaddress,login_time,user_login,user_pass,decode(login_status,'F','Failed','S','Succesful','unknown'), user_id from admin_login where user_login = '" + UserName + "'";

                }
                else if (loginType.Equals("2"))
                {
                    query = "select ipaddress,login_time,user_login,user_pass,decode(login_status,'F','Failed','S','Succesful','unknown'), user_id from admin_login where user_login = '" + UserName + "' and login_status = 'S'";

                }
                else
                {
                    query = "select ipaddress,login_time,user_login,user_pass,decode(login_status,'F','Failed','S','Succesful','unknown'), user_id from admin_login where user_login = '" + UserName + "' and login_status = 'F'";

                }

                //query = "select last_log_ip,last_login,user_log,decode(user_status,'A','Active','D','Deactive','DE','Deleted','U','Unauthorized','P','Pending'),decode(catogry,'1','Personal','2','Operator','3','Authorizor'), user_id from users";
                if (con.State == ConnectionState.Closed)
                { con.Open(); }

                OracleCommand cmd = new OracleCommand(query, con);

                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {


                        userLog.Add(new UsersMangementViewModel
                        {
                            IpAddress = dr[0].ToString(),
                            LoginTime = dr[1].ToString(),
                            //user_id = Convert.ToInt32(dr[1].ToString()),
                            UserLogin = dr[2].ToString(),
                            UserPass = dr[3].ToString(),
                            LoginStatus = dr[4].ToString(),
                            UserID = dr[5].ToString(),

                        });
                    }
                }


            }
            return userLog;
        }

        public List<UsersMangementViewModel> GetCustomersLog()
        {
            List<UsersMangementViewModel> userLog = new List<UsersMangementViewModel>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select user_log,last_login,decode(user_status,'A','Active','D','Deactive','DE','Deleted','U','Unauthorized','P','Pending','N/A'),decode(catogry,'1','Personal','2','Operator','3','Authorizor','N/A'),last_log_ip,user_id from users";
                if (con.State == ConnectionState.Closed)
                { con.Open(); }
                OracleCommand cmd = new OracleCommand(query, con);
                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        userLog.Add(new UsersMangementViewModel
                        {
                            Username = dr[0].ToString(),
                            LoginTime = dr[1].ToString(),
                            userstatus = dr[2].ToString(),
                            category = dr[3].ToString(),
                            IpAddress = dr[4].ToString(),
                            UserID = dr[5].ToString(),
                        });
                    }
                }
            }
            return userLog;
        }

        internal int insertuser(string p1, string p2, string p3, string p4)
        {
            throw new NotImplementedException();
        }
        //--------------------------GET UserLog------------
        public List<UsersMangementViewModel> GetUserLog(String UserName, String loginType)
        {
            List<UsersMangementViewModel> userLog = new List<UsersMangementViewModel>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "";

                if (loginType.Equals("1"))
                {
                    query = "select ipaddress,login_time,user_login,user_pass,decode(login_status,'F','Failed','S','Succesful','unknown') from users_login where user_login = '" + UserName + "'";

                }
                else if (loginType.Equals("2"))
                {
                    query = "select ipaddress,login_time,user_login,user_pass,decode(login_status,'F','Failed','S','Succesful','unknown') from users_login where user_login = '" + UserName + "' and login_status = 'S'";

                }
                else
                {
                    query = "select ipaddress,login_time,user_login,user_pass,decode(login_status,'F','Failed','S','Succesful','unknown') from users_login where user_login = '" + UserName + "' and login_status = 'F'";

                }

                if (con.State == ConnectionState.Closed)
                { con.Open(); }

                OracleCommand cmd = new OracleCommand(query, con);

                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {


                        userLog.Add(new UsersMangementViewModel
                        {
                            IpAddress = dr[0].ToString(),
                            LoginTime = dr[1].ToString(),
                            //user_id = Convert.ToInt32(dr[1].ToString()),
                            UserLogin = dr[2].ToString(),
                            UserPass = dr[3].ToString(),
                            LoginStatus = dr[4].ToString(),

                        });
                    }
                }


            }
            return userLog;
        }

        //---------------------------------GetCustomerIDFromAccountNumber------------------------------------------
        /// <summary>
        /// /It Gets Customer Full Account Number
        /// and Returns the ID
        /// </summary>
        /// <param name="AccountNumber"></param>
        /// <returns>CustID</returns>
        public String getCustIDFromAcc(string AccountNumber)
        {
            int CustID = 0;
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select user_id from users where account = " + AccountNumber;

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();


                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {

                        if (dataReader["user_id"] != DBNull.Value)
                        {

                            CustID = Convert.ToInt32(dataReader["user_id"]);

                        }
                    }
                    //Accounts = Accounts.Substring(1);
                    return CustID.ToString();

                }

            }

        }

        //----------------------------------------------GetTransferReport---------------------------------------------------------
        /// <summary>
        /// GetTransferReport
        /// </summary>
        /// <param name="custId"></param>
        /// <returns>List of Requests and response</returns>
        public List<CustomerTransferReportViewModel> GetTransferReport(string custId)
        {
            List<CustomerTransferReportViewModel> items = new List<CustomerTransferReportViewModel>();
            using (OracleConnection con = new OracleConnection(conString))
            {

                string query = " SELECT tran_req_date,tran_req,tran_resp,tran_resp_result,TRAN_STATUS from trans_log WHERE" +
                               " tran_name in('Own Transfer','To Bank Customer Transfer','To Counter Transfer') AND user_id = " + custId;

                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string[] reqString = null;
                            items.Add(new CustomerTransferReportViewModel
                            {
                                TranDate = dr["tran_req_date"].ToString(),
                                TranFullReq = dr["tran_req"].ToString(),
                                TranFullResp = dr["tran_resp"].ToString(),
                                TranResult = dr["tran_resp_result"].ToString(),
                                TranStatus = dr["TRAN_STATUS"].ToString(),

                            });
                        }
                        con.Close();
                    }
                }
            }

            return items;
        }

        public List<SelectListItem> PopulateBranchs(string branchcode, string idorusername)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //char[] chararray = idorusername.ToCharArray();
            //if (char.IsDigit(chararray[0]) && idorusername.Length == 12)
            //{
            //    using (OracleConnection con = new OracleConnection(conString))
            //    {
            //        string query = " select distinct branchs.branch_code,branchs.branch_name from branchs left outer join users on branchs.branch_code = SUBSTR(users.def_acc,3,3) where branchs.branch_sts = '1' and branchs.BRANCH_CODE_NO ='" + branchcode + "' and user_log = '" + idorusername + "'";
            //        using (OracleCommand cmd = new OracleCommand(query))
            //        {
            //            cmd.Connection = con;
            //            con.Open();
            //            using (OracleDataReader sdr = cmd.ExecuteReader())
            //            {
            //                if (sdr.HasRows)
            //                {
            //                    while (sdr.Read())
            //                    {
            //                        items.Add(new SelectListItem
            //                        {
            //                            Text = sdr["branch_name"].ToString(),
            //                            Value = sdr["branch_code"].ToString()
            //                        });
            //                    }
            //                }
            //            }
            //            con.Close();
            //        }
            //    }
            //}
            //else if (char.IsDigit(chararray[0]))
            //{
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select distinct branchs.branch_code,branchs.branch_name from branchs left outer join users on branchs.branch_code = SUBSTR(users.def_acc,3,3) where branchs.branch_sts = '1' and branchs.BRANCH_CODE_NO ='" + branchcode + "' and user_log = '" + idorusername + "' or user_mobile = '" + idorusername + "' or SUBSTR(users.def_acc,14,5) = '" + idorusername + "'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr.HasRows)
                        {
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["branch_name"].ToString(),
                                    Value = sdr["branch_code"].ToString()
                                });
                            }
                        }
                    }
                    con.Close();
                }
            }
            //}
            //else
            //{
            //    using (OracleConnection con = new OracleConnection(conString))
            //    {
            //        string query = " select distinct branchs.branch_code,branchs.branch_name from branchs left outer join users on branchs.branch_code = SUBSTR(users.def_acc,3,3) where branchs.branch_sts = '1' and branchs.BRANCH_CODE_NO ='" + branchcode + "' and user_log = '" + idorusername + "'";
            //        using (OracleCommand cmd = new OracleCommand(query))
            //        {
            //            cmd.Connection = con;
            //            con.Open();
            //            using (OracleDataReader sdr = cmd.ExecuteReader())
            //            {
            //                if (sdr.HasRows)
            //                {
            //                    while (sdr.Read())
            //                    {
            //                        items.Add(new SelectListItem
            //                        {
            //                            Text = sdr["branch_name"].ToString(),
            //                            Value = sdr["branch_code"].ToString()
            //                        });
            //                    }
            //                }
            //            }
            //            con.Close();
            //        }
            //    }
            //}

            return items;
        }

        public List<SelectListItem> PopulateAccountTypes(string idorusername)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //char[] chararray = idorusername.ToCharArray();
            //if (char.IsDigit(chararray[0]) && idorusername.Length == 12)
            //{
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select act_type_code,act_name from Act_types inner join users on act_type_code = SUBSTR(def_acc,6,5) and user_log = '" + idorusername + "' or user_mobile = '" + idorusername + "' or  SUBSTR(users.def_acc,14,5) = '" + idorusername + "'";
                using (OracleCommand cmd = new OracleCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (OracleDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Insert(0, new SelectListItem
                            {
                                Text = sdr["act_name"].ToString(),
                                Value = sdr["act_type_code"].ToString(),
                            });
                        }
                    }
                    con.Close();
                }
            }
            //}
            //else if (char.IsDigit(chararray[0]))
            //{
            //    using (OracleConnection con = new OracleConnection(conString))
            //    {
            //        string query = "select act_type_code,act_name from Act_types inner join users on act_type_code = SUBSTR(def_acc,6,6) and user_id = '" + Convert.ToInt64(idorusername) + "'";
            //        using (OracleCommand cmd = new OracleCommand(query))
            //        {
            //            cmd.Connection = con;
            //            con.Open();
            //            using (OracleDataReader sdr = cmd.ExecuteReader())
            //            {
            //                while (sdr.Read())
            //                {
            //                    items.Insert(0, new SelectListItem
            //                    {
            //                        Text = sdr["act_name"].ToString(),
            //                        Value = sdr["act_type_code"].ToString(),
            //                    });
            //                }
            //            }
            //            con.Close();
            //        }
            //    }
            //}
            //else
            //{
            //    using (OracleConnection con = new OracleConnection(conString))
            //    {
            //        string query = "select act_type_code,act_name from Act_types inner join users on act_type_code = SUBSTR(def_acc,6,6) and user_log = '" + idorusername + "'";
            //        using (OracleCommand cmd = new OracleCommand(query))
            //        {
            //            cmd.Connection = con;
            //            con.Open();
            //            using (OracleDataReader sdr = cmd.ExecuteReader())
            //            {
            //                while (sdr.Read())
            //                {
            //                    items.Insert(0, new SelectListItem
            //                    {
            //                        Text = sdr["act_name"].ToString(),
            //                        Value = sdr["act_type_code"].ToString(),
            //                    });
            //                }
            //            }
            //            con.Close();
            //        }
            //    }
            //}



            return items;
        }

        public CustomerRegBankinfo GetUserRegistrationData(string idorname)
        {
            CustomerRegBankinfo usermodel = new CustomerRegBankinfo();
            if (idorname != null)
            {
                //char[] chararray = idorname.ToCharArray();
                //if (char.IsDigit(chararray[0]) && idorname.Length == 12)
                //{
                using (OracleConnection con = new OracleConnection(conString))
                {
                    string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_name, SUBSTR(def_acc,6,5) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,5)) as account_type,SUBSTR(def_acc,14,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name ,(select user_name from users where user_log = '" + idorname + "' or user_mobile = '" + idorname + "' or SUBSTR(users.def_acc,14,5) = '" + idorname + "') as user_name from users where user_log = '" + idorname + "' or user_mobile = '" + idorname + "' or SUBSTR(users.def_acc,14,5) = '" + idorname + "'";

                    OracleCommand cmd = new OracleCommand(query, con);
                    con.Open();
                    using (IDataReader dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            usermodel.BranchCode = dataReader["branch_code"].ToString();
                            usermodel.Branch = dataReader["branch_name"].ToString();
                            usermodel.CurrencyCode = dataReader["currency_code"].ToString();
                            usermodel.Currency = dataReader["currency_name"].ToString();
                            usermodel.AccountTypecode = dataReader["account_type_code"].ToString();
                            usermodel.AccountType = dataReader["account_type"].ToString();
                            usermodel.AccountNumber = dataReader["account_number"].ToString();
                            usermodel.CategoryCode = dataReader["category_id"].ToString();
                            usermodel.category = dataReader["category_name"].ToString();
                            usermodel.CustomerName = dataReader["user_name"].ToString();
                            usermodel.CustomerID = idorname;
                        }
                    }
                    return usermodel;
                }
                //}
                //else if (char.IsDigit(chararray[0]))
                //{
                //    using (OracleConnection con = new OracleConnection(conString))
                //    {
                //        string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,21,2)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,21,2)) as currency_name, SUBSTR(def_acc,6,6) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,6)) as account_type,SUBSTR(def_acc,12,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name,SUBSTR(def_acc,19,2) as subno, SUBSTR(def_acc,23,3) as subgl from users where user_id = '" + idorname + "'";

                //        OracleCommand cmd = new OracleCommand(query, con);
                //        con.Open();
                //        using (IDataReader dataReader = cmd.ExecuteReader())
                //        {
                //            while (dataReader.Read())
                //            {
                //                usermodel.BranchCode = dataReader["branch_code"].ToString();
                //                usermodel.Branch = dataReader["branch_name"].ToString();
                //                usermodel.CurrencyCode = dataReader["currency_code"].ToString();
                //                usermodel.Currency = dataReader["currency_name"].ToString();
                //                usermodel.AccountTypecode = dataReader["account_type_code"].ToString();
                //                usermodel.AccountType = dataReader["account_type"].ToString();
                //                usermodel.AccountNumber = dataReader["account_number"].ToString();
                //                usermodel.CategoryCode = dataReader["category_id"].ToString();
                //                usermodel.category = dataReader["category_name"].ToString();
                //                usermodel.SUBNO = dataReader["subno"].ToString();
                //                usermodel.SUBGL = dataReader["subgl"].ToString();
                //                usermodel.CustomerID = idorname;
                //            }
                //        }
                //        return usermodel;
                //    }
                //}
                //else
                //{
                //    using (OracleConnection con = new OracleConnection(conString))
                //    {
                //        //string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_name, SUBSTR(def_acc,6,6) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,6)) as account_type,SUBSTR(def_acc,12,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name from users where user_log = '" + idorname + "'";
                //        string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,21,2)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,21,2)) as currency_name, SUBSTR(def_acc,6,6) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,6)) as account_type,SUBSTR(def_acc,12,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name,SUBSTR(def_acc,19,2) as subno, SUBSTR(def_acc,23,3) as subgl from users where user_log = '" + idorname + "'";
                //        OracleCommand cmd = new OracleCommand(query, con);
                //        con.Open();
                //        using (IDataReader dataReader = cmd.ExecuteReader())
                //        {
                //            while (dataReader.Read())
                //            {
                //                usermodel.BranchCode = dataReader["branch_code"].ToString();
                //                usermodel.Branch = dataReader["branch_name"].ToString();
                //                usermodel.CurrencyCode = dataReader["currency_code"].ToString();
                //                usermodel.Currency = dataReader["currency_name"].ToString();
                //                usermodel.AccountTypecode = dataReader["account_type_code"].ToString();
                //                usermodel.AccountType = dataReader["account_type"].ToString();
                //                usermodel.AccountNumber = dataReader["account_number"].ToString();
                //                usermodel.CategoryCode = dataReader["category_id"].ToString();
                //                usermodel.category = dataReader["category_name"].ToString();
                //                usermodel.SUBNO = dataReader["subno"].ToString();
                //                usermodel.SUBGL = dataReader["subgl"].ToString();
                //                usermodel.CustomerID = idorname;
                //            }
                //        }
                //        return usermodel;
                //    }
                //}
            }
            return usermodel;
        }

        public CustomerTransferReportViewModel GetUserReportData(string idorname)
        {
            CustomerTransferReportViewModel usermodel = new CustomerTransferReportViewModel();
            char[] chararray = idorname.ToCharArray();
            if (char.IsDigit(chararray[0]))
            {
                using (OracleConnection con = new OracleConnection(conString))
                {
                    //string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_name, SUBSTR(def_acc,6,5) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,5)) as account_type,SUBSTR(def_acc,11,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name from users where user_id = '" + int.Parse(idorname) + "'";
                    string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code,(select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name,(select curr_code from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_code,(select curr_name from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_name, SUBSTR(def_acc,6,5) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,5)) as account_type,SUBSTR(def_acc,14,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name from users where user_id = '" + idorname + "' or user_mobile = '" + idorname + "' or  SUBSTR(users.def_acc,14,7) = '" + idorname + "' or user_log = '" + idorname + "'";
                    OracleCommand cmd = new OracleCommand(query, con);
                    con.Open();
                    using (IDataReader dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            usermodel.BranchCode = dataReader["branch_code"].ToString();
                            usermodel.Branch = dataReader["branch_name"].ToString();
                            usermodel.CurrencyCode = dataReader["currency_code"].ToString();
                            usermodel.Currency = dataReader["currency_name"].ToString();
                            usermodel.AccountTypecode = dataReader["account_type_code"].ToString();
                            usermodel.AccountType = dataReader["account_type"].ToString();
                            usermodel.AccountNumber = dataReader["account_number"].ToString();
                            usermodel.CustomerID = idorname;
                        }
                    }
                    return usermodel;
                }
            }
            else
            {
                using (OracleConnection con = new OracleConnection(conString))
                {
                    string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,21,2)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,21,2)) as currency_name, SUBSTR(def_acc,6,6) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,6)) as account_type,SUBSTR(def_acc,12,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name,SUBSTR(def_acc,19,2) as subno, SUBSTR(def_acc,23,3) as subgl from users where user_log = '" + idorname + "'";
                    //string query = "select (select branch_code from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_code, (select branch_name from branchs where branch_code = SUBSTR(users.def_acc,3,3)) as branch_name, (select curr_code from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_code, (select curr_name from currency where curr_code =  SUBSTR(def_acc,11,3)) as currency_name, SUBSTR(def_acc,6,5) as account_type_code,(select act_name from act_types where ACT_TYPE_CODE = SUBSTR(def_acc,6,5)) as account_type,SUBSTR(def_acc,11,7) as account_number, (select cat_id from category where cat_id = users.catogry) as category_id,(select cat_name from category where cat_id = users.catogry) as category_name from users where user_log = '" + idorname + "'";
                    OracleCommand cmd = new OracleCommand(query, con);
                    con.Open();
                    using (IDataReader dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            usermodel.BranchCode = dataReader["branch_code"].ToString();
                            usermodel.Branch = dataReader["branch_name"].ToString();
                            usermodel.CurrencyCode = dataReader["currency_code"].ToString();
                            usermodel.Currency = dataReader["currency_name"].ToString();
                            usermodel.AccountTypecode = dataReader["account_type_code"].ToString();
                            usermodel.AccountType = dataReader["account_type"].ToString();
                            usermodel.AccountNumber = dataReader["account_number"].ToString();
                            usermodel.SUBNO = dataReader["SUBNO"].ToString();
                            usermodel.SUBGL = dataReader["SUBGL"].ToString();
                            usermodel.CustomerID = idorname;
                        }
                    }
                    return usermodel;
                }
            }
        }



        public List<Servielist> GetAllServices()
        {
            List<Servielist> service = new List<Servielist>();
            using (OracleConnection con = new OracleConnection(conString))
            {
                OracleCommand cmd = new OracleCommand("select service_id,service_name,service_code,decode(service_status,'A','Active','DE','Delete','Unknown') from SERVICES where service_status='A' ", con);
                if (con.State == ConnectionState.Closed)
                { con.Open(); }


                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {


                        service.Add(new Servielist
                        {
                            service_id = Convert.ToInt32(dr[0].ToString()),

                            name = dr[1].ToString(),
                            service_code = dr[2].ToString(),
                            service_status = dr[3].ToString()
                        });
                    }
                }


            }
            return service;
        }

        public List<ActionsLogViewModel> getactionslog()
        {
            List<ActionsLogViewModel> actions = new List<ActionsLogViewModel>();

            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = "select * from admins_log";
                OracleCommand cmd = new OracleCommand(query, con);
                con.Open();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        ActionsLogViewModel action = new ActionsLogViewModel();
                        action.user_id = dataReader["USER_ID"].ToString();
                        action.user_name = dataReader["USERNAME"].ToString();
                        action.user_role = dataReader["USER_ROLE"].ToString();
                        action.user_status = dataReader["USER_STATUS"].ToString();
                        action.action = dataReader["ACTION"].ToString();
                        action.action_on_user = dataReader["ACTION_ON_USER"].ToString();
                        action.timedate = dataReader["TIMEDATE"].ToString();
                        action.user_branch = dataReader["USER_BRANCH"].ToString();

                        actions.Add(action);
                    }
                }
            }
            return actions;
        }

        //-----------------------------------getTransactions---------------------
        /// <summary>
        /// Get top 5 Transactions
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public List<LatestTransactions> getTransactions(string user_id)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                //string query = "select tran_id,tran_name,tran_status,tran_resp_result from trans_log where user_id =" + user_id + "and ROWNUM <= 5 order by rownum desc";
                string query =
                    "select * from ( select tran_id , tran_name,tran_status,tran_resp_result from trans_log where user_id = '" +
                    user_id + "' order by tran_id desc ) where rownum <= 5";


                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();

                List<LatestTransactions> list = new List<LatestTransactions>();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        LatestTransactions obj = new LatestTransactions();

                        if (dataReader["tran_id"] != DBNull.Value)
                        {
                            //obj.AccountID = (int)dataReader["acc_id"];
                            obj.TranId = Convert.ToInt32(dataReader["tran_id"]);
                        }
                        if (dataReader["tran_name"] != DBNull.Value)
                        {
                            obj.TranName = (string)dataReader["tran_name"];
                        }
                        if (dataReader["tran_status"] != DBNull.Value)
                        {
                            obj.TranStatus = (string)dataReader["tran_status"];
                        }
                        if (dataReader["tran_resp_result"] != DBNull.Value)
                        {
                            obj.TranResult = (string)dataReader["tran_resp_result"];
                        }
                        list.Add(obj);
                    }

                    return list;

                }

            }

        }



        //-----------------------GET Transfer Count------------------------------------------------------
        //
        public String GetTransferCount(string user_id)
        {
            String count = "NULL";
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select count(*) from trans_log where    (tran_name = 'Own Transfer' or tran_name = 'To Bank Customer Transfer')";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();


                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {



                        if (dataReader["count(*)"] != DBNull.Value)
                        {
                            count = dataReader["count(*)"].ToString();
                        }

                    }
                }

                return count;

            }
        }

        public String GetFailedCount(string user_id)
        {
            String count = "NULL";
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select count(*) from trans_log where tran_status like '%Failed%' ";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();


                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {



                        if (dataReader["count(*)"] != DBNull.Value)
                        {
                            count = dataReader["count(*)"].ToString();
                        }

                    }
                }

                return count;

            }
        }
        public String GetSecussfullyCount(string user_id)
        {
            String count = "NULL";
            using (OracleConnection con = new OracleConnection(conString))
            {
                string query = " select count(*) from trans_log where tran_status like '%Secussfully%' ";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();


                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {



                        if (dataReader["count(*)"] != DBNull.Value)
                        {
                            count = dataReader["count(*)"].ToString();
                        }

                    }
                }

                return count;

            }
        }

        //-----------------------GET Accounts Count------------------------------------------------------
        //
        public String GetAccountsCount(string branchcode)
        {
            String count = "NULL", query;
            using (OracleConnection con = new OracleConnection(conString))
            {
                if (!branchcode.Equals("000"))
                {
                    query = " SELECT count(*) from user_acc_link where branch_code = " + branchcode;
                }
                else
                {
                    query = " SELECT count(*) from user_acc_link ";
                }
                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();


                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {



                        if (dataReader["count(*)"] != DBNull.Value)
                        {
                            count = dataReader["count(*)"].ToString();
                        }

                    }
                }

                return count;

            }
        }

        public String GetUsersCount(string branchcode)
        {
            String count = "NULL", query;
            using (OracleConnection con = new OracleConnection(conString))
            {
                if (!branchcode.Equals("000"))
                {
                    query = " SELECT count(*) from users where substr(def_acc,3,3)= " + branchcode;
                }
                else
                {
                    query = " SELECT count(*) from users ";
                }
                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();


                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {



                        if (dataReader["count(*)"] != DBNull.Value)
                        {
                            count = dataReader["count(*)"].ToString();
                        }

                    }
                }

                return count;

            }
        }

        public UserDetailsModel GetUserDetails(string IdOrName)
        {
            UserDetailsModel usermodel = new UserDetailsModel();
            string operatorname = IdOrName + "O";
            string authorizorname = IdOrName + "A";
            using (OracleConnection con = new OracleConnection(conString))
            {

                string query = "select * from users where user_log = '" + IdOrName + "' or user_log = '" + operatorname + "' or user_log = '" + authorizorname + "'";
                //string query = "select * from users where user_id = '" + int.Parse(IdOrName) + "'";
                OracleCommand cmd = new OracleCommand(query, con);
                con.Open();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        usermodel.user_id = int.Parse(dataReader["user_id"].ToString());
                        usermodel.user_name = dataReader["user_name"].ToString();
                        usermodel.user_log = dataReader["user_log"].ToString();
                        usermodel.user_email = dataReader["user_email"].ToString();
                        usermodel.user_mobile = dataReader["user_mobile"].ToString();
                        usermodel.user_fax = dataReader["user_fax"].ToString();
                        usermodel.user_address = dataReader["user_adrs"].ToString();
                        usermodel.user_status = dataReader["user_status"].ToString();
                        usermodel.defult_account = dataReader["def_acc"].ToString();
                        usermodel.last_login = dataReader["last_login"].ToString();
                        usermodel.last_login_ip = dataReader["last_log_ip"].ToString();
                        usermodel.faild_login = int.Parse(dataReader["faild_logins"].ToString());
                        usermodel.first_login = dataReader["first_login"].ToString();
                        usermodel.category = int.Parse(dataReader["catogry"].ToString());
                        usermodel.user_transfer = dataReader["user_transfer"].ToString();
                        usermodel.role_id = int.Parse(dataReader["roleid"].ToString());
                        usermodel.account = dataReader["account"].ToString();
                        usermodel.active = dataReader["active"].ToString();
                        usermodel.last_unssessful_login = dataReader["last_unsuccess_login"].ToString();
                        usermodel.company_name = dataReader["company_name"].ToString();
                        usermodel.user_custid = dataReader["user_custid"].ToString();
                        usermodel.login_status = dataReader["login_status"].ToString();
                    }
                }
                return usermodel;
            }
        }


        //---------------------Get Transfers Only---------------------------------
        /// <summary>
        ///
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public List<AllTransfersViewModel> getMyTransfers(string user_id)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                //string query = "select tran_id,tran_name,tran_status,tran_resp_result from trans_log where user_id =" + user_id + "and ROWNUM <= 5 order by rownum desc";
                //string query =
                //    "select * from ( select tran_id , tran_name,tran_status,tran_resp_result from trans_log where user_id = '" +
                //    user_id + "' order by tran_id desc ) where rownum <= 5";

                string query = "select tran_id, tran_name, tran_status, tran_resp_result from trans_log where  user_id =" + user_id +
                    " and (tran_name = 'Own Transfer' or tran_name = 'To Bank Customer Transfer') order by tran_id desc";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();

                List<AllTransfersViewModel> list = new List<AllTransfersViewModel>();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        AllTransfersViewModel obj = new AllTransfersViewModel();

                        if (dataReader["tran_id"] != DBNull.Value)
                        {
                            //obj.AccountID = (int)dataReader["acc_id"];
                            obj.TranId = Convert.ToInt32(dataReader["tran_id"]);
                        }
                        if (dataReader["tran_name"] != DBNull.Value)
                        {
                            obj.TranName = (string)dataReader["tran_name"];
                        }
                        if (dataReader["tran_status"] != DBNull.Value)
                        {
                            obj.TranStatus = (string)dataReader["tran_status"];
                        }
                        if (dataReader["tran_resp_result"] != DBNull.Value)
                        {
                            obj.TranResult = (string)dataReader["tran_resp_result"];
                        }
                        list.Add(obj);
                    }

                    return list;

                }

            }

        }
        public List<CustomerAccounts> Custcounts(String bracode, String user_id)
        {
            OracleCommand cmd;
            OracleDataReader dr;

            String userid, username, useract, newuseract, newuseractcomplete;
            String query1, result;

            List<CustomerAccounts> customer = new List<CustomerAccounts>();

            if (!bracode.Equals("000"))
            {
                query1 = "select user_acc_link.user_id,user_name,b.branch_name||'-'||t.act_name||'-'||c.curr_name||'-'||SUBSTR(def_acc,14) def_acc,(select branch_name  from branchs where branch_code =SUBSTR(ACC_NO,3,3))||'-'||(select act_name  from act_types where act_type_code =SUBSTR(ACC_NO,6,5))||'-'||(select curr_name  from currency where  CURR_STS='1' and  CURR_CODE =SUBSTR(ACC_NO,11,3))||'-'||SUBSTR(ACC_NO,14) ,ACC_NO from users , user_acc_link ,branchs b ,act_types t , currency c    where     SUBSTR(def_acc,3,3)=b.branch_code and   SUBSTR(def_acc,6,5)=t.act_type_code and SUBSTR(def_acc,11,3)=c.CURR_CODE and   user_acc_link.user_id='" + user_id + "'and  and user_acc_link.user_id=users.user_id  and substr(def_acc,3,3)='" + bracode + "' order by user_id";
            }
            else
            {
                query1 = "select user_acc_link.user_id,user_name,b.branch_name||'-'||t.act_name||'-'||c.curr_name||'-'||SUBSTR(def_acc,14) def_acc,(select branch_name  from branchs where branch_code =SUBSTR(ACC_NO,3,3))||'-'||(select act_name  from act_types where act_type_code =SUBSTR(ACC_NO,6,5))||'-'||(select curr_name  from currency where  CURR_STS='1' and CURR_CODE =SUBSTR(ACC_NO,11,3))||'-'||SUBSTR(ACC_NO,14), ACC_NO from users , user_acc_link ,branchs b ,act_types t , currency c    where     SUBSTR(def_acc,3,3)=b.branch_code and   SUBSTR(def_acc,6,5)=t.act_type_code and SUBSTR(def_acc,11,3)=c.CURR_CODE and   user_acc_link.user_id='" + user_id + "'and   ACC_STATUS='A' and user_acc_link.user_id=users.user_id order by user_id";
            }

            using (OracleConnection con = new OracleConnection(conString))
            {
                cmd = new OracleCommand(query1, con);

                con.Open();

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        userid = dr[0].ToString();
                        username = dr[1].ToString();
                        useract = dr[2].ToString();
                        newuseract = dr[3].ToString();
                        newuseractcomplete = dr[4].ToString();

                        customer.Add(new CustomerAccounts
                        {
                            USER_ID = userid,
                            USER_NAME = username,
                            DEF_ACC = useract,
                            ACC_NO = newuseract,
                            ACC_NO1 = newuseractcomplete
                        });
                    }
                }


            }


            return customer;
        }


        //---------------------Get All Transactions---------------------------------
        public List<AllTransfersViewModel> getAllTransactions(string user_id)
        {
            using (OracleConnection con = new OracleConnection(conString))
            {
                //string query = "select tran_id,tran_name,tran_status,tran_resp_result from trans_log where user_id =" + user_id + "and ROWNUM <= 5 order by rownum desc";
                //string query =
                //    "select * from ( select tran_id , tran_name,tran_status,tran_resp_result from trans_log where user_id = '" +
                //    user_id + "' order by tran_id desc ) where rownum <= 5";

                string query = "select tran_id, tran_name, tran_status, tran_resp_result from trans_log where  user_id =" + user_id +
                               "order by tran_id desc";

                OracleCommand cmd = new OracleCommand(query, con);

                con.Open();

                List<AllTransfersViewModel> list = new List<AllTransfersViewModel>();
                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        AllTransfersViewModel obj = new AllTransfersViewModel();

                        if (dataReader["tran_id"] != DBNull.Value)
                        {
                            //obj.AccountID = (int)dataReader["acc_id"];
                            obj.TranId = Convert.ToInt32(dataReader["tran_id"]);
                        }
                        if (dataReader["tran_name"] != DBNull.Value)
                        {
                            obj.TranName = (string)dataReader["tran_name"];
                        }
                        if (dataReader["tran_status"] != DBNull.Value)
                        {
                            obj.TranStatus = (string)dataReader["tran_status"];
                        }
                        if (dataReader["tran_resp_result"] != DBNull.Value)
                        {
                            obj.TranResult = (string)dataReader["tran_resp_result"];
                        }
                        list.Add(obj);
                    }

                    return list;

                }

            }

        }
    }//class
    public class Encryptor
    {

        public static string v;

        private static TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();

        private static char[] ekey = "jaaaoiuyndfghjytewsdaaaa".ToCharArray();

        private static byte[] Key
        {
            get
            {
                return Encoding.Default.GetBytes(ekey);
                // Return Encoding.Default.GetBytes(WindowsIdentity.GetCurrent.Name.PadRight(24, Chr(0)))
            }
        }

        public static byte[] Vector
        {
            get
            {
                return Encoding.Default.GetBytes("fjhksjf9iufjsoifhihfsgdsgsg");
            }
        }

        public static string Encrypt(string Text)
        {
            return Encryptor.Transform(Text, des.CreateEncryptor(Key, Vector));
        }

        public static string Decrypt(string encryptedText)
        {
            return Encryptor.Transform(encryptedText, des.CreateDecryptor(Key, Vector));
        }

        private static string Transform(string Text, ICryptoTransform CryptoTransform)
        {
            MemoryStream stream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(stream, CryptoTransform, CryptoStreamMode.Write);
            byte[] Input = Encoding.Default.GetBytes(Text);
            cryptoStream.Write(Input, 0, Input.Length);
            cryptoStream.FlushFinalBlock();
            return Encoding.Default.GetString(stream.ToArray());
        }
    }
}
