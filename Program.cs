using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;

namespace PortWatcher
{
    class Program
    {

        static void ShowHelp()
        {

            Console.WriteLine("Usage: PortWatcher -p PortNumber [-e EmailAddress [-s SmtpSerever[:Port] [-user UserName -pass Password] [-ssl] ] ]");
            Console.WriteLine("-p PortNumber,PortNumber,... = The IP or UDP port number either local or remote. No space.");
            Console.WriteLine("-e EmailAddress = Email address to receive notification");
            Console.WriteLine("-s SmtpServer:Port = Smtp server address and port. Default 127.0.0.1:25 if not specified");
            Console.WriteLine("-user Smtp User Name = User name for Smtp server");
            Console.WriteLine("-pass Smtp Password = Password for Smtp server");
            Console.WriteLine("-ssl = Enable SSL for Smtp server");
        }

        static string[] _ports;
        static string _email = "";
        static string _smtpAddr = "127.0.0.1";
        static int _smtpPort = 25;
        static bool _enableSsl = false;
        static string _smtpUser = "";
        static string _smtpPass = "";

        static bool ParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                switch (arg.Trim().ToLower())
                {
                    case "-p":
                        if (args.Length > i)
                        {
                            string ports = args[i + 1].Trim();
                            _ports = ports.Split(',');
                        }
                        break;
                    case "-e":
                        if (args.Length > i)
                        {
                            _email = args[i + 1].Trim();
                        }
                        break;
                    case "-s":
                        if (args.Length > i)
                        {

                            _smtpAddr = args[i + 1].Trim();
                            int pos = _smtpAddr.IndexOf(":");
                            if (pos > 0 && pos < _smtpAddr.Length)
                            {
                                _smtpPort = int.Parse(_smtpAddr.Substring(pos + 1));
                            }
                        }
                        break;
                    case "-user":
                        if (args.Length > i)
                        {
                            _smtpUser = args[i + 1].Trim();
                        }
                        break;
                    case "-pass":
                        if (args.Length > i)
                        {
                            _smtpPass = args[i + 1].Trim();
                        }
                        break;
                    case "-ssl":
                        _enableSsl = true;
                        break;
                    default:
                        break;
                }

            }
            if (_ports == null || _ports.Length < 1)
            {
                return false;
            }

            Console.WriteLine("Monitoring ports: " + string.Join(",",_ports));
            Console.WriteLine("Email: " + _email);
            Console.WriteLine("Smtp: {0}:{1}", _smtpAddr, _smtpPort);
            Console.WriteLine("Smtp EnableSSL: {0}", _enableSsl);
            return true;
        }



        static int Main(string[] args)
        {
            Console.WriteLine(string.Format("{0} {1}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version));
            if (!ParseArgs(args))
            {
                ShowHelp();
                return 1;
            }


            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

            // IPEndPoint[] endPoints = ipProperties.GetActiveTcpListeners();
            var oldConnections = new List<string>();


            while (true)
            {
                try
                {
                    var newConnections = new List<string>();

                    TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

                    foreach (TcpConnectionInformation c in tcpConnections)
                    {
                        foreach (string port in _ports)
                        {
                            if (c.LocalEndPoint.Port.ToString() == port || c.RemoteEndPoint.Port.ToString() == port)
                            {
                                string connection = string.Format("{0} <==> {1} ({2})",
                           c.LocalEndPoint.ToString(),
                           c.RemoteEndPoint.ToString(), c.State);
                                newConnections.Add(connection);
                            }
                        }
                    }
                    if (!Enumerable.SequenceEqual(oldConnections, newConnections))
                    {
                        Console.Beep();
                        Console.WriteLine("Connections: {0}", newConnections.Count);
                        foreach (var connection in newConnections)
                            Console.WriteLine(connection);

                        if (newConnections.Count > oldConnections.Count)
                        {
                            SendNotification(newConnections);
                        }

                        oldConnections = newConnections.GetRange(0, newConnections.Count);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        static void SendNotification(List<string> newConnections)
        {
            if (string.IsNullOrEmpty(_email))
                return;

            Console.Write("Sending email...");
            using (MailMessage message = new MailMessage())
            {
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(_email);
                message.To.Add(new MailAddress(_email));
                message.Subject = string.Format("PortWatch Notification Connections {0}", newConnections.Count); //vary subject to prevent gmail topic grouping
                smtp.Port = _smtpPort;
                smtp.Host = _smtpAddr;
                smtp.EnableSsl = _enableSsl;
                smtp.UseDefaultCredentials = string.IsNullOrEmpty(_smtpUser);
                smtp.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                message.IsBodyHtml = false;

                var bodyString = new StringBuilder();
                foreach (var conn in newConnections)
                {
                    bodyString.AppendLine(conn);
                }
                message.Body = bodyString.ToString();
                smtp.Send(message);

                Console.WriteLine("Sent.");
            }
        }
    }
}



