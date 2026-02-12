using ExtractPosData.Models;
using Gokulsystems.Models;
using Newtonsoft.Json;
//using System.Xaml;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
namespace Gokulsystems
{
    class clsGokulSystems
    {
        string Differentendpoints = ConfigurationManager.AppSettings.Get("Differentendpoints");
        

        public List<JArray> products(int StoreId, decimal tax, string BaseUrl, string Username, string Password, string Pin)
        {
            List<JArray> productList = new List<JArray>(); 

            if (string.IsNullOrEmpty(BaseUrl))
            {
                BaseUrl = "https://products.gpossystem.com";
            }
            // if (Differentendpoints.Contains(StoreId.ToString()))
            if (string.IsNullOrEmpty(Pin))    //gokul
            {          
                Boolean flag = Regex.IsMatch(BaseUrl, @"com$");
                if (flag)
                {
                    BaseUrl += "/api/v1/GetProduct";
                }

                var client = new RestClient(BaseUrl);
                var request = new RestRequest(Method.GET);
                string content = null;

                request.AddHeader("UserId", Username);
                request.AddHeader("Password", Password);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("User-Agent", "PostmanRuntime/7.42.0");
                request.AddHeader("Accept-Encoding", "gzip,deflate,br");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "*/*");

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);
                content = response.Content;
                //File.WriteAllText($"{StoreId}.json", response.Content);
                var result = JsonConvert.DeserializeObject<clsProductList.items>(content);

                var pJson = (dynamic)JObject.Parse(content);
                var jArray = (JArray)pJson["Data"];
                productList.Add(jArray);

            }
            else
            {
                

                string authInfo = Username + ":" + Password + ":" + Pin;
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                string content = null;
                GokulclsProductList obj = new GokulclsProductList();

                BaseUrl = string.IsNullOrEmpty(obj.Url) ? BaseUrl : obj.Url;
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(Method.GET);

                request.AddHeader("Authorization", "Basic " + authInfo);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Accept", "application/json");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);
                content = response.Content;
                var result = JsonConvert.DeserializeObject<clsProductList.items>(content);

                var pJson = (dynamic)JObject.Parse(content);
                var jArray = (JArray)pJson["Data"];
                productList.Add(jArray);
            }

