using System;
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

		if (supplier is ClientMain clientMain) {
			var api = clientMain.Api;
			if (clientMain.MouseStateRaw.Left) {
				return;
			}
		}
		var oldFilter = efilter;
		efilter = e => e is EntityItem or EntityProjectile { NonCollectible: false } ||
			PrecisePickedUpModSystem.EnableOverhaulCompat && OverhaulCompat.RayTraceForSelection(e) ||
			oldFilter(e);
	}
}