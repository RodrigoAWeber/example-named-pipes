using System;
using System.Threading;
using PipeCommon;

namespace ExampleServer
{
    class Program
    {
        static void Main(string[] _)
        {
            var cancellation = new CancellationTokenSource();

            var pipeServer = new PipeServer(PipeConstants.PIPE_NAME, cancellation.Token);

            try
            {
                pipeServer.Start();

                Console.WriteLine("Server OK");

                string type;

                do
                {
                    var message = pipeServer.ReadMessage();

                    type = message.Value<string>("Type");

                    Console.WriteLine($"Client Request: {type}");

                    switch (type)
                    {
                        case PipeConstants.READ_DATE_TIME:
                            pipeServer.SendMessage(new { Data = DateTime.Now });
                            break;

                        case PipeConstants.READ_COMPUTER_NAME:
                            pipeServer.SendMessage(new { Data = Environment.MachineName });
                            break;

                        case PipeConstants.END_COMMUNICATION:
                            pipeServer.SendMessage(new { Data = "OK" });
                            break;

                        default:
                            pipeServer.SendMessage(new { ErrorMessage = "Type not implemented" });
                            break;
                    }
                } while (type != PipeConstants.END_COMMUNICATION);

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                pipeServer.Close();
            }
        }
    }
}
