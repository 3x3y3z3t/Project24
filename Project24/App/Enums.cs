/*  Enums.cs
 *  Version: 1.5 (2023.02.11)
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
        /// <summary> Represents usage as consumptiosn during services. This export will always attaches to a Visiting Ticket. </summary>
        public const string Ticket = "Ticket";

        /// <summary> Represents common daily or monthly consumption. </summary>
        public const string Common = "Common";

        /// <summary> Represents throwing away equipment (due to expired or damaged). </summary>
        public const string Dump = "Dump";
    }

}
