/*  App/Utils/PageCollection.cs
 *  Version: v1.2 (2023.11.19)
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
            private const string c_Me = nameof(Home);

            public static class Account
            {
                private const string c_Me = nameof(Account);

                public const string List = $"{Home.c_Me}/{c_Me}/{nameof(List)}";
                public const string Manage = $"{Home.c_Me}/{c_Me}/{nameof(Manage)}";
            }

            public static class Management
            {
                private const string c_Me = nameof(Management);

                public const string Updater = $"{Home.c_Me}/{c_Me}/{nameof(Updater)}";
                public const string ConfigPanel = $"{Home.c_Me}/{c_Me}/{nameof(ConfigPanel)}";
            }


            public const string Index = $"{c_Me}/{nameof(Index)}";
        }


        public static class Simulator
        {
            private const string c_Me = nameof(Simulator);

            public static class FinancialManagement
            {
                private const string c_Me = nameof(FinancialManagement);

                public const string List = $"{Simulator.c_Me}/{c_Me}/{nameof(List)}";
                public const string Create = $"{Simulator.c_Me}/{c_Me}/{nameof(Create)}";
                public const string Remove = $"{Simulator.c_Me}/{c_Me}/{nameof(Remove)}";
                public const string Update = $"{Simulator.c_Me}/{c_Me}/{nameof(Update)}";
            }
        }


        public const string Index = nameof(Index);
        public const string ServerAnnouncement = nameof(ServerAnnouncement);













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
