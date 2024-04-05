using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static Hashtable fileTable = new Hashtable();
    static string IDsFilePath = @"C:\Users\SonRose352\SEprojects\SE_lab3_Task2_Type2\Server\IDs.txt";
    static bool isRunning = true;

    static async Task ProcessClientAsync(Socket socket)
    {
        try
        {
            using Socket client = await socket.AcceptAsync();
            string code = "";
            string serverFolderPath = @"C:\Users\SonRose352\SEprojects\SE_lab3_Task2_Type2\Server\data";


            var commandResponseBytes = new byte[1024];
            var commandBytes = await client.ReceiveAsync(commandResponseBytes);
            string command = Encoding.UTF8.GetString(commandResponseBytes, 0, commandBytes);


            if (command == "exit")
            {
                Console.WriteLine("Server is shutting down...");
                isRunning = false;
            }


            if (command == "1")
            {
                var responseBytes = new byte[512];

                var serverFileBytes = await client.ReceiveAsync(responseBytes);
                string serverFileName = Encoding.UTF8.GetString(responseBytes, 0, serverFileBytes);

                Array.Clear(responseBytes);
                await client.ReceiveAsync(responseBytes);
                int clientFileSize = BitConverter.ToInt32(responseBytes);

                byte[] fileData = new byte[clientFileSize];
                await client.ReceiveAsync(fileData);

                string fullServerPath = "";
                if (serverFileName.Length == 4 && serverFileName.StartsWith('.'))
                {
                    serverFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + serverFileName;
                }
                fullServerPath = Path.Combine(serverFolderPath, serverFileName);


                if (!File.Exists(fullServerPath))
                {
                    File.WriteAllBytes(fullServerPath, fileData);

                    object newID = 1;
                    foreach (DictionaryEntry entry in fileTable)
                    {
                        int key = int.Parse(entry.Key.ToString());
                        if (key >= (int)newID)
                        {
                            newID = key + 1;
                        }
                    }

                    fileTable.Add(newID.ToString(), serverFileName);


                    code = $"200,{newID}";
                }
                else
                {
                    code = "403";
                }

                byte[] codeBytes = Encoding.UTF8.GetBytes(code);
                await client.SendAsync(codeBytes);
            }



            if (command == "2")
            {
                var responseBytes = new byte[1024];

                var secondCommandBytes = await client.ReceiveAsync(responseBytes);
                string secondCommand = Encoding.UTF8.GetString(responseBytes, 0, secondCommandBytes);

                if (secondCommand == "1")
                {
                    Array.Clear(responseBytes);
                    var serverFileNameBytes = await client.ReceiveAsync(responseBytes);
                    string serverFileName = Encoding.UTF8.GetString(responseBytes, 0, serverFileNameBytes);
                    string serverFullName = Path.Combine(serverFolderPath, serverFileName);
                    if (File.Exists(serverFullName))
                    {
                        code = "200";

                        byte[] codeBytes = Encoding.UTF8.GetBytes(code);
                        await client.SendAsync(codeBytes);

                        byte[] clientFileBytes = File.ReadAllBytes(serverFullName);
                        byte[] fileSizeBytes = BitConverter.GetBytes(clientFileBytes.Length);
                        await client.SendAsync(fileSizeBytes);
                        await client.SendAsync(clientFileBytes);
                    }
                    else
                    {
                        code = "404";

                        byte[] codeBytes = Encoding.UTF8.GetBytes(code);
                        await client.SendAsync(codeBytes);
                    }

                }
                else
                {
                    string serverFileName = "";
                    string serverFullName = "";

                    var IDBytes = await client.ReceiveAsync(responseBytes);
                    string ID = Encoding.UTF8.GetString(responseBytes, 0, IDBytes);

                    bool fileExists = false;
                    foreach (DictionaryEntry entry in fileTable)
                    {
                        if (entry.Key.ToString() == ID)
                        {
                            fileExists = true;
                            serverFileName = entry.Value.ToString();
                            serverFullName = Path.Combine(serverFolderPath, serverFileName);
                            break;
                        }
                    }

                    if (fileExists && File.Exists(serverFullName))
                    {
                        code = "200";
                        byte[] codeBytes = Encoding.UTF8.GetBytes(code);
                        await client.SendAsync(codeBytes);

                        byte[] clientFileNameBytes = Encoding.UTF8.GetBytes(serverFileName);
                        byte[] clientFileNameBytesLength = BitConverter.GetBytes(clientFileNameBytes.Length);
                        await client.SendAsync(clientFileNameBytesLength);
                        await client.SendAsync(clientFileNameBytes);

                        byte[] clientFileBytes = File.ReadAllBytes(serverFullName);
                        byte[] fileSizeBytes = BitConverter.GetBytes(clientFileBytes.Length);
                        await client.SendAsync(fileSizeBytes);
                        await client.SendAsync(clientFileBytes);
                    }
                    else
                    {
                        code = "404";

                        byte[] codeBytes = Encoding.UTF8.GetBytes(code);
                        await client.SendAsync(codeBytes);
                    }
                }
            }


            if (command == "3")
            {
                var responseBytes = new byte[1024];
                var secondCommandBytes = await client.ReceiveAsync(responseBytes);
                string secondCommand = Encoding.UTF8.GetString(responseBytes, 0, secondCommandBytes);

                string serverFileName = "";

                if (secondCommand == "1")
                {
                    var serverFileNameBytes = new byte[1024];
                    var serverFileNameLength = await client.ReceiveAsync(serverFileNameBytes);
                    serverFileName = Encoding.UTF8.GetString(serverFileNameBytes, 0, serverFileNameLength);
                }
                else
                {
                    var IDBytes = await client.ReceiveAsync(responseBytes);
                    string ID = Encoding.UTF8.GetString(responseBytes, 0, IDBytes);

                    foreach (DictionaryEntry entry in fileTable)
                    {
                        if (entry.Key.ToString() == ID)
                        {
                            serverFileName = entry.Value.ToString();
                            break;
                        }
                    }
                }

                string serverFullName = Path.Combine(serverFolderPath, serverFileName);

                if (File.Exists(serverFullName))
                {
                    code = "200";

                    foreach (DictionaryEntry entry in fileTable)
                    {
                        if (entry.Value.ToString() == serverFileName)
                        {
                            Console.WriteLine(entry.Value);
                            fileTable.Remove(entry.Key);
                            break;
                        }
                    }
                    File.Delete(serverFullName);
                }
                else
                {
                    code = "404";
                }
            }
        }
        catch
        {

        }
    }


    static async Task Main()
    {
        if (File.Exists(IDsFilePath))
        {
            string[] fileLines = File.ReadAllLines(IDsFilePath);
            foreach (string line in fileLines)
            {
                string[] parts = line.Split(' ');
                fileTable[parts[0]] = parts[1];
            }
        }

        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(ipPoint);
        socket.Listen();
        Console.WriteLine("Server started!");


        while (isRunning)
        {
            Thread t = new Thread(async () => await ProcessClientAsync(socket));
            t.Start();
        }


        File.WriteAllText(IDsFilePath, string.Empty);

        List<string> lines = new List<string>();
        foreach (DictionaryEntry entry in fileTable)
        {
            lines.Add($"{entry.Key} {entry.Value}");
        }
        lines.Sort((x, y) => int.Parse(x.Split(' ')[0]).CompareTo(int.Parse(y.Split(' ')[0])));
        File.WriteAllLines(IDsFilePath, lines);
    }
}