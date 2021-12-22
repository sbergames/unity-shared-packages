
namespace SberGames.DataPlatform.Core
{
    public interface IFileWriter
    {
        bool IsClosed { get; }
        void Open(string path);
        void WriteLine(string data);
        void Close();
        void CloseAndRemove();
    }
}