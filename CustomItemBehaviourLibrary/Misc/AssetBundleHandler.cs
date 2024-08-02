using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CustomItemBehaviourLibrary.Misc
{
    public static class AssetBundleHandler
    {
        public static AudioClip[] GetAudioClipList(this AssetBundle bundle, string name, int length)
        {
            AudioClip[] array = new AudioClip[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = bundle.TryLoadAudioClipAsset($"{name} {i}");
            }
            return array;
        }
        /// <summary>
        /// Tries to load an asset from provided asset bundle through a given path into a AudioClip
        /// <para>
        /// If the asset requested does not exist in the bundle, it will be logged for easier tracking of what asset is missing from the bundle
        /// </para>
        /// </summary>
        /// <param name="bundle">The asset bundle we wish to gather the asset from</param>
        /// <param name="path">The path to the asset we wish to load</param>
        /// <returns>The asset's AudioClip if it's present in the asset bundle, otherwise null</returns>
        public static AudioClip TryLoadAudioClipAsset(this AssetBundle bundle, string path)
        {
            return bundle.LoadAsset<AudioClip>(path);
        }
    }
}
