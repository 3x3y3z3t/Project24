/*  AppsettingsHelperClasses.cs
 *  Version: v1.1 (2023.09.21)
 *  
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AppHelper
{
    public sealed class Appsettings_
    {
        [JsonIgnore]
        public const string AppsettingsCurrentVersion = "20230825";

        public string AppsettingsVersion { get; set; } = "";

        public Credentials_ Credentials { get; set; } = new();
        public bool DetailedErrors { get; set; } = false;
        public Logging_ Logging { get; set; } = new();
        public Kestrel_ Kestrel { get; set; } = new();


        #region Appsettings_Nested
        public sealed class Credentials_
        {
            public Credential_ DBCredential { get; set; } = new() { Username = "root", Password = "12345@Aa" };
            public Credential_ PowerUser { get; set; } = new() { Username = "power", Password = "12345@Aa" };


            #region Credentials_Nested
            public sealed class Credential_
            {
                public string Username { get; set; } = "root";
                public string Password { get; set; } = "12345@Aa";
            }
            #endregion
        }

        public sealed class Logging_
        {
            public Dictionary<string, string> LogLevel { get; set; } = new()
            {
                { "Default", Microsoft.Extensions.Logging.LogLevel.Information.ToString() },
                { "Microsoft.AspNetCore", Microsoft.Extensions.Logging.LogLevel.Warning.ToString() },
                { "Microsoft.Hosting.Lifetime", Microsoft.Extensions.Logging.LogLevel.Information.ToString() }
            };
        }

        public sealed class Kestrel_
        {
            public Endpoints_ Endpoints { get; set; } = new();


            #region Kestrel_Nested
            public sealed class Endpoints_
            {
                public Http_ Http { get; set; } = new();


                #region Endpoints_Nested
                public sealed class Http_
                {
                    public string Url { get; set; } = "http://localhost:5000";
                }
                #endregion
            }
            #endregion
        }
        #endregion
    }

}

#region Assembly Microsoft.Extensions.Logging.Abstractions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// C:\Program Files\dotnet\packs\Microsoft.AspNetCore.App.Ref\6.0.15\ref\net6.0\Microsoft.Extensions.Logging.Abstractions.dll

namespace Microsoft.Extensions.Logging
{
    //
    // Summary:
    //     Defines logging severity levels.
    internal enum LogLevel
    {
        //
        // Summary:
        //     Logs that contain the most detailed messages. These messages may contain sensitive
        //     application data. These messages are disabled by default and should never be
        //     enabled in a production environment.
        Trace = 0,
        //
        // Summary:
        //     Logs that are used for interactive investigation during development. These logs
        //     should primarily contain information useful for debugging and have no long-term
        //     value.
        Debug = 1,
        //
        // Summary:
        //     Logs that track the general flow of the application. These logs should have long-term
        //     value.
        Information = 2,
        //
        // Summary:
        //     Logs that highlight an abnormal or unexpected event in the application flow,
        //     but do not otherwise cause the application execution to stop.
        Warning = 3,
        //
        // Summary:
        //     Logs that highlight when the current flow of execution is stopped due to a failure.
        //     These should indicate a failure in the current activity, not an application-wide
        //     failure.
        Error = 4,
        //
        // Summary:
        //     Logs that describe an unrecoverable application or system crash, or a catastrophic
        //     failure that requires immediate attention.
        Critical = 5,
        //
        // Summary:
        //     Not used for writing log messages. Specifies that a logging category should not
        //     write any messages.
        None = 6
    }
}
#endregion
