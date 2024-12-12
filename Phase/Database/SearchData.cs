using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phase.Database
{
    internal class SearchData
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Metadata { get; set; }
        public string Release { get; set; }
        public string ReleaseHash { get; set; }
        public string Summary { get; set; }
        public DateTime UpdateDate { get; set; }
        public float[] Vector { get; set; }
        public SearchDataOperation Operation { get; set; }
        public string LngIsoCode { get; set; }

    }

    internal enum SearchDataOperation
    {
        None,
        Append,
        Delete,
        Update,

    }
}
