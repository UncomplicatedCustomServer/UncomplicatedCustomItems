namespace UncomplicatedCustomItems.Extensions
{
    public static class StringExtensions
    {
        public static string GenerateWithBuffer(this string str, int bufferSize)
        {
            for (int a = str.Length; a < bufferSize; a++)
                str += " ";

            return str;
        }
    }
}
