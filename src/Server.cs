using System.Net;
using System.Net.Sockets;

internal class Program
{
    private static void Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 6379);

        try
        {
            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;

            // Enter the listening loop.
            while (true)
            {
                Console.Write("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                server.Start();
                using TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("Received: {0}", data);

                    // Process the data sent by the client.
                    data = data.ToUpper();

                    // byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                    byte[] msg2 = System.Text.Encoding.ASCII.GetBytes("+PONG\r\n");


                    // Send back a response.
                    // stream.Write(msg, 0, msg.Length);
                    stream.Write(msg2, 0, msg2.Length);
                    Console.WriteLine("Sent: {0}", "+PONG\r\n");
                }
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            server.Stop();
        }

        Console.WriteLine("\nHit enter to continue...");
        Console.Read();
    }
}