using System.Net.Sockets;
using System.Text;

using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
await socket.ConnectAsync("127.0.0.1", 8888);

try
{
    Console.WriteLine("Enter one of this actions:\n1 - create file\n2 - get file\n3 - delete file\nexit - stop server");
    Console.Write("Enter action: > ");
    string command = Console.ReadLine();
    string secondCommand = "";
    while (true)
    {
        if (command != "1" && command != "2" && command != "3" && command != "exit")
        {
            Console.WriteLine("The command was entered incorrectly");
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

    if (command == "1")
    {
        Console.Write("Enter name of the file: > ");
        string clientFileName = Console.ReadLine();
        clientFileName = Path.Combine(@"C:\Users\SonRose352\SEprojects\SE_lab3_Task2_Type2\Client\data", clientFileName);
        while (true)
        {
            if (!File.Exists(clientFileName))
            {
                Console.WriteLine("There is no such file in the folder client/data!");
                Console.Write("Enter name of the file: > ");
                clientFileName = Console.ReadLine();
                clientFileName = Path.Combine(@"C:\Users\SonRose352\SEprojects\SE_lab3_Task2_Type2\Client\data", clientFileName);
            }
            else
            {
                break;
            }
        }

        Console.Write("Enter name of the file to be saved on server (If you want the file name to be generated automatically, press ENTER): > ");
        string serverFileName = "";
        serverFileName = Console.ReadLine();

        if (string.IsNullOrEmpty(serverFileName))
        {
            serverFileName = Path.GetExtension(clientFileName);
        }

        while (true)
        {
            if (serverFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                Console.WriteLine("The command was entered incorrectly");
                Console.Write("Enter name of the file to be saved on server (If you want the file name to be generated automatically, press ENTER): > ");
                serverFileName = Console.ReadLine();

                if (string.IsNullOrEmpty(serverFileName))
                {
                    serverFileName = Path.GetExtension(clientFileName);
                }
            }
            else
            {
                break;
            }
        }


        byte[] serverFileNameBytes = Encoding.UTF8.GetBytes(serverFileName);
        await socket.SendAsync(serverFileNameBytes);


        byte[] clientFileNameBytes = File.ReadAllBytes(clientFileName);
        byte[] fileSizeBytes = BitConverter.GetBytes(clientFileNameBytes.Length);

        await socket.SendAsync(fileSizeBytes);
        await socket.SendAsync(clientFileNameBytes);
    }

    string serverFileName2 = "";
    if (command == "2")
    {
        Console.WriteLine("Do you want to get the file by name or by ID\n1 - by name\n2 - by ID");
        Console.Write("Enter action: > ");
        secondCommand = Console.ReadLine();
        while (true)
        {
            if (secondCommand != "1" && secondCommand != "2")
            {
                Console.WriteLine("The command was entered incorrectly");
                Console.Write("Enter action: > ");
                secondCommand = Console.ReadLine();
            }
            else
            {
                break;
            }
        }

        byte[] secondCommandBytes = Encoding.UTF8.GetBytes(secondCommand);
        await socket.SendAsync(secondCommandBytes);

        if (secondCommand == "1")
        {
            Console.Write("Enter file name: > ");
            serverFileName2 = Console.ReadLine();
            while (true)
            {
                if (serverFileName2.IndexOf('.') == -1)
                {
                    Console.WriteLine("The file name was entered incorrectly");
                    Console.Write("Enter file name: > ");
                    serverFileName2 = Console.ReadLine();
                }
                else
                {
                    break;
                }
            }

            byte[] serverFileNameBytes = Encoding.UTF8.GetBytes(serverFileName2);
            await socket.SendAsync(serverFileNameBytes);
        }
        else
        {
            Console.Write("Enter ID: > ");
            string ID = Console.ReadLine();

            byte[] IDBytes = Encoding.UTF8.GetBytes(ID);
            await socket.SendAsync(IDBytes);
        }
    }


    if (command == "3")
    {
        Console.WriteLine("Do you want to delete the file by name or by ID\n1 - by name\n2 - by ID");
        Console.Write("Enter action: > ");
        secondCommand = Console.ReadLine();
        while (true)
        {
            if (secondCommand != "1" && secondCommand != "2")
            {
                Console.WriteLine("The command was entered incorrectly");
                Console.Write("Enter action: > ");
                secondCommand = Console.ReadLine();
            }
            else
            {
                break;
            }
        }

        byte[] secondCommandBytes = Encoding.UTF8.GetBytes(secondCommand);
        await socket.SendAsync(secondCommandBytes);

        if (secondCommand == "1")
        {
            Console.Write("Enter file name: > ");
            string serverFileName = Console.ReadLine();
            while (true)
            {
                if (serverFileName.IndexOf('.') == -1)
                {
                    Console.WriteLine("The file name was entered incorrectly");
                    Console.Write("Enter file name: > ");
                    serverFileName = Console.ReadLine();
                }
                else
                {
                    break;
                }
            }

            byte[] serverFileNameBytes = Encoding.UTF8.GetBytes(serverFileName);
            await socket.SendAsync(serverFileNameBytes);
        }
        else
        {
            Console.Write("Enter ID: > ");
            string ID = Console.ReadLine();

            byte[] IDBytes = Encoding.UTF8.GetBytes(ID);
            await socket.SendAsync(IDBytes);
        }
    }


    Console.WriteLine("The request was sent.");


    var codeResponseBytes = new byte[512];
    var codeBytes = await socket.ReceiveAsync(codeResponseBytes);
    string code = Encoding.UTF8.GetString(codeResponseBytes, 0, codeBytes);

    if (command == "1")
    {
        if (code == "403")
            Console.WriteLine("The response says that the file was not found!");
        else
            Console.WriteLine($"The response says that the file was saved! ID = {code.Substring(4)}!");
    }
    else if (command == "2")
    {
        if (secondCommand == "1")
        {
            if (code == "404")
                Console.WriteLine("The response says that the file was not found!");
            else
            {
                serverFileName2 = Path.Combine(@"C:\Users\SonRose352\SEprojects\SE_lab3_Task2_Type2\Client\data", serverFileName2);

                var clientFileSizeBytes = new byte[1024];

                await socket.ReceiveAsync(clientFileSizeBytes);
                int clientFileSize = BitConverter.ToInt32(clientFileSizeBytes);

                byte[] fileData = new byte[clientFileSize];
                await socket.ReceiveAsync(fileData);

                File.WriteAllBytes(serverFileName2, fileData);
            }
        }
        else
        {
            if (code == "404")
                Console.WriteLine("The response says that the file was not found!");
            else
            {
                // Получение длины имени файла
                var clientFileNameSizeBytes = new byte[4];
                await socket.ReceiveAsync(clientFileNameSizeBytes);
                int clientFileNameSize = BitConverter.ToInt32(clientFileNameSizeBytes);

                // Получение имени файла
                var clientFileNameBytes = new byte[clientFileNameSize];
                await socket.ReceiveAsync(clientFileNameBytes);
                string clientFileName = Encoding.UTF8.GetString(clientFileNameBytes);
                clientFileName = Path.Combine(@"C:\Users\SonRose352\SEprojects\SE_lab3_Task2_Type2\Client\data", clientFileName);

                // Получение длины файла
                var clientFileSizeBytes = new byte[4];
                await socket.ReceiveAsync(clientFileSizeBytes);
                int clientFileSize = BitConverter.ToInt32(clientFileSizeBytes);

                // Получение самого файла
                byte[] fileData = new byte[clientFileSize];
                await socket.ReceiveAsync(fileData);

                File.WriteAllBytes(clientFileName, fileData);
            }
        }
    }
    else if (command == "3")
    {
        if (code == "404")
            Console.WriteLine("The response says that the file was not found!");
        else
            Console.WriteLine("The response says that the file was successfully deleted!");
    }
}
catch
{

}
finally
{
    socket.Close();
}