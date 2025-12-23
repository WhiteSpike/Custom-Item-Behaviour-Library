using BepInEx.Configuration;

namespace CustomItemBehaviourLibrary.Configuration
{
	internal class LibraryConfiguration
	{
		internal ContainerConfiguration ContainerConfiguration;
		
		public LibraryConfiguration(ConfigFile config)
		{
			ContainerConfiguration = new(config);
		}
	}
}
