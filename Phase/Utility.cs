using Newtonsoft.Json;

namespace Phase
{
    public static class Utility
    {
        public static string GetFloatArrayAsString(float[] input)
        {
            return '[' + String.Join(",", input.Select(p => p.ToString()).ToArray()) + ']';
        }

        public static bool TryParseJson<T>(this string @this, out T result)
        {
            bool success = true;
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
                MissingMemberHandling = MissingMemberHandling.Error
            };
            result = JsonConvert.DeserializeObject<T>(@this, settings);
            return success;
        }
    }
}
