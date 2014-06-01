namespace Barista.Extensions
{
    public static class ArrayExtensions
    {
        public static T GetValue<T>(this object[] arr, int index)
            where T : class
        {
            if (index >= 0 && index < arr.Length)
            {
                return arr.GetValue(index) as T;
            }

            return default(T);
        }
    }
}
