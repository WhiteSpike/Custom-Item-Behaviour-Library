using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using CustomItemBehaviourLibrary.Misc;
using MoreShipUpgrades.Patches.Enemies;
using CustomItemBehaviourLibrary.Patches;
namespace CustomItemBehaviourLibrary
{
    [BepInPlugin(Metadata.GUID,Metadata.NAME,Metadata.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static readonly Harmony harmony = new(Metadata.GUID);
        internal static readonly ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(Metadata.NAME);

        void Awake()
        {
            // netcode patching stuff
            IEnumerable<Type> types;
            try
            {
                types = Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null);
            }
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
            PatchMainVersion();
            PatchBetaVersion();
            mls.LogInfo($"{Metadata.NAME} {Metadata.VERSION} has been loaded successfully.");
        }
        internal static void PatchMainVersion()
        {
            harmony.PatchAll(typeof(BaboonBirdAIPatcher));
            harmony.PatchAll(typeof(SpringManAIPatcher));
            harmony.PatchAll(typeof(HUDManagerPatcher));
            harmony.PatchAll(typeof(PlayerControllerBPatcher));
            mls.LogInfo("Patched relevant components for correct item behaviours...");
        }
        internal static void PatchBetaVersion()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                if (assemblies[i].GetName().Name != "Assembly-CSharp") continue;
                Type[] types = assemblies[i].GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    if (types[j].Name == "BeltBagItem")
                    {
                        harmony.PatchAll(typeof(BeltBagItemPatcher));
                        mls.LogInfo("Patched belt bag for correct behaviour with containers.");
                        return;
                    }
                }
                return;
            }
        }
    }   
}
