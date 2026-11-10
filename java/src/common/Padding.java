package common;

import models.PaddingMode;

import java.security.SecureRandom;
import java.util.Arrays;

public class Padding {
    private static final SecureRandom RANDOM = new SecureRandom();

    public static byte[] addPadding(byte[] data, int blockSizeBytes, PaddingMode paddingMode) {
        if (data == null)
            throw new IllegalArgumentException("Data cannot be null");

        if (blockSizeBytes <= 0 || blockSizeBytes > 255)
            throw new IllegalArgumentException("Invalid block size");

        if (paddingMode == null)
            throw new IllegalArgumentException("Padding mode cannot be null");

        var countMissingBytes = blockSizeBytes - (data.length % blockSizeBytes);

        if (paddingMode == PaddingMode.ZEROS && countMissingBytes == blockSizeBytes)
            return data.clone();

        var result = Arrays.copyOf(data, data.length + countMissingBytes);

        switch (paddingMode) {
            case ZEROS:
                break;

            case ANSI_X923:
                result[result.length - 1] = (byte) countMissingBytes;
                break;

            case PKCS7:
                Arrays.fill(result, data.length, result.length, (byte) countMissingBytes);
                break;

            case ISO10126:
                for (int i = data.length; i < result.length - 1; ++i) {
                    result[i] = (byte) RANDOM.nextInt(256);
                }

                result[result.length - 1] = (byte) countMissingBytes;
                break;

            default:
                throw new IllegalArgumentException("Unknown padding mode");
        }

        return result;
    }

    public static byte[] removePadding(byte[] data, int blockSizeBytes, PaddingMode paddingMode) {
        if (data == null)
            throw new IllegalArgumentException("Data cannot be null");

        if (blockSizeBytes <= 0 || blockSizeBytes > 255)
            throw new IllegalArgumentException("Invalid block size");

        if (paddingMode == null)
            throw new IllegalArgumentException("Padding mode cannot be null");

        if (data.length == 0 || data.length % blockSizeBytes != 0)
            throw new IllegalArgumentException("Invalid padded data length");

        switch (paddingMode) {
            case ZEROS:
                var end = data.length;

                while (end > 0 && data[end - 1] == 0)
                    --end;

                return Arrays.copyOf(data, end);

            case ANSI_X923:
            case PKCS7:
            case ISO10126:
                var countToDelete = data[data.length - 1] & 0xFF;

                if (countToDelete < 1 || countToDelete > blockSizeBytes)
                    throw new IllegalArgumentException("Invalid padding length");

                var paddingStart = data.length - countToDelete;

                if (paddingMode == PaddingMode.PKCS7) {
                    for (int i = paddingStart; i < data.length; ++i) {
                        if ((data[i] & 0xFF) != countToDelete)
                            throw new IllegalArgumentException("Invalid PKCS7 padding");
                    }
                }

                if (paddingMode == PaddingMode.ANSI_X923) {
                    for (int i = paddingStart; i < data.length - 1; ++i) {
                        if (data[i] != 0)
                            throw new IllegalArgumentException("Invalid ANSI X.923 padding");
                    }
                }

                return Arrays.copyOf(data, paddingStart);

            default:
                throw new IllegalArgumentException("Unknown padding mode");
        }
    }
}
