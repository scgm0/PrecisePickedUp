using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace PrecisePickedUp;

public class GameMainRayTraceForSelectionPatch {

	[HarmonyPriority(Priority.First)]
	public static void PreFix(IWorldIntersectionSupplier supplier, ref EntityFilter efilter) {
		if (efilter is null || !efilter.Method.Name.StartsWith("<UpdateCurrentSelection>")) {
			return;
		}

		if (supplier is ClientMain { MouseStateRaw.Left: true }) {
			return;
		}

		var oldFilter = efilter;
		efilter = e => e is EntityItem or EntityProjectile { Collectible: true } ||
			PrecisePickedUpModSystem.EnableOverhaulCompat && OverhaulCompat.RayTraceForSelection(e) ||
			oldFilter(e);
	}
}