package aes;

import common.Helper;
import models.Size;

import java.util.Arrays;
import java.util.HashMap;
import java.util.Map;

public class RijndaelOperations {
    private final GaloisField GF;
    private final Size blockSize;

    private final byte[][] SBox;
    private final byte[][] InverseSBox;
    private final Map<Size, int[]> shifts;


    public RijndaelOperations(GaloisField GF, Size blockSize) {
        this.GF = GF;
        this.blockSize = blockSize;

        this.SBox = generateSBox();
        this.InverseSBox = generateInverseSBox();
        this.shifts = generateShifts();
    }

    public void mixColumns(byte[][] state) {
        int m = state[0].length;
        for (int column = 0; column < m; ++column) {
            byte a0 = state[0][column], a1 = state[1][column];
            byte a2 = state[2][column], a3 = state[3][column];

            state[0][column] = this.GF.add(this.GF.multiply(a0, (byte) 2), this.GF.multiply(a1, (byte) 3), a2, a3);
            state[1][column] = this.GF.add(a0, this.GF.multiply(a1, (byte) 2), this.GF.multiply(a2, (byte) 3), a3);
            state[2][column] = this.GF.add(a0, a1, this.GF.multiply(a2, (byte) 2), this.GF.multiply(a3, (byte) 3));
            state[3][column] = this.GF.add(this.GF.multiply(a0, (byte) 3), a1, a2, this.GF.multiply(a3, (byte) 2));
        }
    }

    public void subBytes(byte[][] state) {
        subBytes(state, false);
    }

    public void subWord(byte[] word) {
        var n = word.length;
        for (int i = 0; i < n; ++i) {
            var number = word[i];
            var row = (number & 0xFF) >> 4;
            var column = (number & 0xFF) & 0x0F;

            var value = SBox[row][column];
            word[i] = value;
        }
    }

    public void inverseSubBytes(byte[][] state) {
        subBytes(state, true);
    }

    public void addRoundKey(byte[][] state, byte[][] roundKey) {
        int n = state.length;
        int m = state[0].length;

        for (int i = 0; i < n; ++i) {
            for (int j = 0; j < m; ++j) {
                state[i][j] ^= roundKey[i][j];
            }
        }
    }

    public void shiftRows(byte[][] state) {
        shiftRows(state, false);
    }

    public void inverseShiftRows(byte[][] state) {
        shiftRows(state, true);
    }

    private void shiftRows(byte[][] state, boolean inverse) {
        int n = 4;
        var countShifts = this.shifts.get(this.blockSize);
        for (int row = 1; row < n; ++row) {
            var countShiftsRow = countShifts[row];

            if (inverse) {
                cycleShiftRight(state[row], countShiftsRow);
            } else {
                cycleShiftLeft(state[row], countShiftsRow);
            }
        }
    }

    public void inverseMixColumns(byte[][] state) {
        int m = state[0].length;
        for (int column = 0; column < m; ++column) {
            byte a0 = state[0][column], a1 = state[1][column];
            byte a2 = state[2][column], a3 = state[3][column];

            state[0][column] = GF.add(
                    GF.multiply(a0, (byte) 0x0E), GF.multiply(a1, (byte) 0x0B), GF.multiply(a2, (byte) 0x0D), GF.multiply(a3, (byte) 0x09)
            );
            state[1][column] = GF.add(
                    GF.multiply(a0, (byte) 0x09), GF.multiply(a1, (byte) 0x0E), GF.multiply(a2, (byte) 0x0B), GF.multiply(a3, (byte) 0x0D)
            );
            state[2][column] = GF.add(
                    GF.multiply(a0, (byte) 0x0D), GF.multiply(a1, (byte) 0x09), GF.multiply(a2, (byte) 0x0E), GF.multiply(a3, (byte) 0x0B)
            );
            state[3][column] = GF.add(
                    GF.multiply(a0, (byte) 0x0B), GF.multiply(a1, (byte) 0x0D), GF.multiply(a2, (byte) 0x09), GF.multiply(a3, (byte) 0x0E)
            );
        }
    }


    private void subBytes(byte[][] state, boolean inverse) {
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

    private void cycleShiftLeft(byte[] stateRow, int countShiftsRow) {
        var countColumns = stateRow.length;

        var firstPart = Arrays.copyOfRange(stateRow, 0, countShiftsRow);
        var secondPart = Arrays.copyOfRange(stateRow, countShiftsRow, countColumns);

        System.arraycopy(secondPart, 0, stateRow, 0, countColumns - countShiftsRow);
        System.arraycopy(firstPart, 0, stateRow, countColumns - countShiftsRow, countShiftsRow);
    }

    private void cycleShiftRight(byte[] stateRow, int countShiftsRow) {
        var countColumns = stateRow.length;

        var firstPart = Arrays.copyOfRange(stateRow, 0, countColumns - countShiftsRow);
        var secondPart = Arrays.copyOfRange(stateRow, countColumns - countShiftsRow, countColumns);

        System.arraycopy(secondPart, 0, stateRow, 0, secondPart.length);
        System.arraycopy(firstPart, 0, stateRow, countShiftsRow, firstPart.length);
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

}
