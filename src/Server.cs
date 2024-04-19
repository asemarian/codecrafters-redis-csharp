using System.Net;
using System.Net.Sockets;


using TcpListener server = new(IPAddress.Any, 6379);

try
{
    server.Start();

    while (true)
    {
        var client = await server.AcceptTcpClientAsync();
        _ = Task.Run(() => HandleClient(client));
    }
}
catch (SocketException e)
{
    Console.WriteLine("SocketException: {0}", e);
}

Console.WriteLine("\nHit enter to continue...");
Console.Read();

void HandleClient(TcpClient client)
{
    Console.WriteLine("Connected!");
    NetworkStream stream = client.GetStream();

    while (client.Connected)
    {
        byte[] buffer = new byte[256];
        stream.Read(buffer);

        var command = System.Text.Encoding.ASCII.GetString(buffer);

        stream.Write(System.Text.Encoding.ASCII.GetBytes("+PONG\r\n"));
    }
}