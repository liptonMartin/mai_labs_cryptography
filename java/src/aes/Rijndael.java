package aes;

import common.Helper;
import interfaces.SymmetricalAlgorithm;
import models.Size;

import java.util.List;


public class Rijndael implements SymmetricalAlgorithm {
    private final GaloisField GF = new GaloisField();

    private Size blockSize = Size.S128;
    private Size keySize = Size.S128;

    private final int roundsCount;

    private byte[][] SBox;
    private byte[][] InverseSBox;

    public Rijndael(Size blockSize, Size keySize) {
        this.blockSize = blockSize;
        this.keySize = keySize;

        this.roundsCount = defineRoundsCount();

        this.SBox = generateSBox();
        this.InverseSBox = generateInverseSBox();
    }

    @Override
    public List<Byte> Encrypt(List<Byte> block) {
        return List.of();
    }

    @Override
    public List<Byte> Decrypt(List<Byte> block) {
        return List.of();
    }

    private void subBytes() {
    }

    private byte[][] generateSBox() {
        var result = new byte[16][16];

        for (int row = 0; row < 16; ++row) {
            for (int column = 0; column < 16; ++column) {
                var number = getNumberFromRowAndColumn(row, column);
                var inverseNumberByte = GF.inverseElement(number);
                var inverseNumber = inverseNumberByte & 0xFF;
                result[row][column] = (byte) (inverseNumber
                        ^ (Helper.cycleRotateLeft(inverseNumber, 1, Byte.SIZE))
                        ^ (Helper.cycleRotateLeft(inverseNumber, 2, Byte.SIZE))
                        ^ (Helper.cycleRotateLeft(inverseNumber, 3, Byte.SIZE))
                        ^ (Helper.cycleRotateLeft(inverseNumber, 4, Byte.SIZE))
                        ^ 0x63
                );
            }
        }

        return result;
    }

    private byte[][] generateInverseSBox() {
        var result = new byte[16][16];

        for (int row = 0; row < 16; ++row) {
            for (int column = 0; column < 16; ++column) {
                var numberByte = getNumberFromRowAndColumn(row, column);
                var number = numberByte & 0xFF;
                var inverseAffine = (byte) ((Helper.cycleRotateLeft(number, 1, Byte.SIZE))
                        ^ (Helper.cycleRotateLeft(number, 3, Byte.SIZE))
                        ^ (Helper.cycleRotateLeft(number, 6, Byte.SIZE))
                        ^ 0x5
                );
                result[row][column] = GF.inverseElement(inverseAffine);
            }
        }

        return result;
    }

    private byte getNumberFromRowAndColumn(int row, int column) {
        var rowHex = Integer.toHexString(row);
        var columnHex = Integer.toHexString(column);
        var numberHex = rowHex + columnHex;
        var numberInt = Integer.parseInt(numberHex, 16);
        return (byte) numberInt;
    }

    private int defineRoundsCount() {
        return switch (this.keySize) {
            case Size.S128 -> 10;
            case Size.S192 -> 12;
            case Size.S256 -> 14;
        };
    }
}
