using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Xml;

namespace CrossfireConnect
{
    public class CrossFire
    {
        public static IPAddress host = IPAddress.Parse("127.0.0.1");
        public static int port = 5000;
        public static string HANDSHAKE_STRING = "CrossfireHandshake";
        public static string TOOL_STRING = "net"; //"console,net,inspector,dom";//

        public static string ConnectionStatus = "Disconnected";

        private static Socket socket;
        private static NetworkStream networkStream;

        public static event EventHandler Connected;
        public static event EventHandler Error;
        public static string LastError;

        public delegate void StringHandler(string data);
        public static event StringHandler NewData;

        public static void Connect()
        {
            new Thread(
                delegate()
                {
                    try
                    {
                        if (socket != null) socket.Close();
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(host, port);

                        if (!socket.Connected)
                        {
                            // Connection failed, try next IPaddress.
                            ConnectionStatus = "Disconnected";
                            LastError = "Unable to connect to host";
                            if (Error != null) Error(null, EventArgs.Empty);
                            return;
                        }
                        else
                        {
                            ConnectionStatus = "Connected";
                            if (Connected != null) Connected(null, EventArgs.Empty);

                            Encoding ASCII = Encoding.ASCII;
                            socket.Send(ASCII.GetBytes(HANDSHAKE_STRING));
                            socket.Send(ASCII.GetBytes("\r\n"));
                            socket.Send(ASCII.GetBytes(TOOL_STRING));
                            socket.Send(ASCII.GetBytes("\r\n"));


                            networkStream = new NetworkStream(socket, true);
                            //networkStream.Write(ASCII.GetBytes(CrossFire.HANDSHAKE_STRING)

                            Byte[] RecvBytes = new Byte[256];
                            int bytes;
                            while ((bytes = networkStream.Read(RecvBytes, 0, RecvBytes.Length)) > 0)
                            {
                                if (NewData != null) NewData(ASCII.GetString(RecvBytes, 0, bytes));
                            }


                            /*  Byte[] RecvBytes = new Byte[256];

                              Int32 bytes = socket.Receive(RecvBytes, RecvBytes.Length, 0);
                              if (NewData != null) NewData(ASCII.GetString(RecvBytes, 0, bytes));

                              while (bytes > 0)
                              {
                                  bytes = socket.Receive(RecvBytes, RecvBytes.Length, 0);
                                  if (NewData != null) NewData(ASCII.GetString(RecvBytes, 0, bytes));
                              }*/
                            networkStream.Close();
                        }
                    } // End of the try block.



                    catch (SocketException ex)
                    {
                        LastError = "SocketException caught!!!";
                        LastError += "\nSource : " + ex.Source;
                        LastError += "\nMessage : " + ex.Message;
                        LastError += "\nErrorCode : " + ex.ErrorCode.ToString() + " - " + ex.SocketErrorCode.ToString(); ;
                        if (Error != null) Error(null, EventArgs.Empty);
                    }
                    catch (ArgumentNullException ex2)
                    {
                        LastError = "ArgumentNullException caught!!!";
                        LastError += "\nSource : " + ex2.Source;
                        LastError += "\nMessage : " + ex2.Message;
                        if (Error != null) Error(null, EventArgs.Empty);
                    }
                    catch (NullReferenceException ex3)
                    {
                        LastError = "NullReferenceException caught!!!";
                        LastError += "\nSource : " + ex3.Source;
                        LastError += "\nMessage : " + ex3.Message;
                        if (Error != null) Error(null, EventArgs.Empty);
                    }
                    catch (Exception ex4)
                    {
                        LastError = "Exception caught!!!";
                        LastError += "\nSource : " + ex4.Source;
                        LastError += "\nMessage : " + ex4.Message;
                        if (Error != null) Error(null, EventArgs.Empty);
                    }


                }).Start();


        }

        public static void Disconnect()
        {
            networkStream.Close();
            //socket.Close();
        }

        public static void SendData(string data)
        {
            new Thread(
                delegate()
                {
                    try
                    {
                        if (ConnectionStatus == "Connected")
                        {
                            Encoding ASCII = Encoding.UTF8;
                            string s = "Content-Length:" + (data.Length).ToString() + "\r\n\r\n" + data + "\r\n";
                            // socket.Send(ASCII.GetBytes(s));
                            byte[] buffer = ASCII.GetBytes(s);
                            networkStream.Write(buffer, 0, buffer.Length);

                        }
                    } // End of the try block.



                    catch (SocketException ex)
                    {
                        LastError = "SocketException caught!!!";
                        LastError += "\nSource : " + ex.Source;
                        LastError += "\nMessage : " + ex.Message;
                        if (Error != null) Error(null, EventArgs.Empty);
                    }
                    catch (ArgumentNullException ex2)
                    {
                        LastError = "ArgumentNullException caught!!!";
                        LastError += "\nSource : " + ex2.Source;
                        LastError += "\nMessage : " + ex2.Message;
                        if (Error != null) Error(null, EventArgs.Empty);
                    }
                    catch (NullReferenceException ex3)
                    {
                        LastError = "NullReferenceException caught!!!";
                        LastError += "\nSource : " + ex3.Source;
                        LastError += "\nMessage : " + ex3.Message;
                        if (Error != null) Error(null, EventArgs.Empty);
                    }
                    catch (Exception ex4)
                    {
                        LastError = "Exception caught!!!";
                        LastError += "\nSource : " + ex4.Source;
                        LastError += "\nMessage : " + ex4.Message;
                        if (Error != null) Error(null, EventArgs.Empty);
                    }


                }).Start();


        }

        private static int seqNum = 0;
        public static int NextSeq
        {
            get
            {
                seqNum++;
                return seqNum;
            }
        }

        public static void SendCommand(string type, string command, string contextId = null, string[] arguments = null, string seqN = null)
        {
            if (seqN == null) seqN = NextSeq.ToString();
            string data;
            if (contextId == null) data = "{" + string.Format("\"type\":\"{0}\", \"command\":\"{1}\", \"seq\":{2}\n", type, command, seqN);
            else data = "{" + string.Format("\"type\":\"{0}\", \"command\":\"{1}\", \"contextId\":\"{2}\", \"seq\":{3}\n", type, command, contextId, seqN);
            if (arguments != null)
            {
                data += ", \"arguments\": {\r\n";
                bool firstArg = true;
                foreach (var arg in arguments)
                {
                    if (firstArg)
                    {
                        firstArg = false;
                    }
                    else
                    {
                        data += ",\r\n";
                    }
                    data += arg;
                }
                data += "\r\n}";
            }
            data += "}\r\n";
            SendData(data);
        }

        public static void GetListContexts()
        {
            SendData("{\n" +
                    "\"type\":\"request\",\n" +
                    "\"command\":\"listContexts\",\n" +
                    "\"seq\":" + NextSeq.ToString() + "\n" +
                    "}");
        }


        internal static void GetTools()
        {
            SendCommand("request", "getTools");
        }
    }
}
