using System;
using System.IO.Pipes;
using System.Threading;

namespace PipeCommon
{
    public class PipeServer : PipeBase
    {   
        protected NamedPipeServerStream ServerStream;

        public PipeServer(string pipeName, CancellationToken cancellationToken)
        {
            ServerStream = new NamedPipeServerStream(
                pipeName,
                direction: PipeDirection.InOut,
                maxNumberOfServerInstances: 1,
                transmissionMode: PipeTransmissionMode.Message,
                options: PipeOptions.Asynchronous);

            CloseConnection = false;
            PipeStream = ServerStream;
            CancellationToken = cancellationToken;
        }

        public void Start()
        {
            using (var connectionEvent = new ManualResetEventSlim())
            {
                ServerStream.BeginWaitForConnection(
                    (result) => WaitForConnectionCallback(result, connectionEvent), null);

                if (!connectionEvent.Wait(ConnectTimeout, CancellationToken))
                {
                    throw new TimeoutException("Client not connected");
                }
            }
        }

        private void WaitForConnectionCallback(IAsyncResult result, ManualResetEventSlim manualEvent)
        {
            if (!CloseConnection)
            {
                ServerStream.EndWaitForConnection(result);

                manualEvent.Set();
            }
        }
    }
}
