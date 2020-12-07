using System;
using System.Management.Automation;
using System.Management;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using MatthiWare.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace Logger
{
    class Program
    {
        static void Main(string[] args)
        {
            ParseCommand(Console.ReadLine().Split(" "));
        }
        static string ip;
        static string commandLocation;
        static string outputLocation;
        public static void ParseCommand(string[] args)
        {
            var options = new CommandLineParserOptions
            {
                AppName = "TCP Tracker",
            };

            var parser = new CommandLineParser<ScriptOptions>(options);

            var result = parser.Parse(args);

            if (result.HasErrors)
            {
                ParseCommand(Console.ReadLine().Split(" "));
                return;
            }

            var programOptions = result.Result;

            //RunCommand();
            commandLocation = programOptions.commandLocation;
            outputLocation = programOptions.outputLocation;
            ip = programOptions.IpAdress;
            var timer = new Timer(test, null, 0, programOptions.time*1000*60);
            Console.Read();
        }

        public static void RunCommand(string commandLocation, string ipAddress, string outputLocation)
        {
            string command = File.ReadAllText(commandLocation);

            //Console.WriteLine(RunScript(command));
            string output = RunScript(command);

            string[] lines = Regex.Split(output, "\r\n|\r|\n");

            List<string> lines_list = lines.ToList();

            //remove the header

            outputInfo info = new outputInfo();
            int argsCount = 0;
            foreach (string line in lines_list)
            {
                if (line == "")
                    continue;

                switch(argsCount)
                {
                    case 0:
                        info = new outputInfo();
                        info.remoteIP = IPAddress.Parse(line.Substring(line.IndexOf(':') + 2));
                        break;
                    case 1:
                        info.remotePort = Convert.ToInt32(line.Substring(line.IndexOf(':') + 2));
                        break;
                    case 2:
                        info.PID = Convert.ToInt32(line.Substring(line.IndexOf(':') + 2));
                        break;
                    case 3:
                        info.process_name = line.Substring(line.IndexOf(':') + 2);
                        break;
                    case 4:
                        info.username = line.Substring(line.IndexOf(':') + 2);
                        break;
                }
                
                argsCount++;
                if(argsCount==5)
                {
                    argsCount = 0;
                    if (info.remoteIP.ToString() == ipAddress)
                    {
                        string outputString = $"[{DateTime.Now}] Ip address: {info.remoteIP}; remote port: {info.remotePort}; PID: {info.PID}; Process name: {info.process_name}; username: {info.username}";
                        Console.WriteLine(outputString);
                        //log into the file
                        using (StreamWriter writer = File.AppendText(outputLocation))
                        {
                            writer.WriteLine(outputString);
                        }
                    }
                }
            }
            Console.ReadKey();
        }

        static void test(object o)
        {
            RunCommand(commandLocation, ip,outputLocation);

            GC.Collect();
        }

        public static string Command(string[] arguments)
        {
            if(arguments[0]=="run")
            {
                return "correct";
            }
            else
            {
                return "Command not found. Try run 'help'";
            }
        }

        private static string RunScript(string scriptText)
        {
            // create Powershell runspace

            Runspace runspace = RunspaceFactory.CreateRunspace();

            // open it

            runspace.Open();

            // create a pipeline and feed it the script text

            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(scriptText);

            // add an extra command to transform the script
            // output objects into nicely formatted strings

            // remove this line to get the actual objects
            // that the script returns. For example, the script

            // "Get-Process" returns a collection
            // of System.Diagnostics.Process instances.

            pipeline.Commands.Add("Out-String");

            // execute the script

            Collection<PSObject> results = pipeline.Invoke();

            // close the runspace

            runspace.Close();

            // convert the script result into a single string

            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.AppendLine(obj.ToString());
            }

            return stringBuilder.ToString();
        }

        public class outputInfo
        {
            public IPAddress remoteIP { get; set; }
            public int remotePort { get; set; }
            public int PID { get; set; }
            public string process_name { get; set; }
            public string username { get; set; }

        }
    }
}
