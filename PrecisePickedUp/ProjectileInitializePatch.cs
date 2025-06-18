using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace PrecisePickedUp;

public static class ProjectileInitializePatch {
	public static void PosFix(Entity __instance) {
		if (__instance.HasBehavior<EntityProjectileBehavior>()) {
			return;
		}

		__instance.AddBehavior(new EntityProjectileBehavior(__instance));
	}
}