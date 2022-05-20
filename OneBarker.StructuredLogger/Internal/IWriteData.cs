namespace OneBarker.StructuredLogger.Internal;

internal interface IWriteData
{
    void WriteData(byte[] data);

    void Flush();
}
