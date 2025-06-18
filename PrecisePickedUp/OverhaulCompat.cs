using System.Reflection;
using System.Text;
using CombatOverhaul.RangedSystems;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.Client.NoObf;

namespace PrecisePickedUp;

public static class OverhaulCompat {
	static private readonly MethodInfo ProjectileEntityInitialize =
		AccessTools.Method(typeof(ProjectileEntity), nameof(ProjectileEntity.Initialize));

	public static void Patch() {
		PrecisePickedUpModSystem.Instance?.HarmonyInstance.Patch(ProjectileEntityInitialize,
			postfix: PrecisePickedUpModSystem.EntityProjectileInitializePosFix);
	}

	public static void Unpatch() {
		PrecisePickedUpModSystem.Instance?.HarmonyInstance.Unpatch(ProjectileEntityInitialize,
			PrecisePickedUpModSystem.EntityProjectileInitializePosFix);
	}

	public static bool EntityGetName(Entity entity, ref string s) {
		if (entity is not ProjectileEntity projectile) {
			return false;
		}

		var stack = projectile.ProjectileStack!;
		if (stack.Item is null) {
			var item = entity.Api.World.GetItem(stack.Id);
			AccessTools.Field(typeof(ItemStack), "item").SetValue(stack, item);
		}

		s = stack.Item!.GetHeldItemName(stack);

		return true;
	}

	public static void GetInfoText(Entity entity, StringBuilder infotext) {
		if (entity is not ProjectileEntity projectile) {
			return;
		}

		var stack = projectile.ProjectileStack!;
		stack.Item.GetHeldItemInfo(new DummySlot(stack), infotext, entity.Api.World, ClientSettings.ExtendedDebugInfo);
	}

	public static bool RayTraceForSelection(Entity entity) { return entity is ProjectileEntity; }

	public static ItemStack? GetProjectileItemStack(Entity entity) {
		return entity is not ProjectileEntity projectile ? null : projectile.ProjectileStack;
	}
}