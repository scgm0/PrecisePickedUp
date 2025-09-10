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
		if (entity is not ProjectileEntity { CanBeCollected: true } projectile) {
			return false;
		}

		var stack = projectile.ProjectileStack!;
		if (stack.Item is null) {
			ref var item = ref UnsafeAccessorExtensions.GetItemStack_item(stack);
			item = entity.Api.World.GetItem(stack.Id);
		}

		s = stack.Item!.GetHeldItemName(stack);

		return true;
	}

	public static void GetInfoText(Entity entity, StringBuilder infotext) {
		if (entity is not ProjectileEntity { CanBeCollected: true } projectile) {
			return;
		}

		var stack = projectile.ProjectileStack!;
		if (stack.Item is null) {
			ref var item = ref UnsafeAccessorExtensions.GetItemStack_item(stack);
			item = entity.Api.World.GetItem(stack.Id);
		}

		stack.Item!.GetHeldItemInfo(new DummySlot(stack), infotext, entity.Api.World, ClientSettings.ExtendedDebugInfo);
	}

	public static bool RayTraceForSelection(Entity entity) { return entity is ProjectileEntity { CanBeCollected: true }; }

	public static ItemStack? GetProjectileItemStack(Entity entity) {
		return entity is not ProjectileEntity projectile ? null : projectile.ProjectileStack;
	}

	public static bool NotCollect(Entity entity) {
		if (entity is not ProjectileEntity projectileEntity) {
			return false;
		}

		var stack = projectileEntity.ProjectileStack!;
		if (stack.Item is null) {
			ref var item = ref UnsafeAccessorExtensions.GetItemStack_item(stack);
			item = entity.Api.World.GetItem(stack.Id);
		}

		var stats = stack.Item!.GetCollectibleBehavior<ProjectileBehavior>(true).GetStats(stack);
		projectileEntity.CanBeCollected = stats.CanBeCollected;

		return !projectileEntity.CanBeCollected;
	}
}