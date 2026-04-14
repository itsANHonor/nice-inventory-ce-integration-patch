using HarmonyLib;
using Verse;

namespace NiceInventoryTabCEIntegrationPatch;

public sealed class NiceInventoryTabCEIntegrationPatchMod : Mod
{
	private const string HarmonyId = "itsANHonor.NiceInventoryTabCEIntegrationPatch";

	public NiceInventoryTabCEIntegrationPatchMod(ModContentPack content) : base(content)
	{
		var harmony = new Harmony(HarmonyId);
		var infoWidget = AccessTools.TypeByName("NiceInventoryTab.InfoWidget");
		if (infoWidget == null)
		{
			Log.Error("[Nice Inventory Tab CE Integration Patch] NiceInventoryTab.InfoWidget not found. Is Nice Inventory Tab enabled?");
			return;
		}

		var checkExtraInfo = AccessTools.Method(infoWidget, "CheckExtraInfo", new[] { typeof(Thing) });
		if (checkExtraInfo == null)
		{
			Log.Error("[Nice Inventory Tab CE Integration Patch] InfoWidget.CheckExtraInfo(Thing) not found; Nice Inventory Tab may have updated.");
			return;
		}

		harmony.Patch(checkExtraInfo, prefix: new HarmonyMethod(typeof(InfoWidget_CheckExtraInfo_Prefix), nameof(InfoWidget_CheckExtraInfo_Prefix.Prefix)));
		Log.Message("[Nice Inventory Tab CE Integration Patch] Patched InfoWidget.CheckExtraInfo (null extraW guard).");
	}
}

/// <summary>
/// Nice Inventory Tab builds some InfoWidget instances (e.g. from CE's inventory tab) without calling
/// MakeWeaponInfo/MakeArmorInfo, so <c>extraW</c> is never created. CheckExtraInfo always touches extraW first → NRE.
/// Skip the method when extraW is missing.
/// </summary>
internal static class InfoWidget_CheckExtraInfo_Prefix
{
	public static bool Prefix(object __instance)
	{
		var extraW = Traverse.Create(__instance).Field("extraW").GetValue();
		return extraW != null;
	}
}
