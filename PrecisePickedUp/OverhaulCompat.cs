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
			postfix: PrecisePickedUpModSystem.EntityProjectileBaseInitializePosFix);
	}

	public static void Unpatch() {
		PrecisePickedUpModSystem.Instance?.HarmonyInstance.Unpatch(ProjectileEntityInitialize,
			PrecisePickedUpModSystem.EntityProjectileBaseInitializePosFix);
	}

	public static bool EntityGetName(Entity entity, ref string s) {
		if (entity is not ProjectileEntity { CanBeCollected: true } projectile) {
			return false;
		}

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

		s = stack.Collectible.GetHeldItemName(stack);

		return true;
	}

	public static void GetInfoText(Entity entity, StringBuilder infotext) {
		if (entity is not ProjectileEntity { CanBeCollected: true } projectile) {
			return;
		}

		var stack = projectile.ProjectileStack;
		if (stack is null) {
			return;
		}

		if (stack is { Class: EnumItemClass.Item, Item: null }) {
			ref var item = ref UnsafeAccessorExtensions.GetItemStack_item(stack);
			item = projectile.Api.World.GetItem(stack.Id);
		}

		if (stack is { Class: EnumItemClass.Block, Block: null }) {
			ref var block = ref UnsafeAccessorExtensions.GetItemStack_block(stack);
			block = projectile.Api.World.GetBlock(stack.Id);
		}

		stack.Collectible.GetHeldItemInfo(new DummySlot(stack), infotext, entity.Api.World, ClientSettings.ExtendedDebugInfo);
	}

	public static bool RayTraceForSelection(Entity entity) { return entity is ProjectileEntity { CanBeCollected: true }; }

	public static ItemStack? GetProjectileItemStack(Entity entity) {
		return entity is not ProjectileEntity projectile ? null : projectile.ProjectileStack;
	}

	public static bool NotCollect(Entity entity) {
		if (entity is not ProjectileEntity projectile) {
			return false;
		}

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

		var stats = stack.Collectible.GetCollectibleBehavior<ProjectileBehavior>(true).GetStats(stack);
		projectile.CanBeCollected = stats.CanBeCollected;

		return !projectile.CanBeCollected;
	}
}