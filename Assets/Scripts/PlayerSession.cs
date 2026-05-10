using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerSession
{
    public static PlayerData CurrentPlayer { get; private set; }

    public static bool IsLoggedIn => CurrentPlayer != null;

    public static void Set(PlayerData player)
    {
        CurrentPlayer = player;
    }

    public static void Clear()
    {
        CurrentPlayer = null;
    }
}
