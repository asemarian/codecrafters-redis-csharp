using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

using TcpListener server = new(IPAddress.Any, 6379);

Dictionary<string, StoredValue> store = [];

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
    switch (command)
    {
        case "PING":
            return SimpleString("PONG");
        case "ECHO":
            return BulkString(arguments[0]);
        case "GET":
            return Get(arguments);
        case "SET":
            Set(arguments);
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

string NullBulkString()
{
    return "$-1\r\n";
}

void Set(List<string> args)
{
    if (args.Count > 2)
    {
        store.Add(args[0], new StoredValue(args[1], ToTimestamp(int.Parse(args[3]))));
    }
    else
    {
        store.Add(args[0], new StoredValue(args[1]));
    }
}

long ToTimestamp(int interval)
{
    return DateTimeOffset.Now.ToUnixTimeMilliseconds() + interval;
}

string Get(List<string> arguments)
{
    var expirationTime = store[arguments[0]].Expiry;
    long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

    if ((expirationTime != null) && (expirationTime - now <= 0))
    {
        return NullBulkString();
    }

    return BulkString(store[arguments[0]].Value);
}

// TODO: add background job to delete expired keys
// TODO: refactor to classes and remove hardcoded assumptions