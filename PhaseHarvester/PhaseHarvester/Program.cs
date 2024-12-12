
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Json.Serialization;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PhaseHarvester.AIModels;
using PxStat.Data;

namespace PhaseHarvester
{
    internal class Program
    {
        static void Main(string[] args)
        {


            Stopwatch sw = Stopwatch.StartNew();

            DuckDB db = new();
            //db.Connect();
            // db.PrepareIndexes();

            //Get catalog
            //Foreach item in catalog
            //      get code
            //      get metadata
            //      test if metadata exists already/has not changed
            //      create summary via ai           
            //      get vectors based on summary
            //      store summary & vectors

            /*
             CREATE TABLE td_metadata(mtd_id INTEGER,
                mtd_code VARCHAR,
                mtd_metadata VARCHAR,
                mtd_release VARCHAR,
                mtd_datetime TIMESTAMP,
                mtd_vector FLOAT[1536],
                mtd_summary VARCHAR,
                mtd_release_hash VARCHAR,
                mtd_lng_iso_code VARCHAR);

            create index seq_mtd_id; 
             
             */
            
            List<SearchData> searchDataList = new();

            string readResult = PxCatalog.Read(ConfigurationManager.AppSettings["CATALOG_URL"]);
            List<Release> releases = JsonConvert.DeserializeObject<List<Release>>(readResult);
            PxCatalog.Releases = releases;
            int counter = 1;
            int rcount = releases.Where(x => x.LngIsoCode.Equals("en")).Count();
            foreach (var item in releases.Where(x=>x.LngIsoCode.Equals("en")))           
            {
                Console.WriteLine(String.Format("Matrix {0}, item {1} of {2}", item.MtrCode, counter.ToString(), rcount.ToString()));
                string releaseString = JsonConvert.SerializeObject(item);
                string releaseHash = Utility.GetHashString(releaseString);


                SearchData dbSearchData = db.GetMetadata(item.MtrCode,item.LngIsoCode);
                if (dbSearchData != null)
                {
                    if (dbSearchData.ReleaseHash.Equals(releaseHash))
                    {
                        //No change 
                        dbSearchData.Operation = SearchDataOperation.None;
                        searchDataList.Add(dbSearchData);
                        counter++;
                        continue;
                    }
                    else
                    {

                        SearchData searchData = GetSearchDataFromCatalogAndAI(item, releaseString, releaseHash);
                        searchData.Operation = SearchDataOperation.Update;
                        searchDataList.Add(searchData);
                        //We need to get the existing data and do an update
                    }
                }
                else
                {
                    //We need to enter new data to the database
                    SearchData searchData = GetSearchDataFromCatalogAndAI(item, releaseString, releaseHash);
                    searchData.Operation = SearchDataOperation.Append;
                    searchDataList.Add(searchData);
                }
                counter++;
            }

            List<string> allDbCodes = db.GetAllCodesFromDatabase();
            List<string> allCatalogCodes = releases.Select(x => x.MtrCode).ToList();


            List<string> deleteCandidates = allDbCodes.Where(x => !allCatalogCodes.Any(y => y.Equals(x))).ToList();

            int deleteCounter = 0;

            int appendCounter = 0;
            int updateCounter = 0;

        
            foreach (var item in searchDataList.Where(x => x.Operation.Equals(SearchDataOperation.Append))) { db.Append(item); appendCounter++; }
            foreach (var item in searchDataList.Where(x => x.Operation.Equals(SearchDataOperation.Update))) { db.Update(item); updateCounter++; }

            Console.WriteLine(String.Format("Process ended - total time: {0} seconds", (int)sw.ElapsedMilliseconds / 1000));
            Console.WriteLine(String.Format("Append Count: {0}, Update Count: {1}, Delete Count: {2}", appendCounter, updateCounter, deleteCounter));

            if(deleteCounter>0|| appendCounter>0 || updateCounter>0 || !File.Exists(ConfigurationManager.AppSettings["CSV_LOCATION"]))
            {
                Console.WriteLine("Rebuilding csv..");
                db.OutputCsv(ConfigurationManager.AppSettings["CSV_LOCATION"]);
            }
        }


        private static SearchData GetSearchDataFromCatalogAndAI(Release release,string releaseString, string releaseHash)
        {
            SearchData searchData = new SearchData();
            string url = String.Format(ConfigurationManager.AppSettings["METADATA_URL"], release.MtrCode, release.LngIsoCode);
            string readResult = PxCatalog.Read(url);
            JsonStat jsData = JsonConvert.DeserializeObject<JsonStat>(readResult, Converter.Settings);
            var prompt = new Dataset(jsData);
            string JsonDataset = JsonConvert.SerializeObject(prompt);

            searchData.UpdateDate = release.RlsLiveDatetimeFrom;
            searchData.Release = releaseString;
            searchData.Metadata =  JsonDataset;
            searchData.ReleaseHash = releaseHash;
            searchData.LngIsoCode = release.LngIsoCode;
            searchData.Code = release.MtrCode;

            AiReader ai = new();
            searchData.Summary = ai.GetAiSummary(JsonDataset);
            searchData.Vector = ai.GetVectors(searchData.Summary);

            return searchData;
        }
    }


}
