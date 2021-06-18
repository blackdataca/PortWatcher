using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;

namespace PortWatcher
{
    class Connection : IEquatable<Connection>
    {
        public DateTime TimeStamp { get; private set; }
        public TcpConnectionInformation Conn { get; private set; }
        public Connection(TcpConnectionInformation conn)
        {
            TimeStamp = DateTime.Now;
            Conn = conn;
        }

  
        public override string ToString() 
        {
                return string.Format("{0:yyyy-MM-dd HH:mm:ss}\t{1} <==> {2} ({3})", TimeStamp, Conn.LocalEndPoint.ToString(), Conn.RemoteEndPoint.ToString(), Conn.State);
        }
        public override bool Equals(object obj) => this.Equals(obj as Connection);

        public bool Equals(Connection other)
        {
            if (other is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != other.GetType())
            {
                return false;
            }
            return (Conn.LocalEndPoint.Address.ToString() == other.Conn.LocalEndPoint.Address.ToString() 
                && Conn.RemoteEndPoint.Address.ToString() == other.Conn.RemoteEndPoint.Address.ToString() 
                 && Conn.State == other.Conn.State);
        }

        public override int GetHashCode() => (Conn).GetHashCode();

        public static bool operator ==(Connection lhs, Connection rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Connection lhs, Connection rhs) => !(lhs == rhs);
    }
    class Program
    {
        /// <summary>
        /// Show command line hint
        /// </summary>
        static void ShowHelp()
        {

            Console.WriteLine("\nUsage: PortWatcher.exe -p PortNumber [-e EmailAddress [-s SmtpSerever[:Port] [-user UserName -pass Password] [-ssl] ] ]\n");
            Console.WriteLine("-p PortNumber,PortNumber,... = The IP or UDP port number either local or remote. No space.");
            Console.WriteLine("-e EmailAddress = Email address to receive notification");
            Console.WriteLine("-s SmtpServer:Port = Smtp server address and port. Default 127.0.0.1:25 if not specified");
            Console.WriteLine("-user Smtp User Name = User name for Smtp server");
            Console.WriteLine("-pass Smtp Password = Password for Smtp server");
            Console.WriteLine("-ssl = Enable SSL for Smtp server");

            Console.Write("Press Enter to quit");
            Console.ReadLine();
        }

        static string[] _ports; //List of ports to monitor
        static string _email = ""; //Notification email address
        static string _smtpAddr = "127.0.0.1"; //Email server address
        static int _smtpPort = 25; //Email server port
        static bool _enableSsl = false; //To support SSL SMTP, set to true, otherwise false
        static string _smtpUser = ""; //SMTP server login user. "" is anonymous
        static string _smtpPass = ""; //SMTP server password

        /// <summary>
        /// Parse command line parameters
        /// </summary>
        /// <param name="args"></param>
        /// <returns>true = parameters parsed successfuly. false = one or more required parameter not set</returns>
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


        /// <summary>
        /// Program entry point. Start in infinite loop until Ctrl-C to quit. Scan interval 1 second.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            Console.WriteLine(string.Format("{0} {1}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version));
            if (!ParseArgs(args))
            {
                ShowHelp();
                return 1;
            }


            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

            var oldConnections = new List<Connection>();

            while (true)
            {
                try
                {
                    var newConnections = new List<Connection>();

                    TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

                    
                    foreach (TcpConnectionInformation c in tcpConnections)
                    {
                        foreach (string port in _ports)
                        {
                            if (c.LocalEndPoint.Port.ToString() == port || c.RemoteEndPoint.Port.ToString() == port)
                            {
                           //     string connection = string.Format("{0} <==> {1} ({2})",
                           //c.LocalEndPoint.ToString(),
                           //c.RemoteEndPoint.ToString(), c.State);
                                var newConn = new Connection(c);
                                bool found = false;
                                foreach (var conn in oldConnections)
                                {
                                    if (conn == newConn)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    //Get new connections
                                    newConnections.Add(newConn);
                                    oldConnections.Add(newConn);
                                }
                            }
                        }
                    }

                    if (newConnections.Count>0)
                    {
                        Console.Beep();
                        Console.WriteLine("Connections: {0}", newConnections.Count);
                        foreach (var connection in newConnections)
                            Console.WriteLine(connection);

                        SendNotification(newConnections);
                    }
                }
                catch (Exception ex)
                {
                    //Display any exception then continue 
                    Console.WriteLine(ex.ToString());
                }
                System.Threading.Thread.Sleep(2000);
            } //end while
        }


        /// <summary>
        /// Sends email notification if email address is set
        /// </summary>
        /// <param name="newConnections"></param>
        static void SendNotification(List<Connection> newConnections)
        {
            if (string.IsNullOrEmpty(_email))
                return;

            Console.Write("Sending email...");
            try
            {
                using (MailMessage message = new MailMessage())
                {
                    SmtpClient smtp = new SmtpClient();
                    message.From = new MailAddress(_email);
                    message.To.Add(new MailAddress(_email));
                    message.Subject = string.Format("PortWatch Notification Connections {0} {1:yyyy-MM-dd HH:mm:ss}", newConnections.Count, DateTime.Now); //vary subject to prevent gmail topic grouping
                    smtp.Port = _smtpPort;
                    smtp.Host = _smtpAddr;
                    smtp.EnableSsl = _enableSsl;
                    smtp.UseDefaultCredentials = string.IsNullOrEmpty(_smtpUser);
                    smtp.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    message.IsBodyHtml = false;

                    var bodyString = new StringBuilder();
                    bodyString.AppendLine("Time\tLocal <==> Remote (Status)");
                    foreach (var conn in newConnections)
                    {
                        bodyString.AppendLine(conn.ToString());
                    }
                    message.Body = bodyString.ToString();
                    smtp.Send(message);

                    Console.WriteLine("Sent.");
                }
            }
            catch (Exception ex)
            {
                //Display any exception then continue 
                Console.WriteLine(ex.ToString());
            }
        }
    }
}



