using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using tpi.Model;

namespace tpi.Service
{
    public class BankingDetailService
    {

        public static string ClassName = "BankingDetailService";
        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static BankingDetail QueryBankingDetail(HttpClient httpClient)
        {

            try
            {
                var odataQuery = webApiQueryUrl + "businessunits()" +
                                 "?$select=blu_bankaccountname,blu_bankaccountnumber,blu_bankbranchcode,blu_bankbranchname,blu_bankname,_blu_donotreplyqueueid_value";
                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                foreach (var data in systemUserObject.value)
                    return new BankingDetail()
                    {
                        AccountName = data["blu_bankaccountname"],
                        AccountNumber = data["blu_bankaccountnumber"],
                        BranchCode = data["blu_bankbranchcode"],
                        BranchName = data["blu_bankbranchname"],
                        BankName = data["blu_bankname"]
                    };

                return null;

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = ClassName + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return null;
            }

        }

    }
}
