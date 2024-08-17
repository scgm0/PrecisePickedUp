using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace PrecisePickedUp;

public static class EntityItemCanCollectPrefix {
	public static bool Prefix(ref bool __result, Entity byEntity) {
		if (PrecisePickedUpModSystem.Config.CanAutoCollect || byEntity is not EntityPlayer) return true;
		__result = false;
		return false;
	}
}