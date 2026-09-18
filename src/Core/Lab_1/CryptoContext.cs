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
    params int[] parameters
)
{
    private byte[] _key = key;
    private EncryptMode EncryptMode => encryptMode;
    private PaddingMode PaddingMode => paddingMode;
    private byte[]? InitializationVector => initializationVector;
    private int[] Parameters => parameters;

    private int? _counter;
    private int? _delta;

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
        _ValidateInputParameters();

        data = _AddPadding(data);

        List<byte[]> encryptedData = [];
        var blockSizeBytes = SymmetricalAlgorithm!.BlockSizeBytes;

        var prevEncryptedBlock = InitializationVector!;
        byte[]? prevBlock = null;

        for (var offset = 0; offset < data.Length; offset += blockSizeBytes)
        {
            var endBlock = offset + blockSizeBytes;
            var block = data[offset..endBlock];

            var gamma = _BeforeEncrypt(block, prevEncryptedBlock, prevBlock);
            var encryptedBlock = SymmetricalAlgorithm.Encrypt(gamma);

            var afterEncrypt = _AfterEncrypt(encryptedBlock, block);
            encryptedData.Add(afterEncrypt);

            prevEncryptedBlock = encryptedBlock;
            prevBlock = block;
        }

        return encryptedData.SelectMany(x => x).ToArray();
    }

    private byte[] _Decrypt(byte[] data)
    {
        _ValidateInputParameters();

        List<byte[]> decryptedData = [];
        var blockSizeBytes = SymmetricalAlgorithm!.BlockSizeBytes;


        var prevDecryptedBlock = InitializationVector!;
        var prevGamma = InitializationVector!;
        var prevEncryptedBlock = new byte[blockSizeBytes];
        for (var offset = 0; offset < data.Length; offset += blockSizeBytes)
        {
            var endBlock = offset + blockSizeBytes;
            var block = data[offset..endBlock];

            var gamma = _BeforeDecrypt(block, prevDecryptedBlock, prevGamma);
            var decryptedBlock = _DoDecrypt(block, gamma, prevEncryptedBlock, prevDecryptedBlock);

            prevDecryptedBlock = decryptedBlock;
            prevEncryptedBlock = block;

            decryptedData.Add(decryptedBlock);
        }

        var decryptedDataArray = decryptedData.SelectMany(x => x).ToArray();
        return _RemovePadding(decryptedDataArray);
    }

    private byte[] _DoDecrypt(byte[] block, byte[] gamma, byte[] prevEncryptedBlock,
        byte[] prevDecryptedBlock)
    {
        switch (EncryptMode)
        {
            case EncryptMode.Ecb:
                return gamma;
            case EncryptMode.Cbc:
                return _XorArrayOfBytes(gamma, prevEncryptedBlock);
            case EncryptMode.Pcbc:
                return _XorArrayOfBytes(gamma, prevEncryptedBlock, prevDecryptedBlock);
            case EncryptMode.Cfb or EncryptMode.Ofb or EncryptMode.Ctr or EncryptMode.RandomDelta:
                return _XorArrayOfBytes(gamma, block);
            default:
                throw new NotImplementedException();
        }
    }

    private byte[] _AddPadding(byte[] data)
    {
        var blockSizeBytes = SymmetricalAlgorithm!.BlockSizeBytes;
        var countMissingBytes = blockSizeBytes - (data.Length % blockSizeBytes);
        var newArray = new byte[countMissingBytes];
        switch (PaddingMode)
        {
            case PaddingMode.Zeros:
                if (countMissingBytes == blockSizeBytes) return data; // not required add empty block
                break;
            case PaddingMode.AnsiX923:
                newArray[^1] = (byte)countMissingBytes;
                break;
            case PaddingMode.Pkcs7:
                Array.Fill(newArray, (byte)countMissingBytes);
                break;
            case PaddingMode.Iso10126:
                var random = new Random();
                for (var i = 0; i < countMissingBytes - 1; ++i)
                {
                    newArray[i] = (byte)random.Next();
                }

                newArray[^1] = (byte)countMissingBytes;
                break;
            default:
                throw new NotImplementedException("Unknown padding mode!");
        }

        return data.Concat(newArray).ToArray();
    }

    private byte[] _RemovePadding(byte[] data)
    {
        switch (PaddingMode)
        {
            case PaddingMode.Zeros:
                var listData = data.ToList();
                for (var i = data.Length - 1; i >= 0; --i)
                    listData.RemoveAt(i);

                return listData.ToArray();
            case PaddingMode.AnsiX923 or PaddingMode.Pkcs7 or PaddingMode.Iso10126:
                var countToDelete = data[^1];
                return data[..^countToDelete];

            default:
                throw new NotImplementedException("Unknown padding mode!");
        }
    }

    private byte[] _BeforeEncrypt(byte[] block, byte[] prevEncryptedBlock, byte[]? prevBlock = null)
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
                    throw new InvalidOperationException("Encrypt mode is PCBC and prevBlock is null");

                if (prevBlock.Length != block.Length)
                    throw new InvalidOperationException("prevBlock and block have different length!");

                result = _XorArrayOfBytes(block, prevEncryptedBlock, prevBlock);
                break;

            case EncryptMode.Cfb or EncryptMode.Ofb:
                return prevEncryptedBlock;

            case EncryptMode.Ctr or EncryptMode.RandomDelta:
                return _BeforeEncryptOrDecryptCtrAndRandomDelta();
        }

        return result;
    }

    private byte[] _AfterEncrypt(byte[] encryptedBlock, byte[] block)
    {
        if (block.Length != encryptedBlock.Length)
            throw new InvalidOperationException("The blocks have different length");

        switch (EncryptMode)
        {
            case EncryptMode.Ecb or EncryptMode.Cbc or EncryptMode.Pcbc:
                return encryptedBlock;

            case EncryptMode.Cfb or EncryptMode.Ofb or EncryptMode.Ctr or EncryptMode.RandomDelta:
                return _XorArrayOfBytes(encryptedBlock, block);

            default:
                throw new NotImplementedException();
        }
    }

    private byte[] _BeforeDecrypt(byte[] block, byte[] prevDecryptedBlock, byte[] prevGamma)
    {
        var blockSizeBytes = SymmetricalAlgorithm!.BlockSizeBytes;
        switch (EncryptMode)
        {
            case EncryptMode.Ecb or EncryptMode.Cbc or EncryptMode.Pcbc:
                return SymmetricalAlgorithm!.Decrypt(block);
            case EncryptMode.Cfb:
                return SymmetricalAlgorithm!.Encrypt(prevDecryptedBlock);
            case EncryptMode.Ofb:
                return SymmetricalAlgorithm!.Encrypt(prevGamma);
            case EncryptMode.Ctr or EncryptMode.RandomDelta:
                return _BeforeEncryptOrDecryptCtrAndRandomDelta();
            default:
                throw new NotImplementedException();
        }
    }

    private void _ValidateInputParameters()
    {
        if (SymmetricalAlgorithm is null)
            throw new InvalidOperationException("The symmetrical algorithm is not initialized!");

        if (
            InitializationVector is null &&
            EncryptMode is EncryptMode.Cbc or EncryptMode.Pcbc or EncryptMode.Cfb or EncryptMode.Ofb
        )
            throw new InvalidOperationException("Initializer vector is null");

        if (EncryptMode == EncryptMode.Ctr)
            _counter = Parameters[0];

        if (EncryptMode == EncryptMode.RandomDelta)
        {
            _counter = Parameters[0];
            _delta = Parameters[1];
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

    private byte[] _BeforeEncryptOrDecryptCtrAndRandomDelta()
    {
        if (EncryptMode is not (EncryptMode.Ctr or EncryptMode.RandomDelta))
            throw new InvalidOperationException("Invalid use of method _BeforeEncryptOrDecryptCtrAndRandomDelta");
        
        if (_counter is null || _delta is null)
            throw new InvalidOperationException(
                "The counter or delta is null for Random delta encryption mode");

        var counterBytes = BitConverter.GetBytes(_counter.Value);
        var blockSizeBytes = SymmetricalAlgorithm!.BlockSizeBytes;
        Array.Resize(ref counterBytes, blockSizeBytes);
        if (EncryptMode == EncryptMode.Ctr)
            ++_counter;
        else
            _counter += _delta;
        return counterBytes;
    }
}