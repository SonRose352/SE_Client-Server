using System.Net.Sockets;
using System.Text;

using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
await socket.ConnectAsync("127.0.0.1", 8888);

try
{
    Console.WriteLine("Enter one of this actions:\nPUT - create file\nGET - get file\nDELETE - delete file");
    Console.Write("Enter action: > ");
    string command = Console.ReadLine();
    while (true)
    {
        if (command != "PUT" && command != "GET" && command != "DELETE" && command != "exit")
        {
            Console.WriteLine("Команда введена неверно");
            Console.Write("Enter action: > ");
            command = Console.ReadLine();
        }
        else
        {
            break;
        }
    }

    byte[] commandBytes = Encoding.UTF8.GetBytes(command);
    await socket.SendAsync(commandBytes);


    if (command != "exit")
    {
        Console.Write("Enter filename: > ");
        string fileName = Console.ReadLine();

        byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
        await socket.SendAsync(fileNameBytes);
    }


    if (command == "PUT")
    {
        Console.Write("Enter file content: > ");
        string text = Console.ReadLine();

        byte[] textBytes = Encoding.UTF8.GetBytes(text);
        await socket.SendAsync(textBytes);
    }


    Console.WriteLine("The request was sent.");


    var responseBytes = new byte[512];
    var codeBytes = await socket.ReceiveAsync(responseBytes);
    string code = Encoding.UTF8.GetString(responseBytes, 0, codeBytes);
    if (command == "PUT")
    {
        if (code == "200")
            Console.WriteLine("The response says that the file was created!");
        else
            Console.WriteLine("The response says that creating the file was forbidden!");
    }
    else if (command == "GET")
    {
        if (code == "404")
            Console.WriteLine("The response says that the file was not found!");
        else
        {
            code = code.Substring(4);
            Console.WriteLine($"The content of the file is: {code}");
        }
    }
    else if (command == "DELETE")
    {
        if (code == "200")
        {
            Console.WriteLine("The response says that the file was successfully deleted!");
        }
        else
        {
            Console.WriteLine("The response says that the file was not found!");
        }
    }
}
catch
{
    Console.WriteLine("Ошибка подключения к серверу");
}