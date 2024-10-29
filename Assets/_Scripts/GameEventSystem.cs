using UnityEngine;
using System;
using System.Collections.Generic;

// Core event system implementation
public static class GameEventSystem
{
    private static readonly Dictionary<string, HashSet<Action<object>>> eventHandlers =
        new Dictionary<string, HashSet<Action<object>>>();

    private static readonly Dictionary<string, object> eventCache =
        new Dictionary<string, object>();

    private static readonly HashSet<string> persistentEvents =
        new HashSet<string>();

    public static void Subscribe(string eventName, Action<object> handler, bool receiveCachedEvent = false)
    {
        if (!eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName] = new HashSet<Action<object>>();
        }

        eventHandlers[eventName].Add(handler);

        // If requested, immediately invoke handler with cached event data
        if (receiveCachedEvent && eventCache.ContainsKey(eventName))
        {
            handler.Invoke(eventCache[eventName]);
        }
    }

    public static void Unsubscribe(string eventName, Action<object> handler)
    {
        if (eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName].Remove(handler);

            // Cleanup if no handlers remain
            if (eventHandlers[eventName].Count == 0 && !persistentEvents.Contains(eventName))
            {
                eventHandlers.Remove(eventName);
                eventCache.Remove(eventName);
            }
        }
    }

    public static void RaiseEvent(string eventName, object data = null, bool cache = false)
    {
        // Cache event data if requested
        if (cache)
        {
            eventCache[eventName] = data;
        }

        // If no handlers, just cache if requested and return
        if (!eventHandlers.ContainsKey(eventName)) return;

        // Create a copy of handlers to avoid modification during iteration
        var handlers = new HashSet<Action<object>>(eventHandlers[eventName]);

        foreach (var handler in handlers)
        {
            try
            {
                handler.Invoke(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in event handler for {eventName}: {e}");
            }
        }
    }

    public static void SetPersistent(string eventName, bool persistent = true)
    {
        if (persistent)
        {
            persistentEvents.Add(eventName);
        }
        else
        {
            persistentEvents.Remove(eventName);
        }
    }

    public static void ClearEventCache(string eventName = null)
    {
        if (eventName != null)
        {
            eventCache.Remove(eventName);
        }
        else
        {
            eventCache.Clear();
        }
    }

    public static void ClearAllEvents()
    {
        eventHandlers.Clear();
        eventCache.Clear();
        persistentEvents.Clear();
    }
}

// Event constants and data classes
public static class GameEvents
{
    // Player State Events
    public const string PlayerSpawned = "PlayerSpawned";
    public const string PlayerDied = "PlayerDied";
    public const string PlayerHealthChanged = "PlayerHealthChanged";
    public const string PlayerStaminaChanged = "PlayerStaminaChanged";

    // Combat Events
    public const string CombatStarted = "CombatStarted";
    public const string CombatEnded = "CombatEnded";
    public const string WeaponEquipped = "WeaponEquipped";
    public const string ComboStarted = "ComboStarted";
    public const string ComboProgressed = "ComboProgressed";
    public const string ComboEnded = "ComboEnded";
    public const string EnemyDamaged = "EnemyDamaged";
    public const string EnemyKilled = "EnemyKilled";

    // Movement Events
    public const string DashStarted = "DashStarted";
    public const string DashEnded = "DashEnded";
    public const string PlayerEnteredRegion = "PlayerEnteredRegion";
    public const string PlayerExitedRegion = "PlayerExitedRegion";

    // Game State Events
    public const string GamePaused = "GamePaused";
    public const string GameResumed = "GameResumed";
    public const string GameSaved = "GameSaved";
    public const string GameLoaded = "GameLoaded";
}