using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace PrecisePickedUp;

public static class EntityGetNamePatch {
	public static bool PreFix(Entity __instance, ref string __result, MethodBase __originalMethod) {
		if (PrecisePickedUpModSystem.EnableOverhaulCompat) {
			if (OverhaulCompat.EntityGetName(__instance, ref __result)) {
				return false;
			}
		}

		switch (__instance) {
			case EntityItem item:
				if (item.Slot.Itemstack == null) {
					return false;
				}

				var size = item.WatchedAttributes.GetInt("stackCount", item.Slot.Itemstack.StackSize);
				if (item.Slot.Itemstack.StackSize != size) {
					item.Slot.Itemstack.StackSize = size;
				}

				__result = size > 1 ? $"{item.Slot.Itemstack.GetName()} ({size}x)" : item.Slot.Itemstack.GetName();

				return false;
			case EntityProjectileBase projectile:
				var stack = projectile.ProjectileStack;
				if (stack is null) {
					return false;
				}

				if (stack is { Class: EnumItemClass.Item, Item: null }) {
					ref var item = ref UnsafeAccessorExtensions.GetItemStack_item(stack);
					item = projectile.Api.World.GetItem(stack.Id);
				}

				if (stack is { Class: EnumItemClass.Block, Block: null }) {
					ref var block = ref UnsafeAccessorExtensions.GetItemStack_block(stack);
					block = projectile.Api.World.GetBlock(stack.Id);
				}

				if (stack.Collectible is not null) {
					__result = stack.Collectible.GetHeldItemName(stack);
				}

				return false;
			default: return true;
		}
	}
}