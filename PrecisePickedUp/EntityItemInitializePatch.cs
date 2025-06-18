using Vintagestory.API.Common;

namespace PrecisePickedUp;

public static class EntityItemInitializePatch {
	public static void PosFix(EntityItem __instance) {
		if (__instance.HasBehavior<EntityItemBehavior>()) {
			return;
		}

		__instance.AddBehavior(new EntityItemBehavior(__instance));
	}
}