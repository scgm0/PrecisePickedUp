using Vintagestory.GameContent;

namespace PrecisePickedUp;

public static class EntityProjectileInitializePatch {
	public static void PosFix(EntityProjectile __instance) {
		if (__instance.HasBehavior<EntityProjectileBehavior>()) return;
		__instance.AddBehavior(new EntityProjectileBehavior(__instance));
	}
}