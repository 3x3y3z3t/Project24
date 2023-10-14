/*  Helpers/HelperFileWriter.cs
 *  Version: v1.1 (2023.10.06)
 *  
 *  This file contains all static files' content to be written during first set up.
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.IO;
using System.Text.Json;

namespace AppHelper
{
    internal static class LinuxHelperFileWriter
    {
        public static ErrorCode WriteServiceFile(string _svcName, string _blockName, string _workingDirectory, string _execName, string _user, string _description)
        {
            string content = "" +
                "[Unit]\n" +
                "Description=" + _description + "\n" +
                "\n" +
                "[Service]\n" +
                "WorkingDirectory=" + _workingDirectory + "\n" +
                "ExecStart=" + _workingDirectory + "/" + _execName + "\n" +
                "Restart=always\n" +
                "RestartSec=10\n" +
                "TimeoutSec=90\n" +
                "KillSignal=SIGINT\n" +
                "SyslogIdentifier=dotnet-p24-" + _blockName + "\n" +
                "User=" + _user + "\n" +
                "\n" +
                "[Install]\n" +
                "WantedBy=multi-user.target\n";

            _blockName = _blockName.ToLower();

            return WriteFile("/etc/systemd/system/" + _svcName + "-" + _blockName + ".service", content);
        }

        public static ErrorCode WriteNginxVHostConfigFile(string _appName)
        {
            string content = "" +
                "upstream p24balancer {\n" +
                "    server     127.0.0.1:10000;\n" +
                "    server     127.0.0.1:10001 backup;\n" +
                "}\n" +
                "\n" +
                "server {\n" +
                "    listen                     80;\n" +
                "#    server_name                *;\n" +
                "    location / {\n" +
                "        proxy_pass             http://p24balancer;\n" +
                "        proxy_next_upstream    error timeout http_502;\n" +
                "    }\n" +
                "\n" +
                "    client_max_body_size       32M;\n" +
                "    client_body_buffer_size    512K;\n" +
                "}\n" +
                "\n" +
                "server {\n" +
                "    listen                     10000;\n" +
                "    location / {\n" +
                "        proxy_pass             http://localhost:5000;\n" +
                "        proxy_http_version     1.1;\n" +
                "        proxy_set_header       Upgrade $http_upgrade;\n" +
                "        proxy_set_header       Connection keep-alive;\n" +
                "        proxy_set_header       Host $host;\n" +
                "        proxy_cache_bypass     $http_upgrade;\n" +
                "        proxy_set_header       X-Forwarded-For $proxy_add_x_forwarded_for;\n" +
                "        proxy_set_header       X-Forwarded-Proto $scheme;\n" +
                "    }\n" +
                "\n" +
                "    client_max_body_size       32M;\n" +
                "    client_body_buffer_size    512K;\n" +
                "}\n" +
                "\n" +
                "server {\n" +
                "    listen                     10001;\n" +
                "    location / {\n" +
                "        proxy_pass             http://localhost:5001;\n" +
                "        proxy_http_version     1.1;\n" +
                "        proxy_set_header       Upgrade $http_upgrade;\n" +
                "        proxy_set_header       Connection keep-alive;\n" +
                "        proxy_set_header       Host $host;\n" +
                "        proxy_cache_bypass     $http_upgrade;\n" +
                "        proxy_set_header       X-Forwarded-For $proxy_add_x_forwarded_for;\n" +
                "        proxy_set_header       X-Forwarded-Proto $scheme;\n" +
                "    }\n" +
                "\n" +
                "    client_max_body_size       32M;\n" +
                "    client_body_buffer_size    512K;\n" +
                "}\n";

            string orgFileName = "/etc/nginx/sites-available/" + _appName;
            string symlinkFilename = "/etc/nginx/sites-enabled/" + _appName;

            // write nginx config file;
            ErrorCode errCode = WriteFile(orgFileName, content);
            if (errCode != ErrorCode.NoError)
                return errCode;

            // create symlink to enable vhost config;
            try
            {
                if (File.Exists(symlinkFilename))
                {
                    File.Delete(symlinkFilename);
                }

                _ = File.CreateSymbolicLink(symlinkFilename, orgFileName);
            }
            catch (Exception _e)
            {
                Console.WriteLine("> Exception during creating symlink (nginx): " + _e);
                return ErrorCode.Exception;
            }

            return ErrorCode.NoError;
        }

        public static ErrorCode WriteAppsettingsFile()
        {
            const string appsettingsFilePathMain = "appsettings.Production.json";
            const string appsettingsFilePathPrev = "../prev/" + appsettingsFilePathMain;

            bool f1Exist = File.Exists(appsettingsFilePathMain);
            bool f2Exist = File.Exists(appsettingsFilePathPrev);

            bool f1Dirty = false;
            bool f2Dirty = false;

            Appsettings_ appsettings1 = null;
            Appsettings_ appsettings2 = null;

            if (f1Exist || f2Exist)
            {
                Console.WriteLine("  Appsettings file(s) exist.");

                appsettings1 = LoadExistingAppsettingsFile(appsettingsFilePathMain, f1Exist);
                if (appsettings1 == null)
                {
                    Console.WriteLine("    Appsettings file in Main seems to be corrupted, please check and run Setup again.");
                    return ErrorCode.ObjectIsNull;
                }
                f1Dirty = appsettings1.AppsettingsVersion != Appsettings_.AppsettingsCurrentVersion;

                appsettings2 = LoadExistingAppsettingsFile(appsettingsFilePathPrev, f2Exist);
                if (appsettings2 == null)
                {
                    Console.WriteLine("    Appsettings file in Prev seems to be corrupted, please check and run Setup again.");
                    return ErrorCode.ObjectIsNull;
                }
                f2Dirty = appsettings2.AppsettingsVersion != Appsettings_.AppsettingsCurrentVersion;

                if (!f1Dirty && !f2Dirty)
                {
                    Console.WriteLine("  Appsettings files are up-to-date.");
                    return ErrorCode.NoError;
                }
            }

            Console.WriteLine("  Updating appsettings.Production.json files..");

            JsonSerializerOptions jsonSerializerOptions = P24JsonSerializerContext.JsonSerializerOptionsIndented;

            if (f1Dirty)
            {
                appsettings1.AppsettingsVersion = Appsettings_.AppsettingsCurrentVersion;
                string content = JsonSerializer.Serialize(appsettings1, typeof(Appsettings_), jsonSerializerOptions);

                ErrorCode errCode = WriteFile(appsettingsFilePathMain, content);
                if (errCode != ErrorCode.NoError)
                    return errCode;
            }

            if (f2Dirty)
            {
                appsettings2.AppsettingsVersion = Appsettings_.AppsettingsCurrentVersion;
                appsettings2.Kestrel.Endpoints.Http.Url = "http://localhost:5001";
                string content = JsonSerializer.Serialize(appsettings2, typeof(Appsettings_), jsonSerializerOptions);

                ErrorCode errCode = WriteFile(appsettingsFilePathPrev, content);
                if (errCode != ErrorCode.NoError)
                    return errCode;
            }

            return ErrorCode.NoError;
        }

        private static Appsettings_ LoadExistingAppsettingsFile(string _path, bool _isFileExist = true)
        {
            if (!_isFileExist)
                return new Appsettings_();

            try
            {
                string content = File.ReadAllText(_path);
                return (Appsettings_)JsonSerializer.Deserialize(content, typeof(Appsettings_), P24JsonSerializerContext.JsonSerializerOptionsIndented);
            }
            catch (Exception _e)
            {
                Console.WriteLine("> Exception during reading file at \"" + _path + "\": " + _e);
                return null;
            }
        }

        private static ErrorCode WriteFile(string _filename, string _content)
        {
            try
            {
                StreamWriter writer = new(_filename, false);

                writer.Write(_content);
                writer.Flush();

                writer.Close();
            }
            catch (Exception _e)
            {
                Console.WriteLine("> Exception during writing file \""+ _filename + "\": " + _e);
                return ErrorCode.Exception;
            }

            return ErrorCode.NoError;
        }
    }

}
