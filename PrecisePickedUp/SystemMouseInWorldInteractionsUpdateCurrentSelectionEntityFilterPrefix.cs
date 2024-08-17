using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace PrecisePickedUp;

public static class SystemMouseInWorldInteractionsUpdateCurrentSelectionEntityFilterPrefix {
	public static bool Prefix(ref bool __result, Entity e) { return !(__result = e is EntityItem); }
}