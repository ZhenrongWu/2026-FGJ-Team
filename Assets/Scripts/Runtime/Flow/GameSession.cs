using System;
using System.Collections.Generic;
using FGJ.LiarDice;
using UnityEngine;

namespace FGJ.Flow
{
    public static class SceneNames
    {
        public const string Init = "Init";
        public const string MainMenu = "MainMenu";
        public const string Exploration = "Exploration";
        public const string Gameplay = "Gameplay";
    }

    public static class SceneFlow
    {
        public static readonly string[] StartSceneCandidates = { SceneNames.MainMenu, SceneNames.Exploration };

        public static bool CanLoad(string sceneName) => Application.CanStreamedLevelBeLoaded(sceneName);

        public static string PickFirstLoadable(IEnumerable<string> candidates, Func<string, bool> canLoad)
        {
            foreach (var candidate in candidates)
            {
                if (canLoad(candidate))
                    return candidate;
            }
            return null;
        }

        public static string StartScene(Func<string, bool> canLoad = null)
        {
            return PickFirstLoadable(StartSceneCandidates, canLoad ?? CanLoad) ?? SceneNames.Exploration;
        }
    }

    public static class GameSession
    {
        private static readonly HashSet<string> ClearedRooms = new HashSet<string>();

        public static string CurrentRoomId { get; private set; }
        public static LiarDiceConfig CurrentGameplayConfig { get; private set; }
        public static string ReturnRoomId { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            ClearedRooms.Clear();
            CurrentRoomId = null;
            CurrentGameplayConfig = null;
            ReturnRoomId = null;
        }

        public static void EnterRoom(string roomId, LiarDiceConfig gameplayConfig = null)
        {
            CurrentRoomId = roomId;
            CurrentGameplayConfig = gameplayConfig;
            ReturnRoomId = null;
        }

        public static void CompleteRoom(bool playerWon)
        {
            if (!playerWon)
            {
                Reset();
                return;
            }

            if (CurrentRoomId != null)
            {
                ClearedRooms.Add(CurrentRoomId);
                ReturnRoomId = CurrentRoomId;
            }
            CurrentRoomId = null;
            CurrentGameplayConfig = null;
        }

        public static bool IsCleared(string roomId) => roomId != null && ClearedRooms.Contains(roomId);

        public static string ConsumeReturnRoom()
        {
            var roomId = ReturnRoomId;
            ReturnRoomId = null;
            return roomId;
        }
    }
}
