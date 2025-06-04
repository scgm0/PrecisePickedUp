using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
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
				var size = item.WatchedAttributes.GetInt("stackCount", item.Slot.Itemstack.StackSize);
				if (item.Slot.Itemstack.StackSize != size) {
					item.Slot.Itemstack.StackSize = size;
				}

				__result = size > 1 ? $"{item.Slot.Itemstack.GetName()} ({size}x)" : item.Slot.Itemstack.GetName();
				return false;
			case EntityProjectile projectile:
				var stack = projectile.ProjectileStack;
				if (stack.Item is null) {
					var item = projectile.Api.World.GetItem(stack.Id);
					AccessTools.Field(typeof(ItemStack), "item").SetValue(stack, item);
				}

				__result = stack.Item!.GetHeldItemName(stack);
				return false;
			default: return true;
		}
	}
}