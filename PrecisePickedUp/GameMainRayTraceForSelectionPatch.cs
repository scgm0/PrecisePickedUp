using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace PrecisePickedUp;

public class GameMainRayTraceForSelectionPatch {

	[HarmonyPriority(Priority.First)]
	public static void PreFix(ref EntityFilter efilter) {
		if (efilter is null || !efilter.Method.Name.StartsWith("<UpdateCurrentSelection>")) {
			return;
		}

		var oldFilter = efilter;
		efilter = e => e is EntityItem or EntityProjectile { NonCollectible: false } ||
			PrecisePickedUpModSystem.EnableOverhaulCompat && OverhaulCompat.RayTraceForSelection(e) ||
			oldFilter(e);
	}
}