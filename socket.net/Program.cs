using System.Net;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

var WsOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(120) };
app.UseWebSockets(WsOptions);
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/send")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
            {
                await Send(context, webSocket);
            }
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
    }
});
async Task Send(HttpContext context, WebSocket websocket)
{
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
    if (result != null)
    {
        while (!result.CloseStatus.HasValue)
        {
            string msg = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, result.Count));
            Console.Out.WriteLineAsync($"fala ai: {msg}");
            await websocket.SendAsync(
                new ArraySegment<byte>(
                    Encoding.UTF8.GetBytes(
                        $"admin corno:{DateTime.UtcNow:f}")),
                    result.MessageType,
                    result.EndOfMessage,
                    System.Threading.CancellationToken.None);
            result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
            //Console.Out.WriteLineAsync(result);
        }
    }
}

app.Run();
