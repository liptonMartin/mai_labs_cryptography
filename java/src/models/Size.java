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

    public static Size valueOf(int value) {
        return switch (value) {
            case 128 -> Size.S128;
            case 192 -> Size.S192;
            case 256 -> Size.S256;
            default -> throw new IllegalArgumentException(
                    String.format("Support only 128/192/256 bits. Current value %s", value)
            );
        };
    }
}