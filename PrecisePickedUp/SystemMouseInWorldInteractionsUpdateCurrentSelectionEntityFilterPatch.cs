using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace PrecisePickedUp;

public static class SystemMouseInWorldInteractionsUpdateCurrentSelectionEntityFilterPatch {
	public static bool PreFix(ref bool __result, Entity e) { return !(__result = e is EntityItem); }
}