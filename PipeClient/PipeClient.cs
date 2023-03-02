using System.IO.Pipes;
using System.Text;
using System.Text.RegularExpressions;

namespace NamedPipes
{
    class PipeClient
    {
        public const string pipeName = "MyPipe";

        private static void SendMessage(string message, NamedPipeClientStream namedPipeClient)
        {
            if (!namedPipeClient.IsConnected)
            {
                namedPipeClient.Connect();
            }
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            namedPipeClient.Write(messageBytes, 0, messageBytes.Length);
        }

        private static string ReceiveMessage(NamedPipeClientStream namedPipeClient)
        {
            if (!namedPipeClient.IsConnected)
            {
                namedPipeClient.Connect();
            }
            StringBuilder messageBuilder = new StringBuilder();
            string messageChunk = string.Empty;
            byte[] messageBuffer = new byte[5];
            do
            {
                namedPipeClient.Read(messageBuffer, 0, messageBuffer.Length);
                messageChunk = Encoding.UTF8.GetString(messageBuffer);
                messageBuilder.Append(messageChunk);
                messageBuffer = new byte[messageBuffer.Length];
            }
            while (!namedPipeClient.IsMessageComplete);

            return Regex.Replace(messageBuilder.ToString(), @"\0+", string.Empty);
        }

        public static void Main()
        {
            Console.WriteLine("Welcome to NamedPipe");
            Console.WriteLine("Client commands are:");
            Console.WriteLine("do - do something");
            Console.WriteLine("cat - send me a picture of a cat");
            Console.WriteLine("quit - quit");

            NamedPipeClientStream namedPipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            string message = "";
            while (message != "quit")
            {
                Console.WriteLine("Enter command");
                message = Console.ReadLine();
                SendMessage(message, namedPipeClient);
                if (message == "quit")
                {
                    namedPipeClient.Close();
                    break;
                }
                namedPipeClient.ReadMode = PipeTransmissionMode.Message;
                Console.WriteLine($"Message from server:\n {ReceiveMessage(namedPipeClient)}");
            }
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }
    }
}