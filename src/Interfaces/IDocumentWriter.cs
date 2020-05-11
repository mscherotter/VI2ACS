using System;
using System.Collections.Generic;
using System.Text;

namespace VIToACS.Interfaces
{
    public interface IDocumentWriter
    {
        void WriteScenesDocument(string fileName, string content);
        void WriteThumbnailsDocument(string fileName, string content);
    }
}
