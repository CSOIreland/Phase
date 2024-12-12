using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Phase.Database
{
    [JsonObject]
    public partial class Dataset
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public List<Dimension> Dimensions { get; set; }
        public string Copyright { get; set; }
        public string Note { get; set; }

      
    }

    [JsonObject]
    public partial class Dimension
    {
        public string Name { get; set; }
        public List<string> Variables { get; set; }
    }
}
