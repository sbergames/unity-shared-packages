using System;

namespace SberGames.DataPlatform.Core
{
    public class GUIDGenerator
    {
        public static string Generate()
        {
            return Guid.NewGuid().ToString().Replace("-", "").ToUpper();
        }
    }
}
