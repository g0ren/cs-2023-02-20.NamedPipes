using System.IO.Pipes;
using System.Text;
using System.Text.RegularExpressions;

namespace NamedPipes
{
    class PipeServer
    {
        public const string pipeName = "MyPipe";

        private static void SendMessage(string message, NamedPipeServerStream namedPipeServer)
        {
            if (!namedPipeServer.IsConnected)
            {
                namedPipeServer.WaitForConnection();
            }
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            namedPipeServer.Write(messageBytes, 0, messageBytes.Length);
        }

        private static string ReceiveMessage(NamedPipeServerStream namedPipeServer)
        {
            if (!namedPipeServer.IsConnected)
            {
                namedPipeServer.WaitForConnection();
            }
            StringBuilder messageBuilder = new StringBuilder();
            string messageChunk = string.Empty;
            byte[] messageBuffer = new byte[5];
            do
            {
                namedPipeServer.Read(messageBuffer, 0, messageBuffer.Length);
                messageChunk = Encoding.UTF8.GetString(messageBuffer);
                messageBuilder.Append(messageChunk);
                messageBuffer = new byte[messageBuffer.Length];
            }
            while (!namedPipeServer.IsMessageComplete);

            return  Regex.Replace(messageBuilder.ToString(), @"\0+", string.Empty);
        }

        public static void Main()
        {
            NamedPipeServerStream namedPipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut,
                1, PipeTransmissionMode.Message);
            string message = "";
            while (message != "quit")
            {
                if (!namedPipeServer.IsConnected)
                {
                    try
                    {
                        namedPipeServer.WaitForConnection();
                    }catch(System.IO.IOException e)
                    {
                        break;
                    }
                }
                message = ReceiveMessage(namedPipeServer);

                Console.WriteLine($"Received command: {message}");

                switch (message)
                { 
                    case "do":
                        SendMessage("Doing something...", namedPipeServer);
                        break;
                    case "cat":
                        SendMessage($"   ____\r\n  (.   \\\r\n    \\  |  \r\n     \\ |___(\\--/)\r\n   __/    (  . . )\r\n  \"'._.    '-.O.'\r\n       '-.  \\ \"|\\\r\n          '.,,/'.,,mrf", namedPipeServer);
                        break;
                    case "quit": 
                        namedPipeServer.Close(); 
                        Console.WriteLine("Quit command received, quitting");
                        break;
                    default:
                        SendMessage($"I've received command \'{message}\'. Not sure what to do with it...", namedPipeServer);
                        break;
                }
            }

            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }
    }
}