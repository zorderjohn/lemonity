using UnityEditor;
using Lemonity.Core;

namespace Lemonity.Provider.Ultraleap.Editor
{
	[InitializeOnLoad]
	internal static class UltraleapEditorBootstrap
	{
		static UltraleapEditorBootstrap()
		{
			HandTracking.RegisterProvider(UltraLeapTracking.SubInstance);
		}
	}
}
