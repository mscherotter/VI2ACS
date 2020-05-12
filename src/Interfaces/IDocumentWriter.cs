namespace VIToACS.Interfaces
{
    public interface IDocumentWriter
    {
        void WriteScenesDocument(string fileName, string content);
        void WriteThumbnailsDocument(string fileName, string content);
    }
}
