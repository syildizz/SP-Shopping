namespace SP_Shopping.Utilities;

public class FileSignatureResolver
{

    public enum FileFormat
    {
        NONE, PNG, JPEG, BMP, GIF, TIFF
    }

    private readonly Dictionary<byte[], FileFormat> TypeFromBytes = new()
    {
        { [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], FileFormat.PNG },
        { [0xFF, 0xD8, 0xFF], FileFormat.JPEG },
        { [0x42, 0x4D], FileFormat.BMP},
        { [0x47, 0x49, 0x46, 0x38], FileFormat.GIF},
        { [0x49, 0x49], FileFormat.TIFF},
        { [0x4D, 0x4D], FileFormat.TIFF},
    };

    public FileFormat GetTypeFromFile(byte[] file)
    {
        foreach ((byte[] signature, FileFormat type) in TypeFromBytes)
        {
            if (file.Length >= signature.Length && file[0..signature.Length].SequenceEqual(signature))
            {
                return type;
            }
        }
        return FileFormat.NONE;
    }

}
