using AngleSharp.Dom;
using DuckDB.NET.Data;
using Sample;
using System.Data;
using System.Numerics;
using static DuckDB.NET.Native.NativeMethods;

namespace Phase.Database
    
{
    public class DuckDb : IDatabase
    {
        private DuckDBConnection duckDbConnection;

        private void Connect()
        {
            if (duckDbConnection == null)
            {
                string cname = string.Format(AppServicesHelper.AppConfiguration.Settings["DUCKDB_CONNECTION"], AppServicesHelper.AppConfiguration.Settings["DUCKDB_LOCATION"]);
                duckDbConnection = new DuckDBConnection(cname);
                duckDbConnection.Open();
            }
            else
            {
                if (duckDbConnection.State.Equals(ConnectionState.Closed))
                    duckDbConnection.Open();
            }
        }

        private void Connect(string db)
        {
            if (duckDbConnection == null)
            {
                duckDbConnection = new DuckDBConnection(db);
                duckDbConnection.Open();
            }
            else
            {
                if (duckDbConnection.State.Equals(ConnectionState.Closed))
                    duckDbConnection.Open();
            }
        }




        internal ADO_readerOutput SearchByVector(float[] vector, DuckDBConnection con=null)
        {
            if (con == null)
                Connect();
            else
                duckDbConnection = con;
            string vectorString=Utility.GetFloatArrayAsString(vector);
            using var command = duckDbConnection.CreateCommand();
            command.CommandText = "SELECT mtd_metadata FROM td_metadata " +
                "ORDER BY array_cosine_distance(mtd_vector,cast ($vector as float[1536])::FLOAT[1536]) LIMIT $read_limit;";
            command.Parameters.Add(new DuckDBParameter("vector", Utility.GetFloatArrayAsString(vector)));
            command.Parameters.Add(new DuckDBParameter("read_limit", AppServicesHelper.AppConfiguration.Settings["SEARCH_LIMIT"]));
            using var reader = command.ExecuteReader();
            
            return DuckDbHelper.GetReaderOutput(reader);
        }

        private SearchData SearchSingleDataFromAdo(ADO_readerOutput ado)
        {
            if (ado == null) return null;
            if (!ado.hasData) return null;
            var searchData = new SearchData();
            var data = ado.data[0];
            searchData.Code = data.mtd_code;
            searchData.Summary = data.mtd_summary;
            searchData.ReleaseHash = data.mtd_release_hash;
            searchData.LngIsoCode = data.mtd_lng_iso_code;
            searchData.Vector = data.mtd_vector.ToArray();
            searchData.Release = data.mtd_release;
            searchData.Id = data.mtd_id;
            searchData.UpdateDate = data.mtd_datetime;
            return searchData;
        }
    }


}
