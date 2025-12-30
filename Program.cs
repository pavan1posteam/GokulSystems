using System;
using System.Configuration;
using Gokulsystems.Models;

namespace Gokulsystems
{
    class Program
    {
        private static void Main(string[] args)
        {
            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
            try
            {
                POSSettings pOSSettings = new POSSettings();
                pOSSettings.IntializeStoreSettings();
                foreach (POSSetting current in pOSSettings.PosDetails)
                {
                    try
                    {

                        /*if (current.StoreSettings.StoreId == 12597)
                        { Console.WriteLine("fetching_storeid__" + current.StoreSettings.StoreId ); }
                        else { continue; }*/

                        if ( current.PosName.ToUpper() == "GOKULSYSTEMS"  )
                        {
                            Gokulsystems.clsGokulSystems.GokulCsvProducts clsGokulSystems = new Gokulsystems.clsGokulSystems.GokulCsvProducts(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.BaseUrl, current.StoreSettings.POSSettings.Username, current.StoreSettings.POSSettings.Password, current.StoreSettings.POSSettings.Pin, current.StoreSettings.POSSettings.categoriess);
                            Console.WriteLine();
                        }
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                    }
                }
            }

            catch (Exception ex)
            {
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
            finally
            {
            }
        }
    }
}
