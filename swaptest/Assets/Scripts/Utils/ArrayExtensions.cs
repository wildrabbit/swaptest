namespace Utils
{
    public static class ArrayExtensions
    {
        public static void Fill<T>(this T[] array, T value)
        {
            for(int i = 0; i < array.Length; ++i)
            {
                array[i] = value;
            }
        }

        public static void Fill<T>(this T[,] array, T value)
        {
            int iLength = array.GetLength(0);
            int jLength = array.GetLength(1);
            for(int i = 0; i < iLength; ++i)
            {
                for(int j = 0; j < jLength; ++j)
                {
                    array[i, j] = value;
                }
            }
        }
    }
}
