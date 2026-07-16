using UnityEngine;

public class PlayerHideState : MonoBehaviour
{
    public static bool IsHidden { get; private set; } = false;

    public static void SetHidden(bool hidden)
    {
        IsHidden = hidden;
        Debug.Log("Player hidden state: " + IsHidden);
    }
}