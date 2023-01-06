/*  Enums.cs
 *  Version: 1.4 (2023.01.06)
 *
 *  Contributor
 *      Arime-chan
 */

namespace Project24.App
{
    public enum AppModule : byte
    {
        Dashboard = 1,
        P24_ClinicManager,
        P24b_Nas
    }

    public enum P24Module : byte
    {
        Customer = 1,
        Ticket
    }

    public enum P24ImportExportType : byte
    {
        Import = 1,
        Export = 2
    }

    public static class P24ExportType_
    {
        /// <summary> Represents retail sale. </summary>
        public const string Retail = "Retail";

        /// <summary> Represents usage as consumptiosn during services. </summary>
        public const string Consumption = "Consumption";

        /// <summary> Represents a free distribution or after-service service. </summary>
        public const string FreeService = "FreeService";
    }

}
