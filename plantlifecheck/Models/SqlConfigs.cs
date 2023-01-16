using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace plantlifecheck.Models
{
    public class SqlConfigs
    {
        public List<Config> Configs { get; set; }
    }
    public class Config
    {
        public string ConnectionString { get; set; }
        public List<Query> Queries { get; set; }
    }

    public class Query
    {
        public string Name { get; set; }
        public string Sql { get; set; }
        public bool Transaction { get; set; }
        public CommandType CommandType { get; set; }
        public string ReturnType { get; set; }
        public string DataInizio { get; set; }
        public string DataFine { get; set; }
        public List<ParameterList> ParametersList { get; set; }
        public List<Return> Returns { get; set; }
    }

    public class ParameterList
    {
        public string Parameter { get; set; }
        public string Value { get; set; }
    }
    public class Return
    {
        public string DataInizio { get; set; }
        public string DataFine { get; set; }
        public string Consuntivo { get; set; }
    }
    public class QueryConsuntivi
    {
        public QueryConsuntivi()
        {
            Consuntivi = new Dictionary<string, QueryConsuntivo>();
        }

        public IDictionary<string, QueryConsuntivo> Consuntivi { get; set; }

    }
    public class QueryConsuntivo
    {
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public long Consuntivo { get; set; }
    }
}
