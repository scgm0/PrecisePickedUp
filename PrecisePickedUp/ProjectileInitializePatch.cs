using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace PrecisePickedUp;

public static class ProjectileInitializePatch {
	public static void PosFix(Entity __instance) {
		if (__instance.HasBehavior<EntityProjectileBehavior>()) {
			return;
		}

		if (__instance is EntityProjectile projectile) {
			var stack = projectile.ProjectileStack!;
			projectile.Api.Logger.Notification($"ProjectileStack: {stack} {stack.Item} {projectile.NonCollectible}");
			if (stack.Item is null) {
				ref var item = ref UnsafeAccessorExtensions.GetItemStack_item(stack);
				item = projectile.Api.World.GetItem(stack.Id);
			}

			if (projectile.NonCollectible) {
				return;
			}
		}

		if (PrecisePickedUpModSystem.EnableOverhaulCompat && OverhaulCompat.NotCollect(__instance)) {
			return;
		}

		__instance.AddBehavior(new EntityProjectileBehavior(__instance));
	}
}