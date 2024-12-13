using API;
using PxStat.Template;
using Phase.Database;
using Phase.Resources;

namespace Phase
{
    internal class Search_BSO_Read : BaseTemplate_Read<Search_DTO, Search_VLD>
    {
        internal Search_BSO_Read(IRequest request) : base(request, new Search_VLD())
        {
            /*
             If using DuckDB run this first on the database:

              INSTALL vss;
              LOAD vss;

              SET hnsw_enable_experimental_persistence =true;

              CREATE INDEX idx_mtd_vector ON td_metadata USING HNSW (mtd_vector);
             */
        }

        override protected bool HasPrivilege()
        {
            return true;
        }

        protected override bool HasUserToBeAuthenticated()
        {
            return false;
        }

        protected override bool Execute()
        {
            // DbManager.Connect();
            Log.Instance.Info("Starting search..");
            Ai ai = new();
            float[] vectors = ai.GetVectors(DTO.Search);
            if (vectors == null) {
                Log.Instance.Error("No vectors found for query");
            }

            else Log.Instance.Info("Vectors returned, size=" + vectors.Length);
            DuckDb db = new();
           
            var response = db.SearchByVector(vectors, DbManager.MemoryConnection);
            List<Dataset>results = new List<Dataset>();
            foreach (var item in response.data)
            {
                var obj = API.Utility.JsonDeserialize_IgnoreLoopingReference<Dataset>(item.mtd_metadata.ToString());
                results.Add(obj);
            }
            if (response != null)
            {
                Log.Instance.Info("Items returned=" + response.data.Count);
                Response.statusCode=System.Net.HttpStatusCode.OK;
                
                Response.data = results;
            }
            else {
                Log.Instance.Error("No data returned");
                return false;
            }
            return true;
        }

    }
    


    
}



/*
     internal class Group_BSO_Read : BaseTemplate_Read<Group_DTO_Read, Group_VLD_Read>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request"></param>
        internal Group_BSO_Read(JSONRPC_API request) : base(request, new Group_VLD_Read())
        {
        }

        /// <summary>
        /// Test privilege
        /// </summary>
        /// <returns></returns>
        override protected bool HasPrivilege()
        {
            return IsPowerUser() || IsModerator();
        }

        /// <summary>
        /// Execute
        /// </summary>
        /// <returns></returns>
        protected override bool Execute()
        {}
 */
