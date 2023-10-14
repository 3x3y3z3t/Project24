/*  App/Utils/PageCollection.cs
 *  Version: v1.0 (2023.10.15)
 *  
 *  Author
 *      Arime-chan
 */

namespace Project24.App.Utils
{
    public static class PageCollection
    {
        public static class Home
        {
            public static class Management
            {
                public const string Updater = "Home/Management/Updater";
                public const string ConfigPanel = "Home/Management/ConfigPanel";
            }


            public const string Index = "Home/Index";
        }


        public static class Simulator
        {
            public static class FinancialManagement
            {
                public const string List = "Simulator/FinancialManagement/List";
                public const string Create = "Simulator/FinancialManagement/Create";
                public const string Remove = "Simulator/FinancialManagement/Remove";
                public const string Update = "Simulator/FinancialManagement/Update";
            }
        }


        public const string Index = "Index";
        public const string ServerAnnouncement = "ServerAnnouncement";













        public const string PAGE_INDEX =                                    Index;
        public const string PAGE_SERVER_ANNOUNCEMENT =                      ServerAnnouncement;

        public const string PAGE_HOME_INDEX =                               Home.Index;
        public const string PAGE_HOME_MANAGEMENT_UPDATER =                  Home.Management.Updater;
        public const string HOME_MANAGEMENT_CONFIG_PANEL =                  Home.Management.ConfigPanel;

        public const string PAGE_SIMULATOR_FINANCIAL_MANAGEMENT_LIST =      Simulator.FinancialManagement.List;
        public const string PAGE_SIMULATOR_FINANCIAL_MANAGEMENT_CREATE =    Simulator.FinancialManagement.Create;
        public const string PAGE_SIMULATOR_FINANCIAL_MANAGEMENT_REMOVE =    Simulator.FinancialManagement.Remove;
        public const string PAGE_SIMULATOR_FINANCIAL_MANAGEMENT_UPDATE =    Simulator.FinancialManagement.Update;
        //public const string PAGE_SIMULATOR_FINANCIAL_MANAGEMENT_DETAIL =    Simulator.FinancialManagement.Detail;
    }

}
