namespace Utils.Tests.SampleData.DataDownloader
{
    public class MockClassForSerialization
    {
        public int A = 5;
        public int B = 6;
        public int C = 7;
        public int D = 8;

        public MockClassForSerialization(int a, int b, int c, int d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public MockClassForSerialization()
        {
        }
    }
}