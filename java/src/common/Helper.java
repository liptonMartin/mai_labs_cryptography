package common;

public class Helper {
    public static int cycleRotateLeft(int number, int distance, int k) {
        return (number << distance) | (number >> (k - distance));
    }
}
