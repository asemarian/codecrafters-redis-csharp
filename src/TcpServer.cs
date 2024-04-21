using System.Net;

public class TcpServer
{
    private IPAddress IPAddress {get; set;}
    private int Port {get;set;}

    public TcpServer(IPAddress? ipAddress = null, int port = 6379)
    {
        IPAddress = ipAddress ?? IPAddress.Any;
        Port = port;
    }
}