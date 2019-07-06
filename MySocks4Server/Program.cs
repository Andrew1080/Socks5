

using NATUPNPLib;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Socks45
{
    class Program
    {
        static void Main(string[] args)
        {
            //Construct1();
            //Construct2();
          //Construct3();
         
            //string localIP = "";
            //var host = Dns.GetHostEntry(Dns.GetHostName());
            //foreach (var ip in host.AddressList)
            //{
            //    if (ip.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        localIP =ip.ToString();
            //    }
            //}

            string localIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork).ToString();
            //  string localIP = GetLocalIPAddress();
            //  string localIP = "192.168.56.1";
            Console.WriteLine(localIP);

            TcpListener TCP_ = null;
           
               
              
                int MaxThreadsCount = Environment.ProcessorCount * 200;
                Console.WriteLine(MaxThreadsCount.ToString());
                ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
                ThreadPool.SetMinThreads(2, 2);

            //UPnPNATClass upnpnat = new UPnPNATClass();
            //NATUPNPLib.IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;
            //mappings.Add(7777, "TCP", 7777, localIP, true, "Test Open Port");

            // Устанавливаем порт 
            Int32 port = 7777;
                IPAddress localAddr = IPAddress.Parse(localIP);
               
                TCP_ = new TcpListener(localAddr, port);

                // Запускаем TcpListener и начинаем слушать клиентов.
                TCP_.Start();
            // Thread.Sleep(500);

          
            try
            {
                while (true)
                {
              
                    Console.Write("\nWaiting for a connection... ");
                    ThreadPool.QueueUserWorkItem(ObrabotkaZaprosa, TCP_.AcceptTcpClient());

                }
            }
            catch (SocketException e)
            {
               
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Останавливаем TcpListener.
                TCP_.Stop();
            }
            Console.ReadKey();
        }
        private static  void ObrabotkaZaprosa(object client_obj)
        {
          
            Thread.Sleep(30);

            TcpClient TCP_PROXY_SITE = null;
            
            
            TcpClient TCP_PROXY_PC = client_obj as TcpClient;


          //  string asd = TCP_PROXY_PC.Client.LocalEndPoint.ToString();
            try
            {
                
                // Получаем информацию от клиента
                NetworkStream STREAM_PROXY_PC = TCP_PROXY_PC.GetStream();

                if (STREAM_PROXY_PC.DataAvailable)
                {
                    Byte[] bytesbase = new Byte[1024];

                    int leng = STREAM_PROXY_PC.Read(bytesbase, 0, 1024);//читаем инфу от клиента (5 1 0)

                    byte[] arr_5_0_0_1_addr = new byte[] { 5, 0, 0, 1, 0, 0, 0, 0, 0, 0 };

                    if (bytesbase[0] == 5 && bytesbase[1] == 1)// если сокс 4 и законнектились (1)
                    {

                        byte[] arr_5_0_0_1 = new byte[2];
                        arr_5_0_0_1[0] = 5;
                        arr_5_0_0_1[1] = 0;

                        STREAM_PROXY_PC.Write(arr_5_0_0_1, 0, arr_5_0_0_1.Length);

                        int lengadr = STREAM_PROXY_PC.Read(bytesbase, 0, 255);//заполняем массив адрессом и портом сайта и считаем его длину

                        int port = 0;
                        if (lengadr > 10)
                        {
                            byte[] bytes = new byte[lengadr];
                            Array.Copy(bytesbase, bytes, lengadr);// копируем массив без нулей

                            byte[] portarr = new byte[2]; //длина порта
                            byte[] hostarr = new byte[bytesbase[4]]; // bytesbase[4] длина байтов хостинга
                            Array.Copy(bytes, bytes[4] + 5, portarr, 0, 2); //коприруем numArray2 в numArray начиная с 3 по счету элемента массива от начала . копируем 2 элемента
                            Array.Copy(bytes, 5, hostarr, 0, bytes[4]);
                            if (portarr[0] == 0)
                            { port = 80; }
                            else
                            {
                                port = portarr[1] + (256 * portarr[0]);// берем номер порта сайта, к которому будет коннектиться прокся, из массива
                            }
                            string hostname = Encoding.ASCII.GetString(hostarr);
                            if (hostname.Contains("ea.com") || hostname.Contains("gamigo"))
                            {
                                Console.WriteLine("Find fucking sites. Closing.");
                                try
                                {
                                    if (TCP_PROXY_PC.Connected)
                                    {
                                        TCP_PROXY_PC.Close();
                                    }
                                    if (TCP_PROXY_SITE != null && TCP_PROXY_SITE.Connected)
                                    {
                                        TCP_PROXY_SITE.Close();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Could not close the tcp connection1 fucking sites: ", e);
                                }
                            }

                            IPAddress[] ipAddress = Dns.GetHostAddresses(hostname);
                            
                            TCP_PROXY_SITE = Method_Connect_Tcp_Proxy_Site(ipAddress[0], port);//метод для коннекта к сайту

                            if (TCP_PROXY_SITE.Connected)
                            {
                                arr_5_0_0_1_addr[1] = 0;
                                STREAM_PROXY_PC.Write(arr_5_0_0_1_addr, 0, arr_5_0_0_1_addr.Length);// отправляем на ПК ответ что с сайтом соединились
                            }
                        }
                        else

                        {
                            byte[] bytes = new byte[10];
                            Array.Copy(bytesbase, bytes, 10);

                            byte[] numArray2 = new byte[2];
                            byte[] numArray3 = new byte[4];
                            Array.Copy(bytes, 8, numArray2, 0, 2); //коприруем numArray2 в numArray начиная с 3 по счету элемента массива от начала . копируем 2 элемента
                            Array.Copy(bytes, 4, numArray3, 0, 4);
                            Array.Reverse(numArray2);

                            if (numArray2[0] == 0)
                            {
                                port = BitConverter.ToInt16(numArray2, 0);
                            }
                            else
                            {
                                port = (numArray2[1] * 256) + numArray2[0];
                            }
                            //// берем номер порта сайта, к которому будет коннектиться прокся, из массива
                            IPAddress ipAddress2 = new IPAddress(numArray3);
                         
                            TCP_PROXY_SITE = Method_Connect_Tcp_Proxy_Site(ipAddress2, port);//метод для коннекта к сайту
                            bytes[1] = 0;
                            //var port1 = bytes[8];
                            //var port2 = bytes[9];
                            //bytes[8] = port2;
                            //bytes[9] = port1;
                            STREAM_PROXY_PC.Write(bytes, 0, bytes.Length);
                        }

                        //  byte[] arr_5_0_0_1_addr = new byte[] { 5, 0, 0, 1, 0, 0, 0, 0,0,0 };
                        if (TCP_PROXY_SITE != null)
                        {
                            if (TCP_PROXY_SITE.Connected)
                            {
                                //Task.Factory.StartNew(() => TunnelTCP(TCP_PROXY_PC, TCP_PROXY_SITE));
                                //Task.Factory.StartNew(() => TunnelTCP(TCP_PROXY_SITE, TCP_PROXY_PC));
                                Thread clientThread = new Thread(() => TunnelTCP(TCP_PROXY_PC, TCP_PROXY_SITE));
                                Thread serverThread = new Thread(() => TunnelTCP(TCP_PROXY_SITE, TCP_PROXY_PC));
                                clientThread.IsBackground = true;
                                serverThread.IsBackground = true;
                                clientThread.Start();
                                serverThread.Start();
                            }
                            else
                            {

                                arr_5_0_0_1_addr[1] = 1;
                                STREAM_PROXY_PC.Write(arr_5_0_0_1_addr, 0, arr_5_0_0_1_addr.Length);// отправляем на ПК ответ что с сайтом НЕ соединились
                                if (STREAM_PROXY_PC != null) STREAM_PROXY_PC.Close();
                                if (TCP_PROXY_PC != null) TCP_PROXY_PC.Close();

                            }
                        }
                        else
                        {

                            arr_5_0_0_1_addr[1] = 1;
                            STREAM_PROXY_PC.Write(arr_5_0_0_1_addr, 0, arr_5_0_0_1_addr.Length);// отправляем на ПК ответ что с сайтом НЕ соединились
                            if (STREAM_PROXY_PC != null) STREAM_PROXY_PC.Close();
                            if (TCP_PROXY_PC != null) TCP_PROXY_PC.Close();

                        }

                    }
                    else
                 if (bytesbase[0] == 4 && bytesbase[1] == 1)// если сокс 4 и законнектились (1)
                    {
                        byte[] bytes = new byte[leng];
                        Array.Copy(bytesbase, bytes, leng);

                        byte[] numArray2 = new byte[2];
                        byte[] numArray3 = new byte[4];
                        Array.Copy(bytes, 2, numArray2, 0, 2); //коприруем numArray2 в numArray начиная с 3 по счету элемента массива от начала . копируем 2 элемента
                        Array.Copy(bytes, 4, numArray3, 0, 4);
                        Array.Reverse(numArray2);
                        int port;

                        if (numArray2[0] == 0)
                        {
                            port = BitConverter.ToInt16(numArray2, 0);
                        }
                        else
                        {
                            port = (numArray2[1] * 256) + numArray2[0];
                        }
                        //// берем номер порта сайта, к которому будет коннектиться прокся, из массива
                        IPAddress ipAddress = new IPAddress(numArray3); // берем IP адрес хостинга , к которому будет коннектиться прокся, из массива

                        IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                        string hostname = entry.HostName;
                        if (hostname.Contains("ea.com") || hostname.Contains("gamigo"))
                        {
                            Console.WriteLine("Find fucking sites. Closing.");
                            try
                            {
                                if (TCP_PROXY_PC.Connected)
                                {
                                    TCP_PROXY_PC.Close();
                                }
                                if (TCP_PROXY_SITE != null && TCP_PROXY_SITE.Connected)
                                {
                                    TCP_PROXY_SITE.Close();
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Could not close the tcp connection1 fucking sites: ", e);
                            }
                        }
                        byte[] arr_90_91 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                  
                        TCP_PROXY_SITE = Method_Connect_Tcp_Proxy_Site(ipAddress, port);//метод для коннекта к сайту

                        if (TCP_PROXY_SITE.Connected)
                        {
                            arr_90_91[1] = 90;

                            STREAM_PROXY_PC.Write(arr_90_91, 0, arr_90_91.Length);// отправляем на ПК ответ что с сайтом соединились

                            //Task.Factory.StartNew(() => TunnelTCP(TCP_PROXY_PC, TCP_PROXY_SITE));
                            //Task.Factory.StartNew(() => TunnelTCP(TCP_PROXY_SITE, TCP_PROXY_PC));

                            Thread clientThread = new Thread(() => TunnelTCP(TCP_PROXY_PC, TCP_PROXY_SITE));
                            Thread serverThread = new Thread(() => TunnelTCP(TCP_PROXY_SITE, TCP_PROXY_PC));
                            clientThread.IsBackground = true;
                            serverThread.IsBackground = true;
                            clientThread.Start();
                            serverThread.Start();
                        }
                        else
                        {

                            arr_90_91[1] = 91;
                            STREAM_PROXY_PC.Write(arr_90_91, 0, arr_90_91.Length);// отправляем на ПК ответ что с сайтом НЕ соединились
                            if (STREAM_PROXY_PC != null) STREAM_PROXY_PC.Close();
                            if (TCP_PROXY_PC != null) TCP_PROXY_PC.Close();
                        }

                    }
                    else
                 if (System.Text.Encoding.UTF8.GetString(bytesbase).Contains("HTTP"))
                    {
                        string Query = System.Text.Encoding.UTF8.GetString(bytesbase);
                        var arr_result = System.Text.Encoding.UTF8.GetString(bytesbase).Split(' ');
                        if (arr_result[0] == "CONNECT")
                        {

                            string host = arr_result[1].Split(':')[0];

                            if (host.Contains("ea.com") || host.Contains("gamigo"))
                            {
                                Console.WriteLine("Find fucking sites. Closing.");
                                try
                                {
                                    if (TCP_PROXY_PC.Connected)
                                    {
                                        TCP_PROXY_PC.Close();
                                    }
                                    if (TCP_PROXY_SITE != null && TCP_PROXY_SITE.Connected)
                                    {
                                        TCP_PROXY_SITE.Close();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Could not close the tcp connection1 fucking sites: ", e);
                                }
                            }
                           
                             int port = Convert.ToInt32(arr_result[1].Split(':')[1]);
                            TCP_PROXY_SITE = new TcpClient(host, port);//метод для коннекта к сайту
                            if (TCP_PROXY_SITE.Connected)
                            {
                                STREAM_PROXY_PC = TCP_PROXY_PC.GetStream();
                                string brs = "HTTP/1.1 200 Connection established\r\n\r\n";
                                STREAM_PROXY_PC.Write(Encoding.ASCII.GetBytes(brs), 0, brs.Length);// отправляем на ПК ответ что с сайтом НЕ соединились

                                //Byte[] bytesbase1 = new Byte[256];
                                //int leng1 =  STREAM_PROXY_PC.Read(bytesbase1, 0, 256);//читаем инфу от клиента (5 1 0)

                                //string asdd = System.Text.Encoding.UTF8.GetString(bytesbase1);

                                //Task.Factory.StartNew(() => TunnelTCP(TCP_PROXY_PC, TCP_PROXY_SITE));
                                //Task.Factory.StartNew(() => TunnelTCP(TCP_PROXY_SITE, TCP_PROXY_PC));
                                Thread clientThread = new Thread(() => TunnelTCP(TCP_PROXY_PC, TCP_PROXY_SITE));
                                Thread serverThread = new Thread(() => TunnelTCP(TCP_PROXY_SITE, TCP_PROXY_PC));
                                clientThread.IsBackground = true;
                                serverThread.IsBackground = true;
                                clientThread.Start();
                                serverThread.Start();
                            }

                        }

                        else
                        {


                            string HttpRequestType = "";
                            string HttpVersion = "";
                            string RequestedPath = "";
                            string m_HttpPost = "";
                            StringDictionary retdict = new StringDictionary();
                            string[] Lines = Query.Replace("\r\n", "\n").Split('\n');
                            int Cnt, Ret;
                            //Extract requested URL
                            if (Lines.Length > 0)
                            {
                                //Parse the Http Request Type
                                Ret = Lines[0].IndexOf(' ');
                                if (Ret > 0)
                                {
                                    HttpRequestType = Lines[0].Substring(0, Ret);
                                    Lines[0] = Lines[0].Substring(Ret).Trim();
                                }
                                //Parse the Http Version and the Requested Path
                                Ret = Lines[0].LastIndexOf(' ');
                                if (Ret > 0)
                                {
                                    HttpVersion = Lines[0].Substring(Ret).Trim();
                                    RequestedPath = Lines[0].Substring(0, Ret);
                                }
                                else
                                {
                                    RequestedPath = Lines[0];
                                }
                                //Remove http:// if present
                                if (RequestedPath.Length >= 7 && RequestedPath.Substring(0, 7).ToLower().Equals("http://"))
                                {
                                    Ret = RequestedPath.IndexOf('/', 7);
                                    if (Ret == -1)
                                        RequestedPath = "/";
                                    else
                                        RequestedPath = RequestedPath.Substring(Ret);
                                }

                            }
                            for (Cnt = 1; Cnt < Lines.Length; Cnt++)
                            {
                                Ret = Lines[Cnt].IndexOf(":");
                                if (Ret > 0 && Ret < Lines[Cnt].Length - 1)
                                {
                                    try
                                    {
                                        retdict.Add(Lines[Cnt].Substring(0, Ret), Lines[Cnt].Substring(Ret + 1).Trim());
                                    }
                                    catch { }
                                }
                            }
                            var HeaderFields = retdict;

                            if (HeaderFields == null || !HeaderFields.ContainsKey("Host"))
                            {
                                string brs = "HTTP/1.1 400 Bad Request\r\nConnection: close\r\nContent-Type: text/html\r\n\r\n<html><head><title>400 Bad Request</title></head><body><div align=\"center\"><table border=\"0\" cellspacing=\"3\" cellpadding=\"3\" bgcolor=\"#C0C0C0\"><tr><td><table border=\"0\" width=\"500\" cellspacing=\"3\" cellpadding=\"3\"><tr><td bgcolor=\"#B2B2B2\"><p align=\"center\"><strong><font size=\"2\" face=\"Verdana\">400 Bad Request</font></strong></p></td></tr><tr><td bgcolor=\"#D1D1D1\"><font size=\"2\" face=\"Verdana\"> The proxy server could not understand the HTTP request!<br><br> Please contact your network administrator about this problem.</font></td></tr></table></center></td></tr></table></div></body></html>";

                                STREAM_PROXY_PC.Write(Encoding.ASCII.GetBytes(brs), 0, brs.Length);// отправляем на ПК ответ что с сайтом НЕ соединились
                                new Exception();
                            }
                            int Port;
                            string Host;

                            //Normal HTTP
                            Ret = ((string)HeaderFields["Host"]).IndexOf(":");
                            if (Ret > 0)
                            {
                                Host = ((string)HeaderFields["Host"]).Substring(0, Ret);
                                Port = int.Parse(((string)HeaderFields["Host"]).Substring(Ret + 1));
                            }
                            else
                            {
                                Host = (string)HeaderFields["Host"];
                                Port = 80;
                            }
                            if (HttpRequestType.ToUpper().Equals("POST"))
                            {
                                int index = Query.IndexOf("\r\n\r\n");
                                m_HttpPost = Query.Substring(index + 4);
                            }

                            string ret = HttpRequestType + " " + RequestedPath + " " + HttpVersion + "\r\n";
                            if (HeaderFields != null)
                            {
                                foreach (string sc in HeaderFields.Keys)
                                {
                                    if (sc.Length < 6 || !sc.Substring(0, 6).Equals("proxy-"))
                                        ret += System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(sc) + ": " + (string)HeaderFields[sc] + "\r\n";
                                }
                                ret += "\r\n";
                                if (m_HttpPost != null)
                                    ret += m_HttpPost;
                            }

                            TCP_PROXY_SITE = new TcpClient(Host, Port);//метод для коннекта к сайту
                            if (TCP_PROXY_SITE.Connected)
                            {
                                NetworkStream STREAM_PROXY_SITE = TCP_PROXY_SITE.GetStream();
                                STREAM_PROXY_SITE.Write(Encoding.ASCII.GetBytes(ret), 0, ret.Length);// отправляем на ПК ответ что с сайтом НЕ соединились

                                Thread clientThread = new Thread(() => TunnelTCP(TCP_PROXY_PC, TCP_PROXY_SITE));
                                Thread serverThread = new Thread(() => TunnelTCP(TCP_PROXY_SITE, TCP_PROXY_PC));
                                clientThread.IsBackground = true;
                                serverThread.IsBackground = true;
                                clientThread.Start();
                                serverThread.Start();
                            }
                        }

                    }

                    else
                    {
                        TCP_PROXY_PC.Close();
                    }

                }
            }
            catch (Exception)
            {
                // Disconnent if connections still alive
               // Console.WriteLine("Closing TCP connection.");
                try
                {
                    if (TCP_PROXY_PC.Connected)
                    {
                        TCP_PROXY_PC.Close();
                    }
                    if (TCP_PROXY_SITE != null && TCP_PROXY_SITE.Connected)
                    {
                        TCP_PROXY_SITE.Close();
                    }
                }
                catch (Exception e)
                {
                  //  Console.WriteLine("Could not close the tcp connection1: ", e);
                }
            }
        }

        private static TcpClient Method_Connect_Tcp_Proxy_Site(IPAddress ipadr, int port)
        {
            try
            {
                return new TcpClient(ipadr.ToString(), port);// тут прокси сервер коннектится к IP адрессу и порту сайта
            }
            catch
            {
                return null;
            }
        }

        //private static bool Network_Str_Proxy_PC(NetworkStream prox_site_stream, NetworkStream prox_pc_stream, TcpClient pcclient, TcpClient siteclient)
        //{

        //    if (prox_pc_stream.DataAvailable)
        //    {
        //        int num = 0;
        //        byte[] numArray = new byte[8000];
        //        byte[] numArray1 = null;
        //        num = prox_pc_stream.Read(numArray, 0, numArray.Length);// читаем данные с нашего ПК
        //                                                                //Thread.Sleep(3000);
        //                                                                //  string Type = System.Text.Encoding.ASCII.GetString(numArray, 0, num);
        //                                                                //string returnData = Encoding.UTF8.GetString(numArray);
        //                                                                //  File.AppendAllText(@"C:\Users\AndrewAndrew\Desktop\RES.txt", Type);

        //        if (num > 0)
        //        {
        //            numArray1 = new byte[num];
        //            Array.Copy(numArray, numArray1, num);//копируем в другой массив байт с первого массива только заполненые ячейки, избавляясь от пустых ячеек из первого массива
                   
        //             prox_site_stream.Write(numArray1, 0, num);//отправляем хедеры и прочую инфу на сайт
                    
        //            return true;
        //        }

        //        else
        //        {
        //            return false;
        //        }


        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        //private static bool Network_Str_Proxy_Site(NetworkStream prox_site_stream, NetworkStream prox_pc_stream, TcpClient pcclient, TcpClient siteclient)
        //{

        //    if (prox_site_stream.DataAvailable)
        //    {
        //        int num = 0;
        //        byte[] numArray2 = new byte[8000];
        //        byte[] numArray3 = null;
        //        num = prox_site_stream.Read(numArray2, 0, numArray2.Length);
        //        if (num > 0)
        //        {
        //            numArray3 = new byte[num];
        //            Array.Copy(numArray2, numArray3, num);


        //            //  string Type = System.Text.Encoding.UTF8.GetString(numArray3, 0, num);
                 
        //                prox_pc_stream.Write(numArray3, 0, num);
                    

        //            return true;
        //        }

        //        else
        //        {
        //            return false;
        //        }


        //    }
        //    else
        //    {
        //        return true;
        //    }

        //}


        static  void TunnelTCP(TcpClient inClient, TcpClient outClient)
        {

            NetworkStream inStream = null;
            NetworkStream outStream = null;
            
            try
            {
                  while (true)
                    {

                        inStream = inClient.GetStream();
                    

                        outStream = outClient.GetStream();
                      

                        byte[] buffer = new byte[4096];
                        int read;

                        read = inStream.Read(buffer, 0, buffer.Length);
                        if (read != 0)
                        {
                        outStream.Write(buffer, 0, read);
                        }

                        if (read == 0)
                        { break; }
                    
                    }
                
            }
            catch (Exception e)
            {

                Console.WriteLine("TCP connection error: ", e);
            }
            finally
            {
                Console.WriteLine("Closing TCP connection.");
                // Disconnent if connections still alive
                try
                {
                    if (inClient.Connected)
                    {
                        inClient.Close();
                         inStream.Close();
                    }
                    if (outClient.Connected)
                    {
                        outClient.Close();
                         outStream.Close();
                    }
                }
                catch (Exception e1)
                {
                    Console.WriteLine("Could not close the tcp connection2: ", e1);
                }
            }
         
        }


        public static void Construct1()
        {
            var action = new Process
            {
                StartInfo =
                                    {
                                        Verb = "runas",
                                        UseShellExecute = false,
                                        FileName = "netsh.exe",
                                        RedirectStandardError = true,
                                        RedirectStandardOutput = true,
                                        CreateNoWindow = true,
                                        WindowStyle = ProcessWindowStyle.Hidden,
                                        Arguments =  "advfirewall set privateprofile state off"
                    }
            };

            action.Start();
            action.WaitForExit(5 * 1000);
        }
        public static void Construct2()
        {
            var action = new Process
            {
                StartInfo =
                                    {
                                        Verb = "runas",
                                        UseShellExecute = false,
                                        FileName = "netsh.exe",
                                        RedirectStandardError = true,
                                        RedirectStandardOutput = true,
                                        CreateNoWindow = true,
                                        WindowStyle = ProcessWindowStyle.Hidden,
                                        Arguments =  "advfirewall set  currentprofile state off"
                    }
            };

            action.Start();
            action.WaitForExit(5 * 1000);
        }

        public static void Construct3()
        {
            var action = new Process
            {
                StartInfo =
                                    {
                                        Verb = "runas",
                                        UseShellExecute = false,
                                        FileName = "netsh.exe",
                                        RedirectStandardError = true,
                                        RedirectStandardOutput = true,
                                        CreateNoWindow = true,
                                        WindowStyle = ProcessWindowStyle.Hidden,
                                        Arguments =  @"advfirewall firewall add rule name=""My Application"" dir=in action=allow program=""C:\Users\Andrew\Desktop\Проекты\Socks45\MySocks4Server\bin\Debug\MySocks4Server.exe"" enable=yes"
                    }
            };

            action.Start();
            action.WaitForExit(5 * 1000);
        }
    }
}

