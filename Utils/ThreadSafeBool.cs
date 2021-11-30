namespace SberGames.Utils
{
    using System.Threading;

    public class ThreadSafeBool
    {
        private int threadSafeBoolBackValue = 0;

        public ThreadSafeBool(bool initialValue)
        {
            SetValue(initialValue);
        }
        
        private bool Value => (Interlocked.CompareExchange(ref threadSafeBoolBackValue, 1, 1) == 1);

        public bool TrySetValue(bool newValue)
        {
            var oldIntValue = newValue ? 0 : 1;
            var newIntValue = newValue ? 1 : 0;

            return Interlocked.CompareExchange(ref threadSafeBoolBackValue, newIntValue, oldIntValue) == oldIntValue;
        }

        public void SetValue(bool newValue)
        {
            threadSafeBoolBackValue = newValue ? 1 : 0;
        }
        
        public static implicit operator bool(ThreadSafeBool input)
        {
            return input.Value;
        }
    }
}