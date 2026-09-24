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

public class CryptoContext<T>(
    byte[] key,
    EncryptMode encryptMode,
    PaddingMode paddingMode,
    byte[]? initializationVector = null,
    params int[] parameters
) where T : ISymmetricalEncryptDecrypt
{
    private byte[] _key = key;
    private EncryptMode EncryptMode => encryptMode;
    private PaddingMode PaddingMode => paddingMode;

    private ISymmetricalEncryptDecrypt SymmetricalAlgorithm =>
        (ISymmetricalEncryptDecrypt)Activator.CreateInstance(typeof(T), _key)!;

    private byte[]? InitializationVector => initializationVector;
    private int[] Parameters => parameters;

    private ulong? _counter;
    private uint? _delta;

    public void Encrypt(byte[] data, ref byte[] outputBlock) => outputBlock = _Encrypt(data);
    public void Decrypt(byte[] data, ref byte[] outputBlock) => outputBlock = _Decrypt(data);

    public async Task EncryptAsync(string inputFilePath, string outputFilePath)
        => await _EncryptDecryptFilesAsync(inputFilePath, outputFilePath, EncryptAsync);

    public async Task DecryptAsync(string inputFilePath, string outputFilePath)
        => await _EncryptDecryptFilesAsync(inputFilePath, outputFilePath, DecryptAsync);

    public Task<byte[]> EncryptAsync(byte[] data) => Task.Run(() => _Encrypt(data));
    public Task<byte[]> DecryptAsync(byte[] data) => Task.Run(() => _Decrypt(data));

    private async Task _EncryptDecryptFilesAsync(
        string inputFilePath, string outputFilePath, Func<byte[], Task<byte[]>> asyncMethod
    )
    {
        await using var inputFile = new FileStream(
            inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true
        );
        await using var outputFile = new FileStream(
            outputFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true
        );

        var buffer = new byte[SymmetricalAlgorithm.BlockSizeBytes];
        while (true)
        {
            var read = await inputFile.ReadAsync(buffer);
            if (read == 0)
                break;

            var block = read == buffer.Length ? buffer : buffer[..read];

            var encrypted = await asyncMethod(block);
            await outputFile.WriteAsync(encrypted);
        }
    }

    private byte[] _Encrypt(byte[] data)
    {
        _ValidateInputParameters();

        data = _AddPadding(data);

        List<byte[]> encryptedData = [];
        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;

        var prevEncryptedBlock = InitializationVector!;
        var prevBlock = new byte[8];

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
        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;


        var prevDecryptedBlock = InitializationVector!;
        var prevGamma = InitializationVector!;
        var prevEncryptedBlock = encryptMode is EncryptMode.Pcbc ? new byte[8] : InitializationVector!;
        for (var offset = 0; offset < data.Length; offset += blockSizeBytes)
        {
            var endBlock = offset + blockSizeBytes;
            var block = data[offset..endBlock];

            var gamma = _BeforeDecrypt(block, prevDecryptedBlock, prevGamma);
            var decryptedBlock = _DoDecrypt(block, gamma, prevEncryptedBlock, prevDecryptedBlock);

            prevDecryptedBlock = decryptedBlock;
            prevEncryptedBlock = block;
            prevGamma = gamma;

            decryptedData.Add(decryptedBlock);
        }

        var decryptedDataArray = decryptedData.SelectMany(x => x).ToArray();
        return _RemovePadding(decryptedDataArray);
    }

    private byte[] _DoDecrypt(
        byte[] block, byte[] gamma, byte[] prevEncryptedBlock, byte[] prevDecryptedBlock
    )
    {
        switch (EncryptMode)
        {
            case EncryptMode.Ecb:
                return gamma;
            case EncryptMode.Cbc:
                return Helper.XorArrayOfBytes(gamma, prevEncryptedBlock);
            case EncryptMode.Pcbc:
                return Helper.XorArrayOfBytes(gamma, prevEncryptedBlock, prevDecryptedBlock);
            case EncryptMode.Cfb or EncryptMode.Ofb or EncryptMode.Ctr or EncryptMode.RandomDelta:
                return Helper.XorArrayOfBytes(gamma, block);
            default:
                throw new NotImplementedException();
        }
    }

    private byte[] _AddPadding(byte[] data)
    {
        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;

        if (data.Length % blockSizeBytes == 0)
            return data;

        var countMissingBytes = blockSizeBytes - (data.Length % blockSizeBytes);
        var newArray = new byte[countMissingBytes];
        switch (PaddingMode)
        {
            case PaddingMode.Zeros:
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
                var i = data.Length - 1;
                while (i >= 0 && data[i] == 0)
                {
                    listData.RemoveAt(i);
                    --i;
                }

                return listData.ToArray();
            case PaddingMode.AnsiX923 or PaddingMode.Pkcs7 or PaddingMode.Iso10126:
                var countToDelete = data[^1];
                var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;
                return countToDelete < blockSizeBytes ? data[..^countToDelete] : data;

            default:
                throw new NotImplementedException("Unknown padding mode!");
        }
    }

    private byte[] _BeforeEncrypt(byte[] block, byte[] prevEncryptedBlock, byte[] prevBlock)
    {
        if (EncryptMode != EncryptMode.Ecb && block.Length != prevEncryptedBlock.Length)
        {
            throw new InvalidOperationException("block and prevEncryptedBlock have different length");
        }

        var result = new byte[block.Length];
        switch (EncryptMode)
        {
            case EncryptMode.Ecb:
                return block;
            case EncryptMode.Cbc:
                result = Helper.XorArrayOfBytes(block, prevEncryptedBlock);
                break;
            case EncryptMode.Pcbc:
                if (prevBlock is null)
                    throw new InvalidOperationException("Encrypt mode is PCBC and prevBlock is null");

                if (prevBlock.Length != block.Length)
                    throw new InvalidOperationException("prevBlock and block have different length!");

                result = Helper.XorArrayOfBytes(block, prevEncryptedBlock, prevBlock);
                break;

            case EncryptMode.Cfb or EncryptMode.Ofb:
                return prevEncryptedBlock;

            case EncryptMode.Ctr or EncryptMode.RandomDelta:
                return _BeforeEncryptCtrAndRandomDelta();
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
                return Helper.XorArrayOfBytes(encryptedBlock, block);

            default:
                throw new NotImplementedException();
        }
    }

    private byte[] _BeforeDecrypt(byte[] block, byte[] prevDecryptedBlock, byte[] prevGamma)
    {
        switch (EncryptMode)
        {
            case EncryptMode.Ecb or EncryptMode.Cbc or EncryptMode.Pcbc:
                return SymmetricalAlgorithm.Decrypt(block);
            case EncryptMode.Cfb:
                return SymmetricalAlgorithm.Encrypt(prevDecryptedBlock);
            case EncryptMode.Ofb:
                return SymmetricalAlgorithm.Encrypt(prevGamma);
            case EncryptMode.Ctr or EncryptMode.RandomDelta:
                return _BeforeDecryptCtrAndRandomDelta();
            default:
                throw new NotImplementedException();
        }
    }

    private void _ValidateInputParameters()
    {
        if (
            InitializationVector is null &&
            EncryptMode is EncryptMode.Cbc or EncryptMode.Pcbc or EncryptMode.Cfb or EncryptMode.Ofb or EncryptMode.Ctr
                or EncryptMode.RandomDelta
        )
            throw new InvalidOperationException("Initializer vector is null");

        if (EncryptMode == EncryptMode.Ctr)
            _counter = Helper.TransformArrayBytesBigEndianToUlong(InitializationVector!);

        if (EncryptMode == EncryptMode.RandomDelta)
        {
            _counter = Helper.TransformArrayBytesBigEndianToUlong(InitializationVector!);
            _delta = (uint)(_counter >> (sizeof(byte) * 4));

            if (_delta % 2 == 0)
                ++_delta;
        }
    }

    private byte[] _BeforeEncryptCtrAndRandomDelta()
    {
        _ValidateEncryptDecryptCtrAndRandomDelta();

        var counterBytes = BitConverter.GetBytes(_counter!.Value);
        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;
        Array.Resize(ref counterBytes, blockSizeBytes);
        if (EncryptMode == EncryptMode.Ctr)
            ++_counter;
        else
            _counter += _delta;
        return counterBytes;
    }

    private byte[] _BeforeDecryptCtrAndRandomDelta()
    {
        _ValidateEncryptDecryptCtrAndRandomDelta();

        var counterBytes = BitConverter.GetBytes(_counter!.Value);
        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;
        Array.Resize(ref counterBytes, blockSizeBytes);
        if (EncryptMode == EncryptMode.Ctr)
            ++_counter;
        else
            _counter += _delta;
        return SymmetricalAlgorithm.Encrypt(counterBytes);
    }

    private void _ValidateEncryptDecryptCtrAndRandomDelta()
    {
        if (EncryptMode is not (EncryptMode.Ctr or EncryptMode.RandomDelta))
            throw new InvalidOperationException(
                $"Invalid use of method {nameof(_ValidateEncryptDecryptCtrAndRandomDelta)}"
            );

        if (encryptMode is EncryptMode.Ctr && _counter is null)
            throw new InvalidOperationException("The counter is null for CTR encryption mode");

        if (EncryptMode is EncryptMode.RandomDelta && (_counter is null || _delta is null))
            throw new InvalidOperationException("The counter or delta is null for Random delta encryption mode");
    }
}