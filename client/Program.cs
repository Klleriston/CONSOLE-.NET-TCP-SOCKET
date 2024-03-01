// See https://aka.ms/new-console-template for more information
using System.Net.WebSockets;
using System.Text;

Console.WriteLine("Inicia o chat apertando ENTER");
Console.ReadLine();
using (ClientWebSocket client = new ClientWebSocket())
{
    Uri serviceUri = new Uri("ws://localhost:5085/send");
    var cTs = new CancellationTokenSource();
    cTs.CancelAfter(TimeSpan.FromSeconds(120));
    try
    {
        await client.ConnectAsync(serviceUri, cTs.Token);
        var n = 0;
        while(client.State == WebSocketState.Open)
        {
            Console.Write("Você: ");
            string msg = Console.ReadLine();
            if (!string.IsNullOrEmpty(msg))
            {
                ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                await client.SendAsync(bytesToSend, WebSocketMessageType.Text, true, cTs.Token);
                var responseBuffer = new byte[1024];
                var offset = 0;
                var packet = 1024;
                while (true)
                {
                    ArraySegment<byte> byteRecieved = new ArraySegment<byte>(responseBuffer, offset, packet);
                    WebSocketReceiveResult response = await client.ReceiveAsync(byteRecieved, cTs.Token);
                    var responsemsg = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);
                    Console.WriteLine($"{responsemsg}  ({DateTime.UtcNow.ToLocalTime()})");
                    if (response.EndOfMessage)
                    {
                        break;
                    }
                }
            }
        }
    } 
    catch (WebSocketException ex) 
    {
        Console.WriteLine(ex.Message);
    }
}