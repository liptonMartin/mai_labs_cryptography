package aes;

import common.Helper;
import interfaces.SymmetricalAlgorithm;
import models.Size;

import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;


public class Rijndael implements SymmetricalAlgorithm {
    private final GaloisField GF = new GaloisField();

    private Size blockSize = Size.S128;
    private Size keySize = Size.S128;

    private final int roundsCount;

    private byte[][] SBox;
    private byte[][] InverseSBox;

    private final Map<Size, int[]> shifts;

    public Rijndael(Size blockSize, Size keySize) {
        this.blockSize = blockSize;
        this.keySize = keySize;

        this.roundsCount = defineRoundsCount();

        this.SBox = generateSBox();
        this.InverseSBox = generateInverseSBox();

        this.shifts = generateShifts();
    }

    @Override
    public List<Byte> Encrypt(List<Byte> block) {
        return List.of();
    }

    @Override
    public List<Byte> Decrypt(List<Byte> block) {
        return List.of();
    }

    private byte[][] subBytes(byte[][] state) {
        return subBytes(state, false);
    }

    private byte[][] inverseSubBytes(byte[][] state) {
        return subBytes(state, true);
    }

    private byte[][] shiftRows(byte[][] state) {
        int n = 4;
        var countShifts = this.shifts.get(this.blockSize);
        for (int row = 1; row < n; ++row) {
            var countShiftsRow = countShifts[row];
            var countColumns = state[row].length;

            var firstPart = Arrays.copyOfRange(state[row], 0, countShiftsRow);
            var secondPart = Arrays.copyOfRange(state[row], countShiftsRow, countColumns);

            System.arraycopy(secondPart, 0, state[row], 0, countColumns - countShiftsRow);
            System.arraycopy(firstPart, 0, state[row], countColumns - countShiftsRow, countShiftsRow);
        }

        return state;
    }

    private byte[][] subBytes(byte[][] state, boolean inverse) {
        var n = state.length;
        var m = state[0].length;
        for (int i = 0; i < n; ++i) {
            for (int j = 0; j < m; ++j) {
                var number = state[i][j];
                var row = (number & 0xFF) >> 4;
                var column = (number & 0xFF) & 0x0F;

                var value = inverse ? InverseSBox[row][column] : SBox[row][column];
                state[i][j] = value;
            }
        }

        return state;
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

    private Map<Size, int[]> generateShifts() {
        var shifts = new HashMap<Size, int[]>();
        shifts.put(Size.S128, new int[]{0, 1, 2, 3});
        shifts.put(Size.S192, new int[]{0, 1, 2, 3});
        shifts.put(Size.S256, new int[]{0, 1, 3, 4});
        return shifts;
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
