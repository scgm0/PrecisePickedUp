using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace PrecisePickedUp;

public static class ProjectileInitializePatch {
	public static void PosFix(Entity __instance) {
		if (__instance.HasBehavior<EntityProjectileBaseBehavior>()) {
			return;
		}

		if (__instance is EntityProjectileBase projectile) {
			var stack = projectile.ProjectileStack;
			if (stack is { Class: EnumItemClass.Item, Item: null }) {
				ref var item = ref UnsafeAccessorExtensions.GetItemStack_item(stack);
				item = projectile.Api.World.GetItem(stack.Id);
			}

			if (stack is { Class: EnumItemClass.Block, Block: null }) {
				ref var block = ref UnsafeAccessorExtensions.GetItemStack_block(stack);
				block = projectile.Api.World.GetBlock(stack.Id);
			}

			if (!projectile.Collectible) {
				return;
			}
		}

		if (PrecisePickedUpModSystem.EnableOverhaulCompat && OverhaulCompat.NotCollect(__instance)) {
			return;
		}

		__instance.AddBehavior(new EntityProjectileBaseBehavior(__instance));
	}
}