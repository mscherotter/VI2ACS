using System.Collections.Generic;
using VIToACS.Models;

namespace VIToACS.Interfaces
{
    public interface IInsightsReader<T>
    {
        IEnumerable<ParsedDocument<T>> ReadInsightsFiles();
        void AddNewFile(string fileName, string content);
    }
}
