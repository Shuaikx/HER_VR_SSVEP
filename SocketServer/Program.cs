using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketServer
{
    class Program
    {
        private static TcpListener server;
        private static TcpClient client;
        private static NetworkStream stream;
        private static bool isRunning = true;
        private static bool isAuthenticated = false;

        static void Main(string[] args)
        {
            Console.WriteLine("=== VR_SSVEP Socket Server ===");
            Console.WriteLine("Starting server on 127.0.0.1:4003...");

            try
            {
                // Start TCP Server
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, 4003);
                server.Start();

                Console.WriteLine("Server started. Waiting for Unity client connection...");
                Console.WriteLine("Press 'Q' to quit server\n");

                // Accept client connection in a separate thread
                Thread acceptThread = new Thread(AcceptClient);
                acceptThread.Start();

                // Handle user input
                HandleUserInput();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            finally
            {
                Cleanup();
            }
        }

        static void AcceptClient()
        {
            try
            {
                // Wait for client connection
                client = server.AcceptTcpClient();
                stream = client.GetStream();

                Console.WriteLine("\n[SERVER] Client connected from: " + client.Client.RemoteEndPoint);
                Console.WriteLine("[SERVER] Waiting for authentication...\n");

                // Start receiving data
                Thread receiveThread = new Thread(ReceiveData);
                receiveThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Accept client failed: " + e.Message);
            }
        }

        static void ReceiveData()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (isRunning && client != null && client.Connected)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);

                        if (bytesRead > 0)
                        {
                            // Check for authentication message (0x11)
                            if (buffer[0] == 0x11)
                            {
                                Console.WriteLine("[RECEIVED] Authentication request (0x11)");
                                SendAuthentication();
                            }
                            // Check for quit message (0xFF)
                            else if (buffer[0] == 0xFF)
                            {
                                Console.WriteLine("[RECEIVED] Quit message (0xFF)");
                                Console.WriteLine("[SERVER] Client requested shutdown");
                            }
                            else
                            {
                                // Try to parse as string
                                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                Console.WriteLine($"[RECEIVED] Data: 0x{buffer[0]:X2} | String: {message}");
                            }
                        }
                    }

                    Thread.Sleep(10); // Prevent CPU overuse
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Receive data failed: " + e.Message);
            }
        }

        static void SendAuthentication()
        {
            try
            {
                byte[] authResponse = new byte[1] { 0x11 };
                stream.Write(authResponse, 0, authResponse.Length);
                stream.Flush();
                isAuthenticated = true;
                Console.WriteLine("[SENT] Authentication response (0x11)");
                Console.WriteLine("[SERVER] Client authenticated successfully!\n");
                Console.WriteLine("You can now send messages:");
                Console.WriteLine("  - Type byte value (0-255) to send single byte");
                Console.WriteLine("  - Type text message to send as string");
                Console.WriteLine("  - Type 'Q' to quit\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Send authentication failed: " + e.Message);
            }
        }

        static void HandleUserInput()
        {
            while (isRunning)
            {
                string input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                    continue;

                // Quit command
                if (input.ToUpper() == "Q")
                {
                    isRunning = false;
                    Console.WriteLine("[SERVER] Shutting down...");
                    break;
                }

                // Wait for authentication before allowing message sending
                if (!isAuthenticated)
                {
                    Console.WriteLine("[WARNING] Client not authenticated yet. Wait for Unity client to connect.");
                    continue;
                }

                if (client == null || !client.Connected)
                {
                    Console.WriteLine("[WARNING] No client connected. Cannot send message.");
                    continue;
                }

                // Try to parse as byte value (0-255)
                if (byte.TryParse(input, out byte byteValue))
                {
                    SendByteData(byteValue);
                }
                else
                {
                    // Send as string
                    SendStringData(input);
                }
            }
        }

        static void SendByteData(byte value)
        {
            try
            {
                byte[] data = new byte[1] { value };
                stream.Write(data, 0, data.Length);
                stream.Flush();
                Console.WriteLine($"[SENT] Byte: 0x{value:X2} ({value})");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Send byte failed: " + e.Message);
            }
        }

        static void SendStringData(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                stream.Flush();
                Console.WriteLine($"[SENT] String: {message} ({data.Length} bytes)");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Send string failed: " + e.Message);
            }
        }

        static void Cleanup()
        {
            try
            {
                if (stream != null)
                    stream.Close();

                if (client != null)
                    client.Close();

                if (server != null)
                    server.Stop();

                Console.WriteLine("[SERVER] Server stopped.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Cleanup failed: " + e.Message);
            }
        }
    }
}
