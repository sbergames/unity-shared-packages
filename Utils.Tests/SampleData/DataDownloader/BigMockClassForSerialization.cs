namespace Utils.Tests.SampleData.DataDownloader
{
    public class BigMockClassForSerialization
    {
        public int A = 5;
        public int B = 6;
        public int C = 7;
        public int D = 8;
        public int E = 5;
        public int F = 6;
        public int G = 7;
        public int H = 8;
        public int I = 5;
        public int J = 8;
        public int K = 6;
        public int L = 7;
        
        public int[] SomeArr = new int[100];

        public BigMockClassForSerialization()
        {
            for (int i = 0; i < SomeArr.Length; i++)
            {
                SomeArr[i] = i;
            }
        }
    }
}