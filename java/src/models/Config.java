package models;

import aes.RijndaelConfig;
import interfaces.SymmetricalAlgorithm;

public sealed interface Config permits RijndaelConfig {
    byte[] getKey();

    SymmetricalAlgorithm createAlgorithm();
}
