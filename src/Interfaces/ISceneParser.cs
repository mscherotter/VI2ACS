using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace VIToACS.Interfaces
{
    /// <summary>
    /// Scene parser basic interface
    /// </summary>
    /// <typeparam name="T">the type of scene object to create</typeparam>
    public interface ISceneParser<T>
    {
        /// <summary>
        /// Gets the scenes from a document
        /// </summary>
        /// <param name="doc">a JSON document</param>
        /// <returns>an enumerable collection of scenes</returns>
        IEnumerable<T> GetScenes(JsonDocument doc);
    }
}
