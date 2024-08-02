using System;
using System.Collections.Generic;
using System.Text;

namespace CustomItemBehaviourLibrary.Compatibility
{
    internal static class LategameCompatibility
    {
        public static bool Enabled =>
            BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.malco.lethalcompany.moreshipupgrades");
    }
}
