using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

using TcpListener server = new(IPAddress.Any, 6379);

Dictionary<string, string> store = [];

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
        byte[] buffer = new byte[client.ReceiveBufferSize];
        stream.Read(buffer);

        var (command, arguments) = ParseCommand(System.Text.Encoding.ASCII.GetString(buffer));
        string result = HandleCommand(command, arguments);

        stream.Write(System.Text.Encoding.ASCII.GetBytes(result));
    }
}

(string, List<string>) ParseCommand(string input)
{
    MatchCollection matches = Regex.Matches(input, @"\$\d+\r\n(\w+)\r\n", RegexOptions.Multiline);

    string command = "";
    List<string> arguments = [];

    foreach (Match match in matches)
    {
        if (command == string.Empty)
        {
            command = match.Groups[1].Value.ToUpper();
            continue;
        }

        arguments.Add(match.Groups[1].Value);
    }

    return (command, arguments);
}

string HandleCommand(string command, List<string> arguments)
{
    switch(command)
    {
        case "PING":
            return "+PONG\r\n";
        case "ECHO":
            return BulkString(arguments[0]);
        case "GET":
            return BulkString(store[arguments[0]]);
        case "SET":
            store.Add(arguments[0], arguments[1]);
            return SimpleString("OK");
        default:
            return "";
    }
}

string BulkString(string input)
{
    return $"${input.Length}\r\n{input}\r\n";
}

string SimpleString(string input)
{
    return $"+{input}\r\n";
}

// TODO: take a look at other implementations