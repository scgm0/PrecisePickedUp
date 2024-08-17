using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace PrecisePickedUp;

public static class EntityGetNamePrefix {
	public static bool Prefix(Entity __instance, ref string __result) {
		if (__instance is not EntityItem item) return true;
		__result = $"{item.Itemstack.GetName()} (x{item.WatchedAttributes.GetInt("stackCount", item.Itemstack.StackSize)})";
		return false;
	}
}