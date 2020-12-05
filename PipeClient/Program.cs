using Newtonsoft.Json.Linq;
using PipeCommon;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ExampleClient
{
    class Program
    {
        static void Main(string[] _)
        {
            var cancellation = new CancellationTokenSource();

            var pipeClient = new PipeClient(PipeConstants.PIPE_NAME, cancellation.Token);

            try
            {
                pipeClient.Connect();

                Console.WriteLine("Client OK");

                var requests = new List<string>()
                {
                    PipeConstants.READ_DATE_TIME,
                    PipeConstants.READ_COMPUTER_NAME,
                    PipeConstants.END_COMMUNICATION
                };

                foreach (var request in requests)
                {
                    pipeClient.SendMessage(new { Type = request });

                    var message = pipeClient.ReadMessage();

                    ValidateMessage(message);

                    var data = message.Value<string>("Data");

                    Console.WriteLine($"Server Response: {data}");
                }

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                pipeClient.Close();
            }

            static void ValidateMessage(JObject message)
            {
                var errorMessage = message.Value<string>("ErrorMessage");

                if (errorMessage != null)
                {
                    throw new Exception(errorMessage);
                }
            }
        }
    }
}
