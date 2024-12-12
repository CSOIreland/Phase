namespace Phase
{
    public class Utility
    {
        public static string GetFloatArrayAsString(float[] input)
        {
            return '[' + String.Join(",", input.Select(p => p.ToString()).ToArray()) + ']';
        }
    }
}
