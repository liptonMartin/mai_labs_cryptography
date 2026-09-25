using System.Numerics;

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

public struct EncryptBlocks(byte[] encryptedBlock, byte[] afterEncryptBlock)
{
    public readonly byte[] EncryptedBlock = encryptedBlock;
    public readonly byte[] AfterEncryptBlock = afterEncryptBlock;
}

public struct DecryptBlocks(byte[] gamma, byte[] decryptedBlock)
{
    public readonly byte[] Gamma = gamma;
    public readonly byte[] DecryptedBlock = decryptedBlock;
}

public class CryptoContext<T>(
    byte[] key,
    EncryptMode encryptMode,
    PaddingMode paddingMode,
    byte[]? initializationVector = null,
    params int[] parameters
) where T : ISymmetricalEncryptDecrypt
{
    private readonly byte[] _key = key;
    private EncryptMode EncryptMode => encryptMode;
    private PaddingMode PaddingMode => paddingMode;

    private ISymmetricalEncryptDecrypt SymmetricalAlgorithm =>
        (ISymmetricalEncryptDecrypt)Activator.CreateInstance(typeof(T), _key)!;

    private byte[]? InitializationVector => initializationVector;
    private int[] Parameters => parameters;

    private BigInteger? _counter;
    private BigInteger? _delta;

    public async Task<byte[]> EncryptAsync(byte[] data) => await _Encrypt(data);

    public async Task<byte[]> DecryptAsync(byte[] data) => await _Decrypt(data);

    public async Task EncryptAsync(string inputFilePath, string outputFilePath)
        => await _EncryptDecryptFilesAsync(inputFilePath, outputFilePath, EncryptAsync);

    public async Task DecryptAsync(string inputFilePath, string outputFilePath)
        => await _EncryptDecryptFilesAsync(inputFilePath, outputFilePath, DecryptAsync);

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

    private async Task<byte[]> _Encrypt(byte[] data)
    {
        _ValidateInputParameters();
        data = _AddPadding(data);
        if (_isParallelEncrypt())
            return await _ParallelEncrypt(data);
        return _NonParallelEncrypt(data);
    }

    private async Task<byte[]> _Decrypt(byte[] data)
    {
        _ValidateInputParameters();
        if (_isParallelDecrypt())
        {
            var result = await _ParallelDecrypt(data);
            return _RemovePadding(result);
        }

        return _RemovePadding(_NonParallelDecrypt(data));
    }

    private byte[] _DoDecrypt(
        byte[] block, byte[] gamma, byte[] prevEncryptedBlock, byte[]? prevDecryptedBlock = null
    )
    {
        if (!_isParallelDecrypt() && prevDecryptedBlock is null)
            throw new InvalidOperationException("prevDecryptedBlock are required for non parallel operations");

        switch (EncryptMode)
        {
            case EncryptMode.Ecb:
                return gamma;
            case EncryptMode.Cbc:
                return Helper.XorArrayOfBytes(gamma, prevEncryptedBlock);
            case EncryptMode.Pcbc:
                return Helper.XorArrayOfBytes(gamma, prevEncryptedBlock, prevDecryptedBlock!);
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

    private byte[] _BeforeEncrypt(byte[] block, uint blockNumber, byte[]? prevEncryptedBlock = null,
        byte[]? prevBlock = null)
    {
        if (!_isParallelEncrypt() && (prevEncryptedBlock is null || prevBlock is null))
            throw new InvalidOperationException(
                "prevEncryptedBlock and prevBlock are required for non parallel operations"
            );

        if (!_isParallelEncrypt() && block.Length != prevEncryptedBlock?.Length)
            throw new InvalidOperationException("block and prevEncryptedBlock have different length");

        var result = new byte[block.Length];
        switch (EncryptMode)
        {
            case EncryptMode.Ecb:
                return block;
            case EncryptMode.Cbc:
                result = Helper.XorArrayOfBytes(block, prevEncryptedBlock!);
                break;
            case EncryptMode.Pcbc:
                if (prevBlock!.Length != block.Length)
                    throw new InvalidOperationException("prevBlock and block have different length!");

                result = Helper.XorArrayOfBytes(block, prevEncryptedBlock!, prevBlock);
                break;

            case EncryptMode.Cfb or EncryptMode.Ofb:
                return prevEncryptedBlock!;

            case EncryptMode.Ctr or EncryptMode.RandomDelta:
                return _BeforeEncryptCtrAndRandomDelta(blockNumber);
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

    private byte[] _BeforeDecrypt(
        byte[] block,
        uint blockNumber,
        byte[]? prevDecryptedBlock = null,
        byte[]? prevGamma = null)
    {
        if (!_isParallelDecrypt() && (prevDecryptedBlock is null || prevGamma is null))
            throw new InvalidOperationException(
                "prevDecryptedBlock and prevGamma are required for non parallel operations"
            );

        switch (EncryptMode)
        {
            case EncryptMode.Ecb or EncryptMode.Cbc or EncryptMode.Pcbc:
                return SymmetricalAlgorithm.Decrypt(block);
            case EncryptMode.Cfb:
                return SymmetricalAlgorithm.Encrypt(prevDecryptedBlock!);
            case EncryptMode.Ofb:
                return SymmetricalAlgorithm.Encrypt(prevGamma!);
            case EncryptMode.Ctr or EncryptMode.RandomDelta:
                return _BeforeDecryptCtrAndRandomDelta(blockNumber);
            default:
                throw new NotImplementedException();
        }
    }

    private void _ValidateInputParameters()
    {
        if (InitializationVector is null && EncryptMode is not EncryptMode.Ecb)
            throw new InvalidOperationException("Initializer vector is null");

        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;
        if (EncryptMode is not EncryptMode.Ecb && initializationVector!.Length != blockSizeBytes)
            throw new InvalidOperationException(
                $"Length initialization vector should be equal length block {blockSizeBytes}"
            );

        if (EncryptMode == EncryptMode.Ctr)
        {
            _counter = new BigInteger(InitializationVector!, isUnsigned: true, isBigEndian: true);
            _delta = new BigInteger([1], isUnsigned: true, isBigEndian: true);
        }

        if (EncryptMode == EncryptMode.RandomDelta)
        {
            var halfBlockSizeBytes = blockSizeBytes / 2;
            var mask = (new BigInteger([1]) << (halfBlockSizeBytes + 1)) - 1; // last halfBlockSizeBytes 1
            _counter = new BigInteger(InitializationVector!, isUnsigned: true, isBigEndian: true);
            _delta = _counter & mask;

            if (_delta % 2 == 0)
                ++_delta;
        }
    }

    private byte[] _BeforeEncryptCtrAndRandomDelta(uint blockNumber) => _GetCurrentCounter(blockNumber);

    private byte[] _BeforeDecryptCtrAndRandomDelta(uint blockNumber)
    {
        var counterBytes = _GetCurrentCounter(blockNumber);
        return SymmetricalAlgorithm.Encrypt(counterBytes);
    }

    private void _ValidateEncryptDecryptCtrAndRandomDelta()
    {
        if (EncryptMode is not (EncryptMode.Ctr or EncryptMode.RandomDelta))
            throw new InvalidOperationException(
                $"Invalid use of method {nameof(_ValidateEncryptDecryptCtrAndRandomDelta)}"
            );

        if (_counter is null || _delta is null)
            throw new InvalidOperationException("The counter or delta is null for CTR/Random delta encryption mode");
    }

    private byte[] _NonParallelEncrypt(byte[] data)
    {
        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;
        var countBlocks = data.Length / blockSizeBytes;
        List<byte[]> encryptedData = [];

        var prevEncryptedBlock = InitializationVector!;
        var prevBlock = new byte[blockSizeBytes];

        for (var i = 0; i < countBlocks; ++i)
        {
            var block = _GetBlock(data, i);
            var encryptedBlocks = _EncryptStep(block, (uint)i, prevEncryptedBlock, prevBlock);
            encryptedData.Add(encryptedBlocks.AfterEncryptBlock);

            prevEncryptedBlock = encryptedBlocks.EncryptedBlock;
            prevBlock = block;
        }

        return encryptedData.SelectMany(x => x).ToArray();
    }

    private async Task<byte[]> _ParallelEncrypt(byte[] data)
    {
        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;
        var countBlocks = data.Length / blockSizeBytes;
        var encryptedDataTasks = new Task<byte[]>[countBlocks];

        for (var i = 0; i < countBlocks; ++i)
        {
            var block = _GetBlock(data, i);
            var index = (uint)i;
            encryptedDataTasks[i] = Task.Run(() => _EncryptStep(block, index).AfterEncryptBlock);
        }

        var encryptedData = await Task.WhenAll(encryptedDataTasks);
        return encryptedData.SelectMany(x => x).ToArray();
    }

    private byte[] _NonParallelDecrypt(byte[] data)
    {
        List<byte[]> decryptedData = [];
        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;
        var countBlocks = data.Length / blockSizeBytes;

        var prevDecryptedBlock = InitializationVector!;
        var prevGamma = InitializationVector!;
        var prevEncryptedBlock = encryptMode is EncryptMode.Pcbc ? new byte[blockSizeBytes] : InitializationVector!;
        for (var i = 0; i < countBlocks; ++i)
        {
            var block = _GetBlock(data, i);
            var decryptedBlocks = _DecryptStep(block, prevEncryptedBlock, (uint)i, prevDecryptedBlock, prevGamma);

            prevDecryptedBlock = decryptedBlocks.DecryptedBlock;
            prevEncryptedBlock = block;
            prevGamma = decryptedBlocks.Gamma;

            decryptedData.Add(decryptedBlocks.DecryptedBlock);
        }

        var decryptedDataArray = decryptedData.SelectMany(x => x).ToArray();
        return _RemovePadding(decryptedDataArray);
    }

    private async Task<byte[]> _ParallelDecrypt(byte[] data)
    {
        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;
        var countBlocks = data.Length / blockSizeBytes;
        var decryptedDataTasks = new Task<byte[]>[countBlocks];

        var prevEncryptedBlock = InitializationVector!;
        for (var i = 0; i < countBlocks; ++i)
        {
            var block = _GetBlock(data, i);
            var prevEncryptedBlockCopy = prevEncryptedBlock;
            prevEncryptedBlock = block;
            var index = (uint)i;
            decryptedDataTasks[i] = Task.Run(() => _DecryptStep(block, prevEncryptedBlockCopy, index).DecryptedBlock);
        }

        var decryptedData = await Task.WhenAll(decryptedDataTasks);
        var decryptedDataArray = decryptedData.SelectMany(x => x).ToArray();
        return _RemovePadding(decryptedDataArray);
    }

    private EncryptBlocks _EncryptStep(byte[] block, uint blockNumber, byte[]? prevEncryptedBlock = null,
        byte[]? prevBlock = null)
    {
        var gamma = _BeforeEncrypt(block, blockNumber, prevEncryptedBlock, prevBlock);
        var encryptedBlock = SymmetricalAlgorithm.Encrypt(gamma);

        var afterEncrypt = _AfterEncrypt(encryptedBlock, block);

        return new EncryptBlocks(encryptedBlock: encryptedBlock, afterEncryptBlock: afterEncrypt);
    }

    private DecryptBlocks _DecryptStep(
        byte[] block,
        byte[] prevEncryptedBlock,
        uint blockNumber,
        byte[]? prevDecryptedBlock = null,
        byte[]? prevGamma = null
    )
    {
        var gamma = _BeforeDecrypt(block, blockNumber, prevDecryptedBlock, prevGamma);
        var decryptedBlock = _DoDecrypt(block, gamma, prevEncryptedBlock, prevDecryptedBlock);

        return new DecryptBlocks(gamma: gamma, decryptedBlock: decryptedBlock);
    }

    private bool _isParallelEncrypt() =>
        EncryptMode is EncryptMode.Ecb or EncryptMode.Ctr or EncryptMode.RandomDelta;

    private bool _isParallelDecrypt() =>
        EncryptMode is EncryptMode.Ecb or EncryptMode.Cbc or EncryptMode.Ctr or EncryptMode.RandomDelta;

    private byte[] _GetCurrentCounter(uint blockNumber)
    {
        _ValidateEncryptDecryptCtrAndRandomDelta();

        var currentCounter = _counter! + blockNumber * _delta!;
        var counterBytes = currentCounter.Value.ToByteArray(isUnsigned: true, isBigEndian: true);
        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;
        Array.Resize(ref counterBytes, blockSizeBytes);

        return counterBytes;
    }

    private byte[] _GetBlock(byte[] data, int blockNumber)
    {
        var blockSizeBytes = SymmetricalAlgorithm.BlockSizeBytes;
        var offset = blockNumber * blockSizeBytes;
        var endBlock = offset + blockSizeBytes;
        return data[offset..endBlock];
    }
}