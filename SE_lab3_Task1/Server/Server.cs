using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;

IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
string code = "";
socket.Bind(ipPoint);
socket.Listen();
Console.WriteLine("Server started!");

string folderPath = @"C:\Users\SonRose352\SEprojects\SE_lab3_Task1\Server\data";

try
{
    while (true)
    {
        using Socket client = await socket.AcceptAsync();

        var responseBytes = new byte[512];

        var commandBytes = await client.ReceiveAsync(responseBytes);
        string command = Encoding.UTF8.GetString(responseBytes, 0, commandBytes);


        if (command == "exit")
        {
            Console.WriteLine("Server is shutting down...");
            break;
        }


        var fileNameBytes = await client.ReceiveAsync(responseBytes);
        string fileName = Encoding.UTF8.GetString(responseBytes, 0, fileNameBytes);

        string text = "";
        if (command == "PUT")
        {
            var textBytes = await client.ReceiveAsync(responseBytes);
            text = Encoding.UTF8.GetString(responseBytes, 0, textBytes);
        }

        string fullPath = Path.Combine(folderPath, fileName);


        if (command == "PUT")
        {
            if (!File.Exists(fullPath) && fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                using (StreamWriter sw = File.CreateText(fullPath))
                {
                    sw.Write(text);
                }
                code = "200";
            }
            else
            {
                code = "403";
            }
        }

        string fileContent = "";
        if (command == "GET")
        {
            if (File.Exists(fullPath))
            {
                fileContent = File.ReadAllText(fullPath);
                code = "200 " + fileContent;
            }
            else
            {
                code = "404";
            }
        }

        if (command == "DELETE")
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                code = "200";
            }
            else
            {
                code = "404";
            }
        }


        var codeBytes = Encoding.UTF8.GetBytes(code);
        await client.SendAsync(codeBytes);
    }
}
catch
{

}
