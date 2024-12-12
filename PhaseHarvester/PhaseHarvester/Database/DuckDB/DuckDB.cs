using DuckDB.NET.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PhaseHarvester
{
    internal class DuckDB:IDatabase
    {
        private DuckDBConnection duckDbConnection;

        internal void Connect()
        {
            Setup();
            if (duckDbConnection == null)
            {
                string cname = string.Format(ConfigurationManager.AppSettings["DUCKDB_CONNECTION"], ConfigurationManager.AppSettings["DUCKDB_LOCATION"]);
                duckDbConnection = new DuckDBConnection(cname);
                duckDbConnection.Open();
                
            }
            else
            {
                if (duckDbConnection.State.Equals(ConnectionState.Closed))
                    duckDbConnection.Open();
            }
        }

        internal void PrepareIndexes()
        {
            Connect();
            using var command = duckDbConnection.CreateCommand();
            command.CommandText = "INSTALL vss;";
            command.ExecuteNonQuery();
            using var command2 = duckDbConnection.CreateCommand();
            command2.CommandText = "LOAD vss;";
            command2.ExecuteNonQuery();

            using var command3=duckDbConnection.CreateCommand();
            command3.CommandText = "truncate table td_metadata";
            command3.ExecuteNonQuery();

        }

        internal void Append(SearchData searchData)
        {
            
            Connect();
            using var command = duckDbConnection.CreateCommand();
            
            command.CommandText = "INSERT INTO td_metadata ( mtd_id,mtd_code, mtd_metadata, mtd_release, mtd_datetime, mtd_vector, mtd_summary, mtd_release_hash, mtd_lng_iso_code) " +
                "VALUES(nextval('seq_mtd_id'),$code,$metadata,$release,$datetime,cast ($vector as float[1536]),$summary,$release_hash,$lng_iso_code);";
            
            command.Parameters.Add(new DuckDBParameter("code", searchData.Code));
            command.Parameters.Add(new DuckDBParameter("metadata",searchData.Metadata));
            command.Parameters.Add(new DuckDBParameter("release", searchData.Release));
            command.Parameters.Add(new DuckDBParameter("datetime", searchData.UpdateDate));
            command.Parameters.Add(new DuckDBParameter("vector", Utility.GetFloatArrayAsString(searchData.Vector)));
            command.Parameters.Add(new DuckDBParameter("summary", searchData.Summary));
            command.Parameters.Add(new DuckDBParameter("release_hash",searchData.ReleaseHash));
            command.Parameters.Add(new DuckDBParameter("lng_iso_code",searchData.LngIsoCode));

            command.ExecuteNonQuery();
        }

        internal void OutputCsv(string csvLocation)
        {
            
            Connect();
            using var command = duckDbConnection.CreateCommand();

            command.CommandText = string.Format("copy td_metadata TO '{0}'",csvLocation);
            command.ExecuteNonQuery();
        }


        internal SearchData  GetMetadata(string code,string lngIsoCode)
        {
            Connect();
            using var command = duckDbConnection.CreateCommand();
            command.CommandText = "SELECT mtd_id, mtd_code, mtd_metadata, mtd_release, mtd_datetime, mtd_vector, mtd_summary, mtd_release_hash, mtd_lng_iso_code FROM td_metadata " +
                "WHERE mtd_code=$code AND mtd_lng_iso_code=$lng_iso_code;";
            command.Parameters.Add(new DuckDBParameter("code", code));
            command.Parameters.Add(new DuckDBParameter("lng_iso_code",lngIsoCode));
            using var reader = command.ExecuteReader();
            var output = DuckDbHelper.GetReaderOutput(reader);
            
            return SearchSingleDataFromAdo(output); 
        }

        internal List<string>GetAllCodesFromDatabase()
        {
            Connect();
            using var command = duckDbConnection.CreateCommand();
            command.CommandText = "SELECT mtd_code FROM td_metadata; ";
            using var reader = command.ExecuteReader();
            var output = DuckDbHelper.GetReaderOutput(reader);
            List<string> codes = new List<string>();
            foreach (var item in output.data)
            {
                codes.Add(item.mtd_code);
            }
            return codes;
        }

        internal SearchData SearchSingleDataFromAdo(ADO_readerOutput ado)
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
            searchData.Release=data.mtd_release;
            searchData.Id= data.mtd_id;
            searchData.UpdateDate = data.mtd_datetime;
            return searchData;
        }

        internal void AppendTest()
        {
            Connect();
            using var command = duckDbConnection.CreateCommand();

            command.CommandText = "INSERT INTO integers(inta,intb) values($a,$b);";
            command.Parameters.Add(new DuckDBParameter("a", 3));
            command.Parameters.Add(new DuckDBParameter("b", 4));
            command.ExecuteNonQuery();
        }

        internal void Update(SearchData searchData)
        {
            Connect();
            using var command = duckDbConnection.CreateCommand();

            command.CommandText = "UPDATE phase.main.td_metadata " +
                "SET " +
                "mtd_metadata=$metadata, " +
                "mtd_release=$release, " +
                "mtd_datetime=$datetime, " +
                "mtd_vector=cast ($vector as float[1536]), " +
                "mtd_summary=$summary, " +
                "mtd_release_hash=$release_hash, " +
                "mtd_lng_iso_code=$lng_iso_code " +
                "WHERE mtd_id=$mtd_id ;"; 


            command.Parameters.Add(new DuckDBParameter("code", searchData.Code));
            command.Parameters.Add(new DuckDBParameter("metadata", searchData.Metadata));
            command.Parameters.Add(new DuckDBParameter("release", searchData.Release));
            command.Parameters.Add(new DuckDBParameter("datetime", searchData.UpdateDate));
            command.Parameters.Add(new DuckDBParameter("vector",  Utility.GetFloatArrayAsString(searchData.Vector)));
            command.Parameters.Add(new DuckDBParameter("summary", searchData.Summary));
            command.Parameters.Add(new DuckDBParameter("release_hash", searchData.ReleaseHash));
            command.Parameters.Add(new DuckDBParameter("lng_iso_code", searchData.LngIsoCode));
            command.Parameters.Add(new DuckDBParameter("mtd_id", searchData.Id));


            command.ExecuteNonQuery();

        }

        internal void Delete(string mtrCode)
        {
            Connect();
            using var command = duckDbConnection.CreateCommand();

            command.CommandText = "DELETE FROM phase.main.td_metadata WHERE mtd_code=$code";
            command.Parameters.Add(new DuckDBParameter("code", mtrCode));
            command.ExecuteNonQuery();
        }





        internal void Setup()
        {
            string duckDBlocation = ConfigurationManager.AppSettings["DUCKDB_LOCATION"];
            if(File.Exists(duckDBlocation))  return;
            else
            {
                string cname = string.Format(ConfigurationManager.AppSettings["DUCKDB_CONNECTION"], ConfigurationManager.AppSettings["DUCKDB_LOCATION"]);
                duckDbConnection = new DuckDBConnection(cname);
                duckDbConnection.Open();
                using var command = duckDbConnection.CreateCommand();

                command.CommandText = "CREATE TABLE td_metadata(mtd_id INTEGER, mtd_code VARCHAR, mtd_metadata VARCHAR, mtd_release VARCHAR, mtd_datetime TIMESTAMP, mtd_vector FLOAT[1536], mtd_summary VARCHAR, mtd_release_hash VARCHAR, mtd_lng_iso_code VARCHAR);";

                command.ExecuteNonQuery();

                using var commandSeq = duckDbConnection.CreateCommand();
                commandSeq.CommandText = "CREATE SEQUENCE seq_mtd_id  START WITH 1 INCREMENT BY 1;";
                commandSeq.ExecuteNonQuery();
            }

        }
    }
}
