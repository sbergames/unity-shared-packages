namespace SberGames.Utils.DataDownloader.Exceptions
{
    using System;

    public class ProgressConsumerException : Exception
    {
        public ProgressConsumerException(string message)
            : base(message)
        {
        }
    }
}