using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SberGames.DataPlatform.Core
{
    public class ParallelFileWriter : IFileWriter
    {
        private ConcurrentQueue<string> dataQueue = new ConcurrentQueue<string>();
        
        private CancellationTokenSource cancellationTokenSource;
        private TextWriter file = null;
        private bool isNeedRemove = false;
        private string path;

        public bool IsClosed { get; private set; } = true;
        
        public void Open(string _path)
        {
            path = _path;
            file = new StreamWriter(path,true);
            
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => Loop());

            IsClosed = false;
        }

        public void WriteLine(string data)
        {
            dataQueue.Enqueue(data
                .Replace("\n", "")
                .Replace("\t", "")
                .Replace("\r", "")
                .Replace(" ", ""));
        }

        public void Close()
        {
            cancellationTokenSource.Cancel();
        }

        public void CloseAndRemove()
        {
            isNeedRemove = true;
            cancellationTokenSource.Cancel();
        }

        private void Loop()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                WriteData();
                Thread.Sleep(500);
            }

            WriteData();
            file.Close();
            
            if (isNeedRemove)
            {
                File.Delete(path);
            }
            IsClosed = true;
        }

        private void WriteData()
        {
            string data;
            while (dataQueue.TryDequeue(out data))
            {
                file.WriteLine(data);
            }
            file.Flush();
        }
    }
}
