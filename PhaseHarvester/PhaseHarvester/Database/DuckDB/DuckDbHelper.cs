using DuckDB.NET.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhaseHarvester
{
    internal class DuckDbHelper
    {
        public static ADO_readerOutput GetReaderOutput(DuckDBDataReader reader )
        {
            // Initialise Size
            int dataSize = 0;
            // Initialise resultSetIndex
            int resultSetIndex = 0;
            ADO_readerOutput readerOutput = new ADO_readerOutput();
            if (reader.HasRows)
            {
                // Set the flag to check quickly the data exists
                readerOutput.hasData = true;

                // Initialise resultSet that is incremented for each resultSet
                readerOutput.meta.Add(new List<dynamic>());
                readerOutput.data.Add(new List<dynamic>());
                

                int rowIndex = 0;
                while (reader.Read())
                {
                    // Initialise a new dynamic object
                    dynamic readerData = new ExpandoObject();
                    // Implement the interface for handling dynamic properties
                    var readerData_IDictionary = readerData as IDictionary<string, object>;

                    // Initilise a new dynamic object
                    dynamic readerColumn = new ExpandoObject();
                    // Implement the interface for handling dynamic properties
                    var readerColumn_IDictionary = readerColumn as IDictionary<string, object>;

                    for (int columnIndex = 0; columnIndex < reader.FieldCount; columnIndex++)
                    {
                        // Set Column Name
                        string columnName = reader.GetName(columnIndex).ToString();
                        // Set Column Value
                        dynamic columnValue = reader[columnIndex];
                        // Add Size
                        dataSize += Convert.ToString(columnValue).Length * sizeof(Char);

                        if (rowIndex == 0)
                        {
                            // Get Meta information
                            ADO_readerMetadata readerMetadata = new ADO_readerMetadata();
                            readerMetadata.specificType = reader.GetProviderSpecificFieldType(columnIndex).FullName.ToString();
                            readerMetadata.dotNetType = reader.GetFieldType(columnIndex).FullName.ToString();
                            readerMetadata.sqlType = reader.GetDataTypeName(columnIndex).ToString();

                            // Add the column Metadata
                            readerColumn_IDictionary.Add(columnName, readerMetadata);
                        }

                        // Add the column to the Data
                        readerData_IDictionary.Add(columnName, columnValue);
                    }

                    if (rowIndex == 0)
                    {
                        // Append the Metadata to the Output once only
                        readerOutput.meta[resultSetIndex].Add(readerColumn_IDictionary);
                    }

                    // Append the Data to the Output for each row
                    readerOutput.data[resultSetIndex].Add(readerData_IDictionary);

                    rowIndex++;
                }

                // Move to the next dataset if any
                reader.NextResult();
                resultSetIndex++;
            }




            // Evaluate if more resultset exist
            if (readerOutput.meta.Count == 1 && readerOutput.data.Count == 1)
            {
                readerOutput.meta = readerOutput.meta[0];
                readerOutput.data = readerOutput.data[0];
            }

            // Set the size of the data
            readerOutput.sizeInByte = dataSize;



            return readerOutput;

        }
    }

    public class ADO_readerMetadata
    {
        #region Properties
        /// <summary>
        /// Specific type
        /// </summary>
        public string specificType { get; internal set; }

        /// <summary>
        /// .NET type
        /// </summary>
        public string dotNetType { get; internal set; }

        /// <summary>
        /// SQL type
        /// </summary>
        public string sqlType { get; internal set; }

        #endregion
    }

    public class ADO_readerOutput
    {
        #region Properties
        /// <summary>
        /// Flag to indicate if any data exists
        /// </summary>
        public bool hasData { get; set; }

        /// <summary>
        /// Size of the output in Bytes
        /// </summary>
        public int sizeInByte { get; internal set; }

        /// <summary>
        /// Time spent
        /// </summary>
        public float timeInSec { get; internal set; }

        /// <summary>
        /// Meta output
        /// </summary>
        public List<dynamic> meta { get; internal set; }

        /// <summary>
        /// Data output
        /// </summary>
        public List<dynamic> data { get; set; }

        #endregion

        /// <summary>
        /// Initialise a blank Output 
        /// </summary>
        public ADO_readerOutput()
        {
            hasData = false;
            sizeInByte = 0;
            timeInSec = 0;
            meta = new List<dynamic>();
            data = new List<dynamic>();
        }

    }

    public class Table
    {
        public string Name { get; set; }
        public List<Column> Columns { get; set; }
        public string Url { get; set; }
        public int RowCount { get; set; }

        public Table(string tableName, string url = null)
        {
            this.Name = tableName;
            this.Columns = new List<Column>();
            this.Url = url;
        }
    }

    public class Column
    {
        public string Name { get; set; }
        public Table Table { get; set; }

        public Column(string name, Table table)
        {
            this.Name = name;
            this.Table = table;
        }
    }
}
