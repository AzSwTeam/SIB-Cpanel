using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFBCPanel.Models;
using ModelState = System.Web.Mvc.ModelState;

namespace SFBCPanel.Context
{

    public class Connecttocore
    {
        //Base Url
        public static string BASE_URL = "";
        public static string configip = null, configport = null, configpath = null;

        public static void getconfig()
        {
            try
            {
                // using (StreamReader sr = new StreamReader("Z:\\Projects\\sibcpanel\\SFBCPanel\\Configuration\\SIBconfiguration.txt"))
                //using (StreamReader sr = new StreamReader("C:\\Users\\smah\\Desktop\\web IB\\sibcpanel\\SFBCPanel\\Configuration\\SIBconfiguration.txt"))
                
                using (StreamReader sr = new StreamReader("C:\\inetpub\\wwwroot\\SSBCPanel\\Configuration\\SIBconfiguration.txt"))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        configip = line;
                        configport = sr.ReadLine();
                        configpath = sr.ReadLine();
                        BASE_URL = configip + ":" + configport + "/" + configpath;
                    }
                }
            }
            catch (Exception e)
            {
                String s = e.Message;
            }
        }

        public static string GetHeartBeat()
        {
            getconfig();
            Uri requestUri = new Uri(BASE_URL + "/HeartBeat");
            dynamic dynamicJson = new ExpandoObject();

            //dynamicJson.Authentication = "Card";
            //dynamicJson.ChannelID = "InternetBanking";
            //dynamicJson.lang = "1";
            //dynamicJson.uuid = Guid.NewGuid();

            string json = "";
            json = JsonConvert.SerializeObject(dynamicJson);
            var responJsonText = "";
            JObject JResp = new JObject();

            using (var objClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage respon = objClient
                        .PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json")).Result;

                    if (respon.IsSuccessStatusCode)
                    {
                        responJsonText = respon.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception e)
                {
                    responJsonText = "Error";
                }

                return responJsonText;

            }

        }

        public string sendotp(string userid, string sms, string phonenumber)
        {
            getconfig();

            Uri requestUri = new Uri(BASE_URL + "/SendSMS");

            dynamic dynamicJson = new ExpandoObject();
            dynamicJson.Authentication = "Card";
            dynamicJson.Channel = "InternetBanking";
            dynamicJson.userid = userid;//"130042010593883".ToString();
            dynamicJson.SMS = sms;//"10";
            dynamicJson.PhoneNumber = phonenumber;//"10";
            dynamicJson.flag = "Internetbanking";//"10";
            dynamicJson.lang = 1;
            dynamicJson.uuid = Guid.NewGuid();

            string json = "";
            json = JsonConvert.SerializeObject(dynamicJson);
            var responJsonText = "";
            JObject JResp = new JObject();

            using (var objClient = new HttpClient())
            {
                try
                {

                    HttpResponseMessage respon = objClient
                        .PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json")).Result;

                    if (respon.IsSuccessStatusCode)
                    {
                        responJsonText = respon.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // throw;
                }

                return responJsonText;

            }
        }

        public static string GetCustinfo(string account)
        {
            getconfig();
            //Uri requestUri = new Uri(BASE_URL + "/GetCustInfoByID");
            Uri requestUri = new Uri(BASE_URL + "/GetCustinfo");
            dynamic dynamicJson = new ExpandoObject();

            //dynamicJson.CustID = cif;//"1300420105s93883".ToString();
            dynamicJson.account = account;
            dynamicJson.Authentication = "Card";
            dynamicJson.Channel = "InternetBanking";
            dynamicJson.lang = "1";
            dynamicJson.uuid = Guid.NewGuid();

            string json = "";
            json = JsonConvert.SerializeObject(dynamicJson);
            var responJsonText = "";
            JObject JResp = new JObject();

            using (var objClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage respon = objClient
                        .PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json")).Result;

                    if (respon.IsSuccessStatusCode)
                    {
                        responJsonText = respon.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception e)
                {
                    responJsonText = "Error";
                }

                return responJsonText;

            }

        }

        public static string GetStatus(string serviceCode , string tran_id)
        {
            getconfig();
            //Uri requestUri = new Uri(BASE_URL + "/GetCustInfoByID");
            Uri requestUri = new Uri(BASE_URL + "/GetStatus");
            dynamic dynamicJson = new ExpandoObject();

            //dynamicJson.CustID = cif;//"1300420105s93883".ToString();
            dynamicJson.serviceCode = serviceCode;
            dynamicJson.origionalSourceTransactionId = tran_id;
            dynamicJson.Authentication = "Card";
            dynamicJson.Channel = "InternetBanking";
            dynamicJson.lang = "1";
            dynamicJson.uuid = Guid.NewGuid();

            string json = "";
            json = JsonConvert.SerializeObject(dynamicJson);
            var responJsonText = "";
            JObject JResp = new JObject();

            using (var objClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage respon = objClient
                        .PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json")).Result;

                    if (respon.IsSuccessStatusCode)
                    {
                        responJsonText = respon.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception e)
                {
                    responJsonText = "Error";
                }

                return responJsonText;
            }
        }

        public static string BillerReverse(string account_no,string biller_id,string billersubid,string channel_id,string bnkrefrance,string amount,string vocher_id)
        {
            getconfig();
            //Uri requestUri = new Uri(BASE_URL + "/GetCustInfoByID");
            Uri requestUri = new Uri(BASE_URL + "/BPGReverse");
            dynamic dynamicJson = new ExpandoObject();

            //dynamicJson.CustID = cif;//"1300420105s93883".ToString();
            dynamicJson.acounttfrom = account_no;
            dynamicJson.Biller_ID = biller_id;
            dynamicJson.Biller_sub_ID = billersubid;
            dynamicJson.Channel_ID = channel_id;
            dynamicJson.refno = bnkrefrance;
            dynamicJson.amount = amount;
            dynamicJson.voucher_ID = vocher_id;

            dynamicJson.uuid = Guid.NewGuid();

            string json = "";
            json = JsonConvert.SerializeObject(dynamicJson);
            var responJsonText = "";
            JObject JResp = new JObject();

            using (var objClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage respon = objClient
                        .PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json")).Result;

                    if (respon.IsSuccessStatusCode)
                    {
                        responJsonText = respon.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception e)
                {
                    responJsonText = "Error";
                }

                return responJsonText;

            }

        }

        public static string GetCustinfobycif(string cif)
        {
            getconfig();
            Uri requestUri = new Uri(BASE_URL + "/GetCustInfoByID");
            dynamic dynamicJson = new ExpandoObject();

            dynamicJson.CustID = cif;//"130042010593883".ToString();
            dynamicJson.Authentication = "Card";
            dynamicJson.Channel = "InternetBanking";
            dynamicJson.lang = "1";
            dynamicJson.uuid = Guid.NewGuid();

            string json = "";
            json = JsonConvert.SerializeObject(dynamicJson);
            var responJsonText = "";
            JObject JResp = new JObject();

            using (var objClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage respon = objClient
                        .PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json")).Result;

                    if (respon.IsSuccessStatusCode)
                    {
                        responJsonText = respon.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception e)
                {
                    responJsonText = "Error";
                }

                return responJsonText;

            }

        }

        public static string GetCustaccounts(string accountNo)
        {
            getconfig();
            Uri requestUri = new Uri(BASE_URL + "/GetCustinfoByID");

            dynamic dynamicJson = new ExpandoObject();
            dynamicJson.account = accountNo;//"130042010593883".ToString();
            dynamicJson.Authentication = "Card";
            dynamicJson.Channel = "InternetBanking";
            dynamicJson.lang = "1";
            dynamicJson.uuid = Guid.NewGuid();

            string json = "";
            json = JsonConvert.SerializeObject(dynamicJson);
            var responJsonText = "";
            JObject JResp = new JObject();

            using (var objClient = new HttpClient())
            {
                try
                {

                    HttpResponseMessage respon = objClient
                        .PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json")).Result;

                    if (respon.IsSuccessStatusCode)
                    {
                        responJsonText = respon.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception e)
                {
                    responJsonText = "Error";
                }

                return responJsonText;

            }

        }

    }
}