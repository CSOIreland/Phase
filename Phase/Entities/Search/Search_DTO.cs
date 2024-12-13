using Newtonsoft.Json;
using PxStat.Resources;
using System.Web;

namespace Phase
{
    public class Search_DTO
    {
        public string LngIsoCode {  get; set; } 
        public string Search { get; set; }
        public Search_DTO(dynamic parameters)
        {
            if (parameters.Count == null)
            {
                
                if (parameters.LngIsoCode != null)
                {
                    LngIsoCode = parameters.LngIsoCode;
                }
                else
                    LngIsoCode = "en";

                if (parameters.Search != null)
                {
                    Search = parameters.Search;
                }
            }
            else
            {
                if(parameters.Count>1)
                    LngIsoCode = parameters[1];
                if (parameters.Count > 2) 
                    Search= HttpUtility.UrlDecode(parameters[2]);
            }
            
        }


    }
}
