using MAPITesting.Buildings;
using MAPITesting.Utils;
using MelonLoader;
using S1API;
using S1API.Entities;
using S1API.Entities.NPCs.Northtown;
using S1API.Internal.Utils;
using S1API.Quests;
using UnityEngine;
using System.Collections;

[assembly: MelonInfo(typeof(MAPITesting.Core), Constants.MOD_NAME, Constants.MOD_VERSION, Constants.MOD_AUTHOR)]
[assembly: MelonGame(Constants.Game.GAME_STUDIO, Constants.Game.GAME_NAME)]

namespace MAPITesting
{
    public class Core : MelonMod
    {
        public static Core? Instance { get; private set; }
        private static bool _dispensarySpawned = false;

        public override void OnInitializeMelon()
        {
            Instance = this;
            MelonLogger.Msg("MAPITesting mod initialized");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            base.OnSceneWasInitialized(buildIndex, sceneName);
            
            if (sceneName == "Main" && !_dispensarySpawned)
            {
                MelonLogger.Msg($"Main scene initialized (buildIndex: {buildIndex}). Spawning dispensary...");
                
                DispensarySpawner.SpawnDispensaryBuilding();
                _dispensarySpawned = true;
                
                MelonLogger.Msg("Spawned marijuana dispensary building at docks location");
                
                MelonLogger.Msg("Populating dispensary storage with products...");
                GreenLabDispensary.PopulateAllStorage();
            }
        }

        public override void OnApplicationQuit()
        {
            Instance = null;
            _dispensarySpawned = false;
        }
    }
}