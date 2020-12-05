using System;
using System.IO.Pipes;
using System.Threading;

namespace PipeCommon
{
    public class PipeClient : PipeBase
    {
        private NamedPipeClientStream ClientStream { get; set; }

        public PipeClient(string pipeName, CancellationToken cancellationToken)
        {
            ClientStream = new NamedPipeClientStream(pipeName);

            PipeStream = ClientStream;
            CancellationToken = cancellationToken;
        }

        public void Connect()
        {
            ClientStream.Connect(ConnectTimeout);

            if (!ClientStream.IsConnected)
            {
                throw new TimeoutException("Cannot connect to server");
            }
        }
    }
}
