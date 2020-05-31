using System.IO;

namespace VIToACS.Interfaces
{
    public interface IDocumentWriter
    {
        void WriteScenesDocument(string fileName, string content);
        void WriteThumbnailsDocument(string fileName, string content);
        void WriteThumbnailImage(string fileName, byte[] bytes);
    }
}
