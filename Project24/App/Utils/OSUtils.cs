/*  App/Utils/OSUtils.cs
 *  Version: v1.0 (2023.08.27)
 *  
 *  Contributor
 *      Arime-chan
 */

using AppHelper;

namespace Project24.App
{
    public static class OSUtils
    {
        public static bool Unix_EnableApp(string _appSide, bool _launchNow = false)
        {
            if (_appSide == null)
                return false;

            Unix_ExecSystemdCommand(c_AppName + "-" + _appSide, true, _launchNow);

            return true;
        }

        public static bool Unix_DisableApp(string _appSide, bool _launchNow = false)
        {
            if (_appSide == null)
                return false;

            Unix_ExecSystemdCommand(c_AppName + "-" + _appSide, false, _launchNow);

            return true;
        }

        private static void Unix_ExecSystemdCommand(string _svcName, bool _enable, bool _isNow = false)
        {
            string command = "sudo systemctl ";

            if (_enable)
                command += "enable";
            else
                command += "disable";

            if (_isNow)
                command += " --now ";

            command += _svcName;
            SystemCaller.ExecUnixCommand(command);
        }


        private const string c_AppName = "kestrel-p24-core6";
    }

}
