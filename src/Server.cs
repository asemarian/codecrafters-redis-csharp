using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

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

        var input = System.Text.Encoding.ASCII.GetString(buffer);
        var (command, argument) = ParseCommand(input);
        string result = HandleCommand(command, argument);
        stream.Write(System.Text.Encoding.ASCII.GetBytes(result));
    }
}

(string, string) ParseCommand(string input)
{
    Match match = Regex.Match(input, @"\*\d+\r\n\$\d+\r\n(\w+)\r\n(?:\$\d+\r\n(\w+)\r\n)?");
    string command, argument;

    if (match.Success)
    {
        command = match.Groups[1].Value.ToUpper();
        argument = match.Groups[2].Value;
    }
    else
    {
        throw new Exception("Nope");
    }

    return (command, argument);
}

string HandleCommand(string command, string argument)
{
    switch(command)
    {
        case "PING":
            return "+PONG\r\n";
        case "ECHO":
            return $"${argument.Length}\r\n{argument}\r\n";
        default:
            return "";
    }
}