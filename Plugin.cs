using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace CardLoaderMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "cyantist.inscryption.extendedmap";
        private const string PluginName = "Extended Map";
        private const string PluginVersion = "1.1.0.0";

        internal static ConfigEntry<int> configLength;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginName}!");

            configLength = Config.Bind("General",
                                         "MapTriplets",
                                         4,
                                         "The number of map node triplets. (value*3+1=number of nodes)");
            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(PaperGameMap), "TryInitializeMapData")]
    public class PaperGameMap_TryInitializeMapData
    {
        public static bool Prefix(PaperGameMap __instance)
        {
            if (RunState.Run.map == null)
            {
                var trav = Traverse.Create(__instance);
                RunState.Run.map = MapGenerator.GenerateMap(RunState.CurrentMapRegion, 3, Plugin.configLength.Value*3+1,  trav.Field("PredefinedNodes").GetValue<PredefinedNodes>(),  trav.Field("PredefinedScenery").GetValue<PredefinedScenery>());
                RunState.Run.currentNodeId = RunState.Run.map.RootNode.id;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(RunState), "NextRegion")]
    public class RunState_NextRegion
    {
        public static bool Prefix(ref int ___regionTier, ref int ___regionIndex, ref MapData ___map, ref int ___currentNodeId)
        {
            ___regionTier++;
            ___regionIndex = RegionProgression.GetRandomRegionIndexForTier(___regionTier);
            ___map = MapGenerator.GenerateMap(RunState.CurrentMapRegion, 3, Plugin.configLength.Value*3+1, null, null);
            ___currentNodeId = ___map.RootNode.id;
            return false;
        }
    }
}
