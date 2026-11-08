package models;

public enum Size {
    S128(128),
    S192(192),
    S256(256);

    private final int value;

    Size(int value) {
        this.value = value;
    }

    public int getValue() {
        return value;
    }
}