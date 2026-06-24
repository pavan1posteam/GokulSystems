using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Gokulsystems.Models
{
    public class POSSettings
    {
        public List<POSSetting> PosDetails { get; set; }
        public void IntializeStoreSettings()
        {
            DataSet dsResult = new DataSet();
            List<POSSetting> posdetails = new List<POSSetting>();
            List<StoreSetting> StoreList = new List<StoreSetting>();
            try
            {
                List<SqlParameter> sparams = new List<SqlParameter>();
               sparams.Add(new SqlParameter("@PosId", 37));  //uncomment for live 


                //Handling missing dbsettings.json File 
                string constr = ConfigurationManager.AppSettings["LiquorAppsConnectionString"];
                Console.WriteLine("constr-from-Appconfig " + constr);
                // If App.config doesn't have the connection string, use dbsettings.json
                if (string.IsNullOrWhiteSpace(constr))
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbsettings.json");

                    if (File.Exists(filePath))
                    {
                        try
                        {
                            DbSettings dbcon = JsonConvert.DeserializeObject<DbSettings>(
                                File.ReadAllText(filePath));

                            if (dbcon?.liquorappsconnectionstring != null &&
                                dbcon.liquorappsconnectionstring.Count > 0)
                            {
                                // Local connection string
                                constr = dbcon.liquorappsconnectionstring[1]; // [0] is for local & [1] for live db 
                                Console.WriteLine("constr-2 " + constr);
                            }
                        }
                        catch
                        {
                            // Ignore and handle if constr is still null
                        }
                    }

                }



                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "usp_ts_GetStorePosSetting";
                        cmd.Parameters.Add(sparams[0]);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter())
                        {
                            da.SelectCommand = cmd;
                            da.Fill(dsResult);
                        }
                    }
                }
                if (dsResult != null || dsResult.Tables.Count > 0)
                {
                    foreach (DataRow dr in dsResult.Tables[0].Rows)
                    {
                        POSSetting pobj = new POSSetting();
                        pobj.Setting = dr["Settings"].ToString();
                        StoreSetting obj = new StoreSetting();
                        obj.StoreId = Convert.ToInt32(dr["StoreId"] == DBNull.Value ? 0 : dr["StoreId"]);
                        obj.POSSettings = JsonConvert.DeserializeObject<Setting>(pobj.Setting);
                        pobj.PosName = dr["PosName"].ToString();
                        pobj.PosId = Convert.ToInt32(dr["PosId"]);
                        pobj.StoreSettings = obj;
                        if (pobj.StoreSettings.POSSettings != null)
                        {
                            pobj.StoreSettings.POSSettings.categoriess = obj.POSSettings.categoriess;
                            pobj.StoreSettings.POSSettings.Upc = obj.POSSettings.Upc;
                        }
                        posdetails.Add(pobj);
                    }
                }
                PosDetails = posdetails;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Read();

            }

        }
    }
    public class POSSetting
    {
        public int PosId { get; set; }
        public string PosName { get; set; }
        public StoreSetting StoreSettings { get; set; }
        public string Setting { get; set; }
    }
    public class Setting
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public int AccountID { get; set; }
        public string RefreshToken { get; set; }
        public string Token { get; set; }
        public string merchantId { get; set; }
        public string Store_id { get; set; } // Added for lingaPOs API
        public string I_Storeid { get; set; } // Added for lingaPOs API
        public string Username { get; set; }
        public string Password { get; set; }
        public string Pin { get; set; }
        public int SHOPID { get; set; }
        public string Code { get; set; }
        public string tokenid { get; set; }
        public string instock { get; set; }
        public string category { get; set; }
        public string BaseUrl { get; set; }
        public decimal tax { get; set; }

        public decimal mixtax { get; set; }
        public string PosFileName { get; set; }
        public string PosFileName2 { get; set; }
        public string APIKey { get; set; }
        public int StoreMapId { get; set; }
        public decimal liquortax { get; set; }
        public decimal liquortaxrateperlitre { get; set; }
        public List<categories> categoriess { set; get; }
        public string LocationId { get; set; }
        public bool IsSalePrice { get; set; }
        public bool IsMarkUpPrice { get; set; }
        public int MarkUpValue { get; set; }
        public bool IsApi { get; set; }
        public List<UPC> Upc { get; set; }
        public decimal beertax { get; set; }
        public decimal winetax { get; set; }
        public int client_id { get; set; }
    }
    public class categories
    {
        public string id { get; set; }
        public string name { get; set; }
        public decimal taxrate { get; set; }
        public Boolean selected { get; set; }
    }
    public class UPC
    {
        public string upccode { get; set; }
    }
    public class StoreSetting
    {
        public int StoreId { get; set; }
        public Setting POSSettings { get; set; }
    }
    public class storecat
    {
        public string catid { get; set; }
        public string catname { get; set; }
    }

    public class DbSettings
    {
        public List<string> liquorappsconnectionstring { get; set; }

    }
}
