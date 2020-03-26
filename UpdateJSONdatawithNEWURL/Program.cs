using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRXServices.EmailServices;
using CRXModels.DTO;
using System.Configuration;
using CRXServices.ContentServices;
using Newtonsoft.Json;
using Dapper;
using CRXServices.Utilities;

namespace UpdateJSONdatawithNEWURL
{
    class Program
    {

        private static string sConn = "Data Source=10.1.203.33;Initial Catalog=CoherentRX_Production;User ID=CRX_ProductionDB;Password=2@c45m1t4;Connection Timeout=120;Column Encryption Setting=Enabled";


        static void Main(string[] args)
        {

            Console.WriteLine("ProcessStarted");
            UpdateJSONdataURL();

            Console.WriteLine("ProcessEnded");
            Console.ReadLine();

        }

        static void UpdateJSONdataURL()
        {
            using (SqlConnection conn = new SqlConnection(sConn))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("Get_dataTOUpdatedJSONEmailTEMPLATE", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300000;

                int EmailID = -1;

                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var customEmailTemplate = new EmailTemplate();
                        if (string.IsNullOrEmpty(rdr["JSONTemplate"].ToString()) == false)
                        {
                            customEmailTemplate = JsonConvert.DeserializeObject<EmailTemplate>(rdr["JSONTemplate"].ToString());
                            EmailID = (int)rdr["EmailID"];
                            if (customEmailTemplate.MessageDetailURL != "https://dashboard.coherentrx.com/TMD?id=")
                            {
                                customEmailTemplate.MessageDetailURL = "https://dashboard.coherentrx.com/TMD?id=";                                
                                UpdateJSONTemplateEmailSent(EmailID, customEmailTemplate);

                                Console.WriteLine($"Updated for EmailID -  : " + EmailID);

                            }
                        }
                    }
                }
            }
        }


        public static void UpdateJSONTemplateEmailSent(int EmailId, EmailTemplate emailTemplate)
        {

            var jsonEmailTemplate = JsonConvert.SerializeObject(emailTemplate);

            using (var connection = new SqlConnection(sConn))
            {
                var parms = new DynamicParameters();
                parms.Add("@EmailID", EmailId);
                parms.Add("@JSONEmailTemplate", jsonEmailTemplate);
                try
                {
                    connection.Execute("UpdateJSONTemplateEmailSent", param: parms, commandType: CommandType.StoredProcedure);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
