package console;

import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Scanner;

public final class OutputHelper {

    private static final Scanner SCANNER = new Scanner(System.in);

    private OutputHelper() {
    }

    public static byte[] readBytes(int expectedLength, byte[] defaultValue, String prompt) {
        while (true) {
            System.out.printf("%s (length %d)%n", prompt, expectedLength);
            if (defaultValue != null) {
                System.out.println("  default: " + formatBytes(defaultValue));
            }
            System.out.print("> ");
            String input = SCANNER.nextLine().trim();

            if (input.isEmpty()) {
                if (defaultValue != null && defaultValue.length == expectedLength) {
                    return defaultValue.clone();
                }
                System.out.println("No valid default, please enter a value.");
                continue;
            }

            try {
                byte[] parsed = parseBytes(input);
                if (parsed.length != expectedLength) {
                    System.out.printf("Expected %d bytes, got %d. Try again.%n",
                            expectedLength, parsed.length);
                    continue;
                }
                return parsed;
            } catch (NumberFormatException e) {
                System.out.println("Invalid input: " + e.getMessage());
            }
        }
    }

    private static byte[] parseBytes(String input) {
        String trimmed = input.trim();

        // hex: "01 23 45", "0x01,0x23"
        String hexCandidate = trimmed.replaceAll("(?i)0x", "").replaceAll("[\\s,]+", "");
        if (hexCandidate.matches("(?i)[0-9a-f]+") && hexCandidate.length() % 2 == 0) {
            byte[] result = new byte[hexCandidate.length() / 2];
            for (int i = 0; i < result.length; i++) {
                result[i] = (byte) Integer.parseInt(
                        hexCandidate.substring(i * 2, i * 2 + 2), 16);
            }
            return result;
        }

        // decimal: "1, 35, 69"
        String[] parts = trimmed.split("[,\\s]+");
        byte[] result = new byte[parts.length];
        for (int i = 0; i < parts.length; i++) {
            int value = Integer.parseInt(parts[i].trim());
            if (value < -128 || value > 255) {
                throw new NumberFormatException("byte out of range: " + value);
            }
            result[i] = (byte) value;
        }
        return result;
    }

    private static String formatBytes(byte[] bytes) {
        StringBuilder sb = new StringBuilder("[");
        for (int i = 0; i < bytes.length; i++) {
            if (i > 0) sb.append(", ");
            sb.append(bytes[i] & 0xFF);
        }
        return sb.append(']').toString();
    }

    public static String readString(String prompt, String defaultValue) {
        System.out.printf("%s [default: %s]%n", prompt, defaultValue);
        System.out.print("> ");
        String input = SCANNER.nextLine();
        return input.isEmpty() ? defaultValue : input;
    }

    public static <T extends Enum<T>> T readEnum(String prompt, T defaultValue) {
        T[] values = defaultValue.getDeclaringClass().getEnumConstants();
        while (true) {
            System.out.println(prompt + ":");
            for (int i = 0; i < values.length; i++) {
                System.out.printf("  %d. %s%s%n", i + 1, values[i],
                        values[i] == defaultValue ? " (default)" : "");
            }
            System.out.print("> ");
            String input = SCANNER.nextLine().trim();

            if (input.isEmpty()) return defaultValue;

            try {
                int index = Integer.parseInt(input) - 1;
                if (index >= 0 && index < values.length) return values[index];
            } catch (NumberFormatException ignored) { /* fall through to name match */ }

            for (T v : values) {
                if (v.name().equalsIgnoreCase(input)) return v;
            }
            System.out.println("Invalid choice, try again.");
        }
    }

    public static Path readFilePath(String prompt) {
        while (true) {
            System.out.println(prompt);
            System.out.print("> ");
            String input = SCANNER.nextLine().trim();

            if (input.isEmpty()) {
                System.out.println("Path cannot be empty.");
                continue;
            }
            try {
                return Paths.get(input);
            } catch (Exception e) {
                System.out.println("Invalid path: " + e.getMessage());
            }
        }
    }

    public static void outputByteArray(byte[] data, String label) {
        System.out.printf("%s (%d bytes):%n", label, data.length);
        System.out.print("  hex: ");
        for (byte b : data) System.out.printf("%02X ", b & 0xFF);
        System.out.println();
    }

    public static Byte readOptionalByte(String prompt) {
        return readOptionalByte(prompt, "default");
    }

    public static Byte readOptionalByte(String prompt, String defaultValue) {
        while (true) {
            System.out.println(prompt + " (empty = " + defaultValue + ")");
            System.out.print("> ");
            String input = SCANNER.nextLine().trim();

            if (input.isEmpty()) return null;

            try {
                int value;
                if (input.startsWith("0x") || input.startsWith("0X")) {
                    value = Integer.parseInt(input.substring(2), 16);
                } else {
                    value = Integer.parseInt(input);
                }
                if (value < -128 || value > 255) {
                    System.out.println("Value out of byte range [-128; 255].");
                    continue;
                }
                return (byte) value;
            } catch (NumberFormatException e) {
                System.out.println("Invalid byte: " + e.getMessage());
            }
        }
    }
}