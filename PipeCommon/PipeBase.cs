using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;

namespace PipeCommon
{
    public abstract class PipeBase
    {
        protected CancellationToken CancellationToken { get; set; }
        protected PipeStream PipeStream { get; set; }
        protected bool CloseConnection { get; set; }

        public int ConnectTimeout { get; set; } = 10000; // 10s
        public int ReadTimeout { get; set; } = 15000;    // 15s 
        public int WriteTimeout { get; set; } = 10000;   // 10s
        public int PackageSize { get; set; } = 2048 * 2048;

        public void SendMessage(object data)
        {
            var json = JsonConvert.SerializeObject(data);

            var bytes = Encoding.UTF8.GetBytes(json);

            using (var writeEvent = new ManualResetEventSlim())
            {
                BeginWrite(bytes, writeEvent);

                if (!writeEvent.Wait(WriteTimeout, CancellationToken))
                {
                    throw new TimeoutException();
                }
            }
        }

        public JObject ReadMessage()
        {
            byte[] package = null;

            using (var readEvent = new ManualResetEventSlim())
            {
                BeginRead(readEvent, (bytes) => package = bytes);

                if (!readEvent.Wait(ReadTimeout, CancellationToken))
                {
                    throw new TimeoutException();
                }
            }

            if (package == null)
            {
                throw new Exception("Failed to receive data");
            }

            var data = Encoding.UTF8.GetString(package, 0, package.Length);

            return JsonConvert.DeserializeObject<JObject>(data);
        }

        private void BeginWrite(byte[] data, ManualResetEventSlim manualEvent)
        {
            PipeStream.BeginWrite(data, 0, data.Length, (result) =>
            {
                PipeStream.EndWrite(result);
                PipeStream.Flush();

                manualEvent.Set();
            }, null);
        }

        private void BeginRead(ManualResetEventSlim manualEvent, Action<byte[]> callback)
        {
            var buffer = new byte[PackageSize];

            PipeStream.BeginRead(buffer, 0, buffer.Length, (result) =>
            {
                var bytesRead = PipeStream.EndRead(result);

                if (bytesRead > 0)
                {
                    callback(buffer.Take(bytesRead).ToArray());
                }
                else
                {
                    Close();
                }

                manualEvent.Set();
            }, null);
        }

        public void Close()
        {
            CloseConnection = true;

            PipeStream.Close();
            PipeStream.Dispose();
        }
    }
}
