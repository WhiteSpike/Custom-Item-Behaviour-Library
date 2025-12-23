using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomItemBehaviourLibrary.Configuration
{
	internal class ContainerConfiguration
	{
		const string SECTION_NAME = "Containers";
		internal ConfigEntry<string> BlacklistedStorableItems;
		const string BLACKLISTED_ITEMS_KEY = "Blacklisted Items";
		const string BLACKLISTED_ITEMS_DEFAULT = "";
		const string BLACKLISTED_ITEMS_DESCRIPTION = "List of item names (separated by a comma and the name can be either the internal developer name or the name displayed in the scan node) that you do not wish to be able to store into any of the containers.\nThese are usually items that do not work well when deposited and cause issues.";
		internal const char BLACKLISTED_ITEMS_DELIMITER = ',';
		public ContainerConfiguration(ConfigFile config) 
		{
			BlacklistedStorableItems = config.Bind(SECTION_NAME, BLACKLISTED_ITEMS_KEY, BLACKLISTED_ITEMS_DEFAULT, BLACKLISTED_ITEMS_DESCRIPTION);
		}
	}
}
