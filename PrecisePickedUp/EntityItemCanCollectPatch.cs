using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace PrecisePickedUp;

public static class EntityItemCanCollectPatch {
	public static bool PreFix(ref bool __result, Entity byEntity) {
		if (PrecisePickedUpModSystem.Config.CanAutoCollect || byEntity is not EntityPlayer) {
			return true;
		}

		return __result = false;
	}
}