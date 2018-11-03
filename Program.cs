using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PortPing
{
    class Program
    {
        static void Main(string[] args)
        {
            var argsParsed = ParseArgs(args);
            var hostname = argsParsed.Hostname;
            var port = argsParsed.Port;

            if (!HostnameResolvable(hostname))
            {
                Console.WriteLine("Hostname is not resolvable!");
                Environment.Exit(1);
            }

            double requests = 0;
            double responses = 0;
            double percentage = 0.0;

            Console.CancelKeyPress += delegate
            {
                Console.WriteLine("\r\n=== Statistics ===");
                if (requests > 0)
                {
                    percentage = (responses / requests) * 100;
                }
                else
                {
                    percentage = 0;
                }

                Console.WriteLine("Requests:   " + requests.ToString());
                Console.WriteLine("Responses:  " + responses.ToString());
                Console.WriteLine("Percentage: " + Math.Round(percentage, 2).ToString() + " %");
            };

            while (true)
            {
                var result = CheckPort(hostname, port);
                requests++;
                if (result)
                {
                    responses++;
                    Console.WriteLine("[ OK ] Request=" + requests.ToString() + " Response from " + hostname + ":" + port.ToString());
                    Thread.Sleep(1000);
                }
                else
                {
                    Console.WriteLine("[FAIL] " + hostname + ":" + port.ToString() + " not responding");
                }
            }
        }

        static private ConnectionArgs ParseArgs(string[] args)
        {
            var hostname = "";
            var port = 0;

            if (args.Length < 2)
            {
                Console.WriteLine("Too less arguments!");
                PrintUsageAndExit();
            }

            hostname = args[0];

            try
            {
                port = Convert.ToInt32(args[1]);
            }
            catch (Exception)
            {
                Console.WriteLine("Port must be a number!");
                PrintUsageAndExit();
            }
            return new ConnectionArgs(hostname, port);
        }

        static private void PrintUsageAndExit()
        {
            Console.WriteLine("Usage: portping.exe <hostname> <port>");
            Environment.Exit(1);
        }

        static private bool HostnameResolvable(string hostname)
        {
            var status = false;
            if (!IsIPv4Address(hostname))
            {
                try
                {
                    hostname = Dns.GetHostEntry(hostname).AddressList[0].ToString();
                    if (IsIPv4Address(hostname))
                    {
                        status = true;
                    }
                }
                catch
                {
                    return status;
                }
            }
            return status;
        }

        static private bool CheckPort(string hostname, int port)
        {
            var stopWatch = Stopwatch.StartNew();

            var status = false;

            var client = new TcpClient();
            var result = client.BeginConnect(hostname, port, null, null);

            result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
            if (!client.Connected)
            {
                stopWatch.Stop();
                var diff = 1000 - stopWatch.ElapsedMilliseconds;
                if (diff > 0)
                {
                    Thread.Sleep(Convert.ToInt32(diff));
                }
                return false;
            }

            status = true;
            client.EndConnect(result);

            client.Close();

            return status;
        }

        static private bool IsIPv4Address(string input)
        {
            var pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            var match = System.Text.RegularExpressions.Regex.Match(input, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return match.Success;
        }
    }
}
