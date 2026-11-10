package aes;

import interfaces.EncryptionRound;

public class SpNet implements EncryptionRound {
    private final RijndaelOperations RO;

    public SpNet(RijndaelOperations RO) {
        this.RO = RO;
    }

    @Override
    public byte[][] DoEncrypt(byte[][] state, byte[][] roundKey) {
        RO.subBytes(state);
        RO.shiftRows(state);
        RO.mixColumns(state);
        RO.addRoundKey(state, roundKey);
        return state;
    }

    @Override
    public byte[][] DoDecrypt(byte[][] state, byte[][] roundKey) {
        RO.inverseShiftRows(state);
        RO.inverseSubBytes(state);
        RO.addRoundKey(state, roundKey);
        RO.inverseMixColumns(state);
        return state;
    }

}
