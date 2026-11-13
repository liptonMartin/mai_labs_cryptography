package console;

import java.util.LinkedHashMap;
import java.util.Map;
import java.util.Scanner;
import java.util.Set;

public class ConsoleApp {

    @FunctionalInterface
    private interface LabAction {
        void run() throws Exception;
    }

    private record LabActionEntry(String description, LabAction run) {
    }

    private record LabEntry(String title, Map<String, LabActionEntry> actions) {
    }

    private static final Scanner SCANNER = new Scanner(System.in);
    private static final Map<String, LabEntry> LABS = buildLabs();

    private static Map<String, LabEntry> buildLabs() {
        Map<String, LabEntry> labs = new LinkedHashMap<>();

        Map<String, LabActionEntry> lab1Actions = new LinkedHashMap<>();
        lab1Actions.put("1", new LabActionEntry("AES Encrypt/Decrypt", AesExamples::aesMessage));
        lab1Actions.put("2", new LabActionEntry("AES Encrypt/Decrypt file", AesExamples::aesFiles));

        labs.put("1", new LabEntry("Lab 3. AES", lab1Actions));

        return labs;
    }

    public static void main(String[] args) {
        while (true) {
            printLabsMenu();
            String labChoice = readChoice(LABS.keySet());
            if (labChoice == null) return;

            LabEntry lab = LABS.get(labChoice);

            while (true) {
                printActionsMenu(lab.title(), lab.actions());
                String actionChoice = readChoice(lab.actions().keySet());
                if (actionChoice == null) break;

                System.out.println();
                System.out.println("─".repeat(60));

                try {
                    lab.actions().get(actionChoice).run().run();
                } catch (Exception ex) {
                    System.out.println("\u001B[31mError: " + ex.getMessage() + "\u001B[0m");
                }

                System.out.println();
                System.out.println("─".repeat(60));
                System.out.println();
                System.out.println("Press Enter to continue...");
                SCANNER.nextLine();
            }
        }
    }

    private static void printLabsMenu() {
        clearScreen();
        System.out.println("Choose lab number:");
        for (Map.Entry<String, LabEntry> e : LABS.entrySet()) {
            System.out.println("  " + e.getKey() + ". " + e.getValue().title());
        }
        System.out.println("  0. Exit");
        System.out.print("> ");
    }

    private static void printActionsMenu(String labTitle, Map<String, LabActionEntry> actions) {
        clearScreen();
        System.out.println("Lab: " + labTitle);
        System.out.println("Choose action:");
        for (Map.Entry<String, LabActionEntry> e : actions.entrySet()) {
            System.out.println("  " + e.getKey() + ". " + e.getValue().description());
        }
        System.out.println("  0. Back");
        System.out.print("> ");
    }

    private static String readChoice(Set<String> validKeys) {
        while (true) {
            String input = SCANNER.hasNextLine() ? SCANNER.nextLine().trim() : null;

            if ("0".equals(input)) return null;
            if (input != null && validKeys.contains(input)) return input;

            System.out.print("\u001B[33mInvalid input, please try again: \u001B[0m");
        }
    }

    private static void clearScreen() {
        System.out.print("\u001B[H\u001B[2J");
        System.out.flush();
    }
}