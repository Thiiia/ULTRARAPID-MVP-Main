using UnityEngine;

public static class FactorDataManagerScript
{
    // Save unlocked state for a factor
    public static void SetUnlocked(string factorName, bool unlocked)
    {
        PlayerPrefs.SetInt($"{factorName}_unlocked", unlocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Save full state for a factor
    public static void SetFull(string factorName, bool full)
    {
        PlayerPrefs.SetInt($"{factorName}_full", full ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Load unlocked state
    public static bool IsUnlocked(string factorName)
    {
        return PlayerPrefs.GetInt($"{factorName}_unlocked", 0) == 1;
    }

    // Load full state
    public static bool IsFull(string factorName)
    {
        return PlayerPrefs.GetInt($"{factorName}_full", 0) == 1;
    }

    // Clear all saved factor data
   public static void ResetAllFactorData()
{
    Debug.Log("Resetting factor progression...");

    //  Remove all saved factor progress
    PlayerPrefs.DeleteAll();

    // initial three factors (Root, 12, and 10)
    PlayerPrefs.SetInt("120_unlocked", 1); // Root Factor
    PlayerPrefs.SetInt("0_unlocked", 1);
    PlayerPrefs.SetInt("5_unlocked", 1);

    PlayerPrefs.Save();
}

}