            return productList;

        }
        public class GokulCsvProducts
        {
            string BaseUrl1 = ConfigurationManager.AppSettings.Get("BaseDirectory");
            string Differentendpoints = ConfigurationManager.AppSettings.Get("Differentendpoints");
            string StaticQty = ConfigurationManager.AppSettings.Get("StaticQty");

            public GokulCsvProducts(int storeid, decimal tax, string BaseUrl, string Username, string Password, string Pin, List<categories> cat)
            {
                GokulproductForCSV(storeid, tax, BaseUrl, Username, Password, Pin, cat);
            }
            public void GokulproductForCSV(int storeid, decimal tax, string BaseUrl, string Username, string Password, string Pin, List<categories> cat)
            {
                try
                {
                    clsGokulSystems products = new clsGokulSystems();
                    var productList = products.products(storeid, tax, BaseUrl, Username, Password, Pin);

                    List<datatableModel> pf = new List<datatableModel>();
                    List<ProductsModel> prodlist = new List<ProductsModel>();
                    List<FullNameProductModel> full = new List<FullNameProductModel>();

                    foreach (var item in productList)
                    {
                        foreach (var itm in item)
                        {
                            ProductsModel pmsk = new ProductsModel();
                            FullNameProductModel fname = new FullNameProductModel();
                            datatableModel pdf = new datatableModel();
                            pmsk.StoreID = storeid;
                            //if (Differentendpoints.Contains(storeid.ToString()))
                            if (string.IsNullOrEmpty(Pin))
                            {
                                string upc = itm["upc"].ToString().ToLower();
                                if (!string.IsNullOrEmpty(upc))
                                {
                                    string cleanedUpc = Regex.Replace(upc, "[^0-9.]", "").Trim().ToLower();
                                    if (!string.IsNullOrEmpty(cleanedUpc))
                                    {
                                        pmsk.upc = "#" + cleanedUpc;
                                        fname.upc = "#" + cleanedUpc;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                                
                                string sku = itm["sku"].ToString();
                                pmsk.sku = "#" + sku;
                                fname.sku = "#" + sku;
                                pmsk.Qty = Convert.ToInt32(itm["qty"]);
                                if(StaticQty.Contains(storeid.ToString()))
                                {
                                    pmsk.Qty = 999;
                                }
                                pmsk.StoreProductName = itm["productname"].ToString().Trim();
                                fname.pname = itm["productname"].ToString().Trim();
                                pmsk.StoreDescription = itm["productname"].ToString().Trim();
                                fname.pdesc = itm["productname"].ToString().Trim();
                                //pmsk.pack = getpack(pmsk.StoreProductName);
                                #region new include for pack 
                                pmsk.pack = 1;
                                var packs = itm["pack"].ToString();
                                if (!string.IsNullOrEmpty(packs))
                                    pmsk.pack =  getpack(packs);
                                
                                if(pmsk.pack == 1)
                                    pmsk.pack = getpack(pmsk.StoreProductName);
                                #endregion
                                fname.pack = pmsk.pack;
                                pmsk.Price = Convert.ToDecimal(itm["retailprice"]);
                                fname.Price = Convert.ToDecimal(itm["retailprice"]);
                                pmsk.sprice =0;
                                pmsk.Start = "";
                                pmsk.End = "";
                                pmsk.Tax = tax;
                                pmsk.altupc1 = "";
                                pmsk.altupc2 = "";
                                pmsk.altupc3 = "";
                                pmsk.altupc4 = "";
                                pmsk.altupc4 = "";
                                pmsk.altupc5 = "";
                                pmsk.uom = itm["size"].ToString();
                                if (string.IsNullOrEmpty(pmsk.uom))
                                    pmsk.uom = getVolume(pmsk.StoreProductName);
                                fname.pcat = itm["depname"].ToString();
                                #region new include for excluding thc & tobacco category 
                                if (storeid == 12892)
                                {
                                    string category = fname.pcat?.Trim().ToUpper();

                                    if (category.Contains("TOBACCO") ||
                                        category.Contains("THC"))
                                    {
                                        continue;
                                    }
                                }

                                #endregion
                                fname.pcat1 = "";
                                fname.pcat2 = "";
                                fname.country = "";
                                fname.region = "";
                                if (pmsk.Price > 0 && pmsk.Price < Convert.ToDecimal(199.99) && !string.IsNullOrEmpty(pmsk.upc))
                                {
                                    prodlist.Add(pmsk);
                                    prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                    prodlist = prodlist.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                                    full.Add(fname);
                                    full = full.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                    full = full.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                                }
                            }
                            else
                            {

                                decimal result;
                                string upc = itm["UPC"].ToString();
                                Decimal.TryParse(upc, System.Globalization.NumberStyles.Float, null, out result);
                                upc = result.ToString();

                                if (upc == "" || upc == "0")
                                {
                                    pmsk.upc = "";
                                    fname.upc = "";
                                }
                                else
                                {
                                    pmsk.upc = upc;
                                    fname.upc = upc;
                                }
                                string sku = itm["SKU"].ToString();
                                pmsk.sku = sku;
                                fname.sku = sku;
                                pmsk.Qty = Convert.ToInt32(itm["TotalQty"]);
                                if (StaticQty.Contains(storeid.ToString()))
                                {
                                    pmsk.Qty = 999;
                                }
                                if (string.IsNullOrEmpty(pmsk.Qty.ToString()))
                                    continue;
                                pmsk.StoreProductName = itm["ItemName"].ToString();
                                fname.pname = itm["ItemName"].ToString();
                                pmsk.StoreDescription = itm["ItemName"].ToString();
                                fname.pdesc = itm["ItemName"].ToString();
                                //pmsk.pack = getpack(pmsk.StoreProductName);
                                #region new include for pack 
                                pmsk.pack = 1;
                                var packs = itm["pack"].ToString();
                                if (!string.IsNullOrEmpty(packs))
                                    pmsk.pack = getpack(packs);

                                if (pmsk.pack == 1)
                                    pmsk.pack = getpack(pmsk.StoreProductName);
                                #endregion
                                fname.pack = pmsk.pack;
                                pmsk.Price = Convert.ToDecimal(itm["Price"]);
                                fname.Price = Convert.ToDecimal(itm["Price"]);
                                pmsk.sprice = Convert.ToDecimal(itm["SALEPRICE"]);
                                pmsk.Start = "";
                                pmsk.End = "";
                                pmsk.Tax = tax;
                                pmsk.altupc1 = "";
                                pmsk.altupc2 = "";
                                pmsk.altupc3 = "";
                                pmsk.altupc4 = "";
                                pmsk.altupc4 = "";
                                pmsk.altupc5 = "";
                                pmsk.uom = itm["SizeName"].ToString();
                                if (string.IsNullOrEmpty(pmsk.uom))
                                    pmsk.uom = getVolume(pmsk.StoreProductName);
                                fname.pcat = itm["Department"].ToString();
                                fname.pcat1 = "";
                                fname.pcat2 = "";
                                fname.country = "";
                                fname.region = "";
                                if (pmsk.Price > 0 && pmsk.Price < Convert.ToDecimal(199.99) && !string.IsNullOrEmpty(pmsk.upc))
                                {
                                    prodlist.Add(pmsk);
                                    prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                    prodlist = prodlist.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                                    full.Add(fname);
                                    full = full.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                    full = full.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                                }

                            }
                        }
                        if (cat != null && cat.Count > 0)
                        {
                            foreach (var cats in cat)
                            {
                                var query = (from w in pf
                                             where w.pcat.Trim() != cats.name.ToUpper()
                                             select w).ToList();
                                pf = query;
                            }
                        }
                        Console.WriteLine("Generating GokulSystems " + storeid + " Product CSV Files.....");
                        string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", storeid, BaseUrl1);
                        Console.WriteLine("Product File Generated For GokulSystems " + storeid);
                        Console.WriteLine();
                        Console.WriteLine("Generating GokulSystems " + storeid + " Fullname CSV Files.....");
                        var fullfilename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", storeid, BaseUrl1);
                        Console.WriteLine("Fullname File Generated For GokulSystems " + storeid);
                        //Datatabletocsv csv = new Datatabletocsv();
                      //  csv.Datatablecsv(storeid, tax, pf);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            public int getpack(string prodName)
            {
                prodName = prodName.ToUpper();
                var regexMatch = Regex.Match(prodName, @"(?<Result>\d+)\s*PK");
                var prodPack = regexMatch.Groups["Result"].Value;
                if (prodPack.Length > 0)
                {
                    int outVal = 0;
                    int.TryParse(prodPack.Replace("$", ""), out outVal);
                    return outVal;
                }
                return 1;
            }
            public string getVolume(string prodName)
            {
                prodName = prodName.ToUpper();
                var regexMatch = Regex.Match(prodName, @"(?<Result>\d+)ML| (?<Result>\d+)LTR| (?<Result>\d+)OZ | (?<Result>\d+)L|(?<Result>\d+)OZ");
                var prodPack = regexMatch.Groups["Result"].Value;
                if (prodPack.Length > 0)
                {
                    return regexMatch.ToString();
                }
                return "";
            }
        }
        public class GokulclsProductList
        {
            public bool StatusVal { get; set; }
            public int StatusCode { get; set; }
            public string StatusMsg { get; set; }
            public string Price { get; set; }
            public string SessionID { get; set; }

            public string Url { get; set; }
            public class Data
            {
                public string UPC { get; set; }
                public string SKU { get; set; }
                public string ItemName { get; set; }
                public decimal Price { get; set; }
                public decimal Cost { get; set; }
                public decimal SALEPRICE { get; set; }
                public string SizeName { get; set; }
                public string PackName { get; set; }
                public string Vintage { get; set; }
                public string Department { get; set; }
                public decimal PriceA { get; set; }
                public decimal PriceB { get; set; }
                public decimal PriceC { get; set; }
                public Int32 TotalQty { get; set; }
                public decimal tax { get; set; }
            }
            public class items
            {
                public List<Data> item { get; set; }
            }
        }
        public class datatableModel
        {
            public int StoreID { get; set; }
            public string upc { get; set; }
            public decimal Qty { get; set; }
            public string sku { get; set; }
            public int pack { get; set; }
            public string uom { get; set; }
            public string pcat { get; set; }
            public string pcat1 { get; set; }
            public string pcat2 { get; set; }
            public string country { get; set; }
            public string region { get; set; }
            public string StoreProductName { get; set; }
            public string StoreDescription { get; set; }
            public decimal Price { get; set; }
            public decimal sprice { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public decimal Tax { get; set; }
            public string altupc1 { get; set; }
            public string altupc2 { get; set; }
            public string altupc3 { get; set; }
            public string altupc4 { get; set; }
            public string altupc5 { get; set; }
        }
        public class ListtoDataTableConverter
        {
            public DataTable ToDataTable<T>(List<T> items, int StoreId)
            {
                DataTable dt = new DataTable(typeof(T).Name);

                PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo prop in Props)
                {
                    dt.Columns.Add(prop.Name);
                }

                foreach (T item in items)
                {
                    var values = new object[Props.Length];

                    for (int i = 0; i < Props.Length; i++)
                    {
                        //inserting property values to datatable rows
                        values[i] = Props[i].GetValue(item, null);
                    }
                    dt.Rows.Add(values);
                }
                return dt;
            }
        }
        public class Datatabletocsv
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            public List<POSSetting> PosDetails { get; set; }
            public void Datatablecsv(int storeid, decimal tax, List<datatableModel> dtlist)
            {
                DataSet dsResult = new DataSet();
                List<POSSetting> posdetails = new List<POSSetting>();
                string constr = ConfigurationManager.AppSettings.Get("LiquorAppsConnectionString");
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "usp_ts_GetStorePosSetting";
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
                        if (dr["PosName"].ToString().ToUpper() == "GOKULSYSTEMS")
                        {
                            POSSetting pobj = new POSSetting();
                            StoreSetting obj = new StoreSetting();
                            obj.StoreId = Convert.ToInt32(dr["StoreId"] == DBNull.Value ? 0 : dr["StoreId"]);
                            pobj.StoreSettings = obj;
                            posdetails.Add(pobj);
                        }
                    }
                }
                PosDetails = posdetails;
                foreach (var item in PosDetails)
                {
                    try
                    {
                        ListtoDataTableConverter cvr = new ListtoDataTableConverter();
                        DataTable dt = cvr.ToDataTable(dtlist, storeid);
                       // storeid = item.StoreSettings.StoreId;
                        var dtr = from s in dt.AsEnumerable() select s;
                        List<ProductsModel> prodlist = new List<ProductsModel>();
                        List<FullNameProductModel> full = new List<FullNameProductModel>();
                        dynamic upcs;
                        //dynamic taxs;
                        int barlenth = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            ProductsModel pmsk = new ProductsModel();
                            FullNameProductModel fname = new FullNameProductModel();
                            dt.DefaultView.Sort = "sku";
                            upcs = dt.DefaultView.FindRows(dr["sku"]).ToArray();
                            barlenth = ((Array)upcs).Length;
                            pmsk.StoreID = storeid;
                            if (barlenth > 0)
                            {
                                for (int i = 0; i <= barlenth - 1; i++)
                                {
                                    if (i == 0)
                                    {
                                        if (!string.IsNullOrEmpty(dr["upc"].ToString()))
                                        {
                                            var upc = "#" + upcs[i]["upc"].ToString().ToLower();
                                            string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                            if (!string.IsNullOrEmpty(numberUpc))
                                            {
                                                pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                                fname.upc = "#" + numberUpc.Trim().ToLower();
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    if (i == 1)
                                    {
                                        pmsk.altupc1 = "#" + upcs[i]["upc"];
                                    }
                                    if (i == 2)
                                    {
                                        pmsk.altupc2 = "#" + upcs[i]["upc"];
                                    }
                                    if (i == 3)
                                    {
                                        pmsk.altupc3 = "#" + upcs[i]["upc"];
                                    }
                                    if (i == 4)
                                    {
                                        pmsk.altupc4 = "#" + upcs[i]["upc"];
                                    }
                                    if (i == 5)
                                    {
                                        pmsk.altupc5 = "#" + upcs[i]["upc"];
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(dr["sku"].ToString()))
                            {
                                pmsk.sku = "#" + dr["sku"].ToString();
                                fname.sku = "#" + dr["sku"].ToString();
                            }
                            else
                            { continue; }
                            pmsk.Qty = 999;
                            pmsk.StoreProductName = dr.Field<string>("StoreProductName").Trim();
                            pmsk.StoreDescription = dr.Field<string>("StoreProductName").Trim();
                            fname.pdesc = dr.Field<string>("StoreProductName").Trim();
                            fname.pname = dr.Field<string>("StoreProductName").Trim();
                            pmsk.Price = System.Convert.ToDecimal(dr["Price"].ToString());
                            fname.Price = System.Convert.ToDecimal(dr["Price"].ToString());
                            pmsk.sprice = System.Convert.ToDecimal(dr["sprice"].ToString());
                            pmsk.pack = 1;
                            pmsk.Tax = Convert.ToDecimal(dr["Tax"]);
                            if (pmsk.sprice > 0)
                            {
                                pmsk.Start = DateTime.Now.ToString("MM/dd/yyyy");
                                pmsk.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                pmsk.Start = "";
                                pmsk.End = "";
                            }
                            fname.pcat = dr.Field<string>("pcat");
                            fname.pcat1 = dr.Field<string>("pcat1");
                            fname.pcat2 = "";
                            fname.pack = 1;
                            pmsk.uom = dr.Field<string>("uom");
                            fname.uom = dr.Field<string>("uom");
                            fname.region = "";
                            fname.country = "";
                            if (pmsk.Price > 0 && pmsk.Price < Convert.ToDecimal(199.99) && !string.IsNullOrEmpty(pmsk.upc))
                            {
                                prodlist.Add(pmsk);
                                prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                prodlist = prodlist.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                                full.Add(fname);
                                full = full.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                full = full.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                            }
                        }
                        Console.WriteLine("Generating GokulSystems " + storeid + " Product CSV Files.....");
                        string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", storeid, BaseUrl);
                        Console.WriteLine("Product File Generated For GokulSystems " + storeid);
                        Console.WriteLine();
                        Console.WriteLine("Generating GokulSystems " + storeid + " Fullname CSV Files.....");
                        var fullfilename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", storeid, BaseUrl);
                        Console.WriteLine("Fullname File Generated For GokulSystems " + storeid);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("" + e.Message);
                    }
                }
            }
        }
        public class clsProductList
        {
            public bool StatusVal { get; set; }
            public int StatusCode { get; set; }
            public string StatusMsg { get; set; }
            public string Price { get; set; }
            public string SessionID { get; set; }

            public string Url { get; set; }
            public class Data
            {
                public string UPC { get; set; }
                public int SKU { get; set; }
                public string ItemName { get; set; }
                public double Price { get; set; }
                public double Cost { get; set; }
                public double SALEPRICE { get; set; }
                public string SizeName { get; set; }
                public object PackName { get; set; }
                public string Vintage { get; set; }
                public string Department { get; set; }
                public double PriceA { get; set; }
                public double PriceB { get; set; }
                public double PriceC { get; set; }
                public double TotalQty { get; set; }
                public string ALTUPC1 { get; set; }
                public string ALTUPC2 { get; set; }
                public int STORECODE { get; set; }
            }

            public class items
            {
                public List<Data> item { get; set; }
            }
        }
        public class ProductsModel
        {
            public int StoreID { get; set; }
            public string upc { get; set; }
            public Int64 Qty { get; set; }
            public string sku { get; set; }
            public int pack { get; set; }
            public string uom { get; set; }
            public string StoreProductName { get; set; }
            public string StoreDescription { get; set; }
            public decimal Price { get; set; }
            public decimal sprice { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public decimal Tax { get; set; }
            public string altupc1 { get; set; }
            public string altupc2 { get; set; }
            public string altupc3 { get; set; }
            public string altupc4 { get; set; }
            public string altupc5 { get; set; }
            public decimal Deposit { get; set; }
        }
    }
}