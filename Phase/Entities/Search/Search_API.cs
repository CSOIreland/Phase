using API;

namespace Phase
{
    [AllowAPICall]
    public class Search_API
    {
        public static dynamic Read(JSONRPC_API requestApi)
        {
            return new Search_BSO_Read(requestApi).Read().Response;
        }
    }
}
