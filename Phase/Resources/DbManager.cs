using DuckDB.NET.Data;
using Phase.Database;
using API;
using Sample;

namespace Phase.Resources
{
    public static class DbManager
    {
        public static DuckDBConnection MemoryConnection { get; private set; }
        public static void Connect()
        {
            try
            {
                if (MemoryConnection != null) return;
                MemoryConnection = new DuckDBConnection(AppServicesHelper.AppConfiguration.Settings["MEMORY_CONNECTION"]);
                MemoryConnection.Open();
                using var command = MemoryConnection.CreateCommand();
                command.CommandText = "SELECT table_name FROM duckdb_tables WHERE table_name='td_metadata';";
                var reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    Log.Instance.Debug("Recreating data");
                    using var ddlCommand = MemoryConnection.CreateCommand();
                    ddlCommand.CommandText = "CREATE TABLE td_metadata(mtd_id INTEGER,  mtd_code VARCHAR,  mtd_metadata VARCHAR, mtd_release VARCHAR, mtd_datetime TIMESTAMP, mtd_vector FLOAT[1536], mtd_summary VARCHAR, mtd_release_hash VARCHAR,  mtd_lng_iso_code VARCHAR);";
                    ddlCommand.ExecuteNonQuery();

                    using var ddlCommandLoad = MemoryConnection.CreateCommand();
                    ddlCommandLoad.CommandText = String.Format("COPY td_metadata from '{0}'", AppServicesHelper.AppConfiguration.Settings["CSV_DATA_LOCATION"]);
                    ddlCommandLoad.ExecuteNonQuery();

                    // SET home_directory='/path/to/dir'
                    using var ddlHomeDirectory = MemoryConnection.CreateCommand();
                    ddlHomeDirectory.CommandText = "SET home_directory='C:\\Phase'";
                    ddlHomeDirectory.ExecuteNonQuery();

                    using var ddlInstall = MemoryConnection.CreateCommand();
                    ddlInstall.CommandText = "INSTALL vss;";
                    ddlInstall.ExecuteNonQuery();

                    using var ddlLoad = MemoryConnection.CreateCommand();
                    ddlLoad.CommandText = "LOAD vss;;";
                    ddlLoad.ExecuteNonQuery();

                    using var ddlIndex = MemoryConnection.CreateCommand();
                    ddlIndex.CommandText = "CREATE INDEX cos_idx_mtd_vector ON td_metadata USING HNSW (mtd_vector) WITH (metric = 'cosine')";
                    ddlIndex.ExecuteNonQuery();
                }
                else
                    Log.Instance.Debug("Reading from static data");

            }
            catch (Exception ex)
            {
                Log.Instance.Fatal(ex);
                throw;
            }
            
        }

        private static void Populate()
        {

        }
    }
}
