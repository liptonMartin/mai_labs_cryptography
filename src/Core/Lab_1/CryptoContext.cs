using System.Security.Cryptography;

namespace Core.Lab_1;

public enum EncryptMode
{
    Ecb,
    Cbc,
    Pcbc,
    Cfb,
    Ofb,
    Ctr,
    RandomDelta,
}

public enum PaddingMode
{
    Zeros,
    AnsiX923,
    Pkcs7,
    Iso10126
}

public class CryptoContext(
    byte[] key,
    EncryptMode encryptMode,
    PaddingMode paddingMode,
    byte[]? initializationVector = null,
    params object[] parameters
)
{
    private byte[] _key = key;
    private EncryptMode EncryptMode => encryptMode;
    private PaddingMode PaddingMode => paddingMode;
    private byte[]? InitializationVector => initializationVector;
    private object[] Parameters => parameters;

    private int? _counter;

    public ISymmetricalEncryptDecrypt? SymmetricalAlgorithm { get; set; }

    public void Encrypt(byte[] data, ref byte[] outputBlock) => outputBlock = _Encrypt(data);
    public void Decrypt(byte[] data, ref byte[] outputBlock) => outputBlock = _Decrypt(data);

    public async Task EncryptAsync(string inputFile, string outputFile)
    {
        var data = await File.ReadAllBytesAsync(inputFile);
        var encrypted = await EncryptAsync(data);
        await File.WriteAllBytesAsync(outputFile, encrypted);
    }

    public async Task DecryptAsync(string inputFile, string outputFile)
    {
        var data = await File.ReadAllBytesAsync(inputFile);
        var decrypted = await DecryptAsync(data);
        await File.WriteAllBytesAsync(outputFile, decrypted);
    }

    public Task<byte[]> EncryptAsync(byte[] data) => Task.Run(() => _Encrypt(data));
    public Task<byte[]> DecryptAsync(byte[] data) => Task.Run(() => _Decrypt(data));

    private byte[] _Encrypt(byte[] data)
    {
        if (SymmetricalAlgorithm is null)
            throw new InvalidOperationException("The symmetrical algorithm is not initialized!");

        var encryptedData = new byte[data.Length];
        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;

        if (
            InitializationVector is null &&
            EncryptMode is EncryptMode.Cbc or EncryptMode.Pcbc or EncryptMode.Cfb or EncryptMode.Ofb
        )
            throw new InvalidOperationException("Initializer vector is null");

        if (EncryptMode == EncryptMode.Ctr)
            _counter = (int)Parameters[0];

        var prevEncryptedBlock = InitializationVector;
        byte[]? prevBlock = null;

        for (var offset = 0; offset < data.Length; offset += blockSizeBytes)
        {
            var endBlock = Math.Min(data.Length - 1, offset + blockSizeBytes);
            var block = data[offset..endBlock];
            if (data.Length - 1 <= endBlock)
                block = _PadBlock(block);

            var blockByEncryptMode = _JoinEncryptMode(block, prevEncryptedBlock!, prevBlock);
            var encryptedBlock = SymmetricalAlgorithm.Encrypt(blockByEncryptMode);

            var totalEncryptedBlock = _DefineTotalEncryptedBlock(encryptedBlock, block);
            encryptedData = encryptedData.Concat(totalEncryptedBlock).ToArray();

            prevEncryptedBlock = encryptedBlock;
            prevBlock = block;
        }

        return encryptedData;
    }

    private byte[] _Decrypt(byte[] data)
    {
        throw new NotImplementedException();
    }

    private byte[] _PadBlock(byte[] block)
    {
        var blockSizeBytes = SymmetricalAlgorithm!.BlockSizeBytes;
        var countMissingBytes = blockSizeBytes - block.Length;
        var lengthNewArray = countMissingBytes > 0 ? countMissingBytes : countMissingBytes + 1;
        var newArray = new byte[lengthNewArray];
        switch (PaddingMode)
        {
            case PaddingMode.Zeros:
                if (block.Length == blockSizeBytes) return block;
                break;
            case PaddingMode.AnsiX923:
                newArray[^1] = (byte)countMissingBytes;
                break;
            case PaddingMode.Pkcs7:
                Array.Fill(newArray, (byte)countMissingBytes);
                break;
            case PaddingMode.Iso10126:
                var random = new Random();
                for (var i = 0; i < lengthNewArray - 1; ++i)
                {
                    newArray[i] = (byte)random.Next();
                }

                newArray[^1] = (byte)countMissingBytes;
                break;
        }

        return block.Concat(newArray).ToArray();
    }

    private byte[] _JoinEncryptMode(byte[] block, byte[] prevEncryptedBlock, byte[]? prevBlock = null)
    {
        if (block.Length != prevEncryptedBlock.Length)
        {
            throw new InvalidOperationException("block and prevEncryptedBlock have different length");
        }

        var result = new byte[block.Length];
        switch (EncryptMode)
        {
            case EncryptMode.Ecb:
                return block;
            case EncryptMode.Cbc:
                result = _XorArrayOfBytes(block, prevEncryptedBlock);
                break;
            case EncryptMode.Pcbc:
                if (prevBlock is null)
                {
                    throw new InvalidOperationException("Encrypt mode is PCBC and prevBlock is null");
                }

                if (prevBlock.Length != block.Length)
                {
                    throw new InvalidOperationException("prevBlock and block have different length!");
                }

                result = _XorArrayOfBytes(block, prevEncryptedBlock, prevBlock);
                break;

            case EncryptMode.Cfb or EncryptMode.Ofb:
                return prevEncryptedBlock;

            case EncryptMode.Ctr:
                if (_counter is null)
                {
                    throw new InvalidOperationException("The counter is null for CTR encryption mode");
                }

                var counterBytes = BitConverter.GetBytes(_counter.Value);
                return _PadBlock(counterBytes);

            case EncryptMode.RandomDelta:
                // TODO: do random delta 
                throw new NotImplementedException();
        }

        return result;
    }

    private byte[] _DefineTotalEncryptedBlock(byte[] encryptedBlock, byte[] block)
    {
        if (block.Length != encryptedBlock.Length)
            throw new InvalidOperationException("The blocks have different length");

        switch (EncryptMode)
        {
            case EncryptMode.Ecb or EncryptMode.Cbc or EncryptMode.Pcbc:
                return encryptedBlock;

            case EncryptMode.Cfb or EncryptMode.Ofb or EncryptMode.Ctr:
                return _XorArrayOfBytes(encryptedBlock, block);

            case EncryptMode.RandomDelta:
                // TODO: do random delta
                throw new NotImplementedException();

            default:
                throw new NotImplementedException();
        }
    }


    private byte[] _XorArrayOfBytes(params byte[][] arrays)
    {
        var length = arrays[0].Length;
        var result = new byte[length];
        for (var i = 0; i < length; ++i)
        {
            foreach (var array in arrays)
            {
                result[i] ^= array[i];
            }
        }

        return result;
    }
}