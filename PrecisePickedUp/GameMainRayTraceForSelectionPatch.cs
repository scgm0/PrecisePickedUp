using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace PrecisePickedUp;

public class GameMainRayTraceForSelectionPatch {
	public static ICoreAPI api;

	[HarmonyPriority(Priority.First)]
	public static void PreFix(ref EntityFilter efilter) {
		if (!efilter.Method.Name.StartsWith("<UpdateCurrentSelection>")) return;
		var oldFilter = efilter;
		efilter = e => e is EntityItem || oldFilter(e);
	}
}