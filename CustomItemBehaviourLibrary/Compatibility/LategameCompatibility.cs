namespace CustomItemBehaviourLibrary.Compatibility
{
    internal static class LategameCompatibility
    {
        public static bool Enabled =>
            BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.malco.lethalcompany.moreshipupgrades");
    }
}
