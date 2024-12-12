namespace Phase
{
    public class Search_DTO
    {
        public string LngIsoCode {  get; set; } 
        public string Search { get; set; }
        public Search_DTO(dynamic parameters)
        {
            if (parameters.LngIsoCode != null)
            {
                LngIsoCode = parameters.LngIsoCode;
            }
            else
                LngIsoCode = "en";

            if(parameters.Search!=null)
            {
                Search = parameters.Search; 
            }
        }
    }
}
