using System.Collections.Generic;
using VIToACS.Models;

namespace VIToACS.Interfaces
{
    public interface IInsightsReader
    {
        IEnumerable<ParsedDocument> ReadInsightsFiles();
    }
}
