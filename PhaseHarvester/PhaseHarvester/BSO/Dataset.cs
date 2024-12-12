using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PxStat.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace PhaseHarvester
{
    [JsonObject]
    public class Dataset
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public List<Dimension> Dimensions { get; set; }
        public string Copyright { get; set; }
        public string Note { get; set; }

        [JsonConstructor]
        public  Dataset(JsonStat dataSet)
        {
            Dimensions = new List<Dimension>(); 
            if (dataSet == null) throw new ArgumentNullException("Invalid dataset");
            this.Code = dataSet.Extension["matrix"].ToString();
            this.Title = dataSet.Label;
            this.Note = string.Empty;
            foreach(var item in dataSet.Note)
            {
                Note = " " + item;
            }
            JObject cp = (JObject)dataSet.Extension["copyright"];
            this.Copyright = cp["name"].ToString();
            foreach (var dim in dataSet.Dimension)
            {
                Dimension d = new();
                d.Variables = new List<string>();
                var dvalue = dim.Value;
                d.Name = dvalue.Label;
                var vvalue = dvalue.Category.Label;
                foreach(var v in vvalue)
                {
                    d.Variables.Add(v.Value.ToString());
                }
               

                Dimensions.Add(d);
            }
            
        }
    }

    [JsonObject]
    public class Dimension
    {
        public string Name { get; set; }
        public List<string> Variables { get; set; }
    }
}
