using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace PrecisePickedUp;

public sealed class EntityItemBehavior(Entity entity) : EntityBehavior(entity) {

	static private readonly string ActionLangCode = Lang.Get($"precisepickedup:{nameof(EnumDespawnReason.PickedUp)}");
	private float _cumulativeTime;
	public override string PropertyName() { return nameof(EntityItemBehavior); }

	public override void OnGameTick(float deltaTime) {
		if (!PrecisePickedUpModSystem.Config.AutoMerge || entity.Api is ICoreClientAPI) {
			return;
		}

		_cumulativeTime += deltaTime;
		if (_cumulativeTime < PrecisePickedUpModSystem.Config.MergeInterval) {
			return;
		}

		_cumulativeTime = 0;
		var entityItem = (EntityItem)entity;
		if (entityItem.Slot.Itemstack == null) {
			return;
		}

		var currentSize = entityItem.Slot.Itemstack.StackSize;
		if (entityItem.Slot.Itemstack.StackSize <= 0) {
			entityItem.Die(EnumDespawnReason.Expire);
			entityItem.WatchedAttributes.SetInt("stackCount", 0);
			return;
		}

		var quantity = entityItem.Slot.Itemstack.Collectible.MaxStackSize - currentSize;
		if (quantity <= 0) {
			return;
		}

		var entities = entityItem.Api.World.GetEntitiesAround(entityItem.Pos.XYZ,
			PrecisePickedUpModSystem.Config.MergeRange.X,
			PrecisePickedUpModSystem.Config.MergeRange.Y,
			e => e != entity && e is EntityItem {
					Slot: {
						Itemstack: { } itemStack
					} slot
				} && e != entityItem && itemStack.Equals(entityItem.Api.World,
					entityItem.Slot.Itemstack,
					GlobalConstants.IgnoredStackAttributes) &&
				slot.StackSize <= entityItem.Slot.StackSize);

		foreach (var entity1 in entities) {
			var entityItem2 = (EntityItem)entity1;
			quantity -= entityItem2.Slot.TryPutInto(entityItem.Api.World, entityItem.Slot, quantity);

			if (entityItem2.Slot.Itemstack is not { StackSize: > 0 }) {
				entityItem2.Die(EnumDespawnReason.PickedUp);
			}

			if (quantity <= 0) {
				break;
			}
		}

		if (currentSize != entityItem.Slot.Itemstack.StackSize) {
			entityItem.WatchedAttributes.SetInt("stackCount", entityItem.Slot.Itemstack.StackSize);
		}
	}

	public override void OnInteract(
		EntityAgent byEntity,
		ItemSlot itemslot,
		Vec3d hitPosition,
		EnumInteractMode mode,
		ref EnumHandling handled) {
		if (entity.Api is not ICoreServerAPI || byEntity is not EntityPlayer player || mode != EnumInteractMode.Interact ||
			PrecisePickedUpModSystem.Config.PickupConditions == PickupConditionsEnum.OnlyRightHand &&
			player.Player.InventoryManager.ActiveHotbarSlot?.Itemstack is not null ||
			PrecisePickedUpModSystem.Config.PickupConditions == PickupConditionsEnum.LeftOrRightHand &&
			player.Player.InventoryManager.GetHotbarItemstack(10) is not null &&
			player.Player.InventoryManager.ActiveHotbarSlot?.Itemstack is not null) {
			return;
		}

		OnCollideWithPlayer(player);

		if (!PrecisePickedUpModSystem.Config.RangePickup) {
			return;
		}

		var itemStack = ((EntityItem)entity).Slot.Itemstack!;
		var entities = entity.Api.World.GetEntitiesAround(entity.Pos.XYZ,
			PrecisePickedUpModSystem.Config.PickupRange.X,
			PrecisePickedUpModSystem.Config.PickupRange.Y,
			e => e != entity && e is EntityItem entityItem && itemStack.Equals(entity.World,
				entityItem.Slot.Itemstack,
				GlobalConstants.IgnoredStackAttributes));
		foreach (var entity1 in entities) {
			entity1.GetBehavior<EntityItemBehavior>()?.OnCollideWithPlayer(player);
		}
	}

	public void OnCollideWithPlayer(EntityPlayer player) {
		var collect = player.GetBehavior("collectitems") as EntityBehaviorCollectEntities;
		collect?.OnFoundCollectible(entity);
		var item = (EntityItem)entity;
		if (item.Slot.Itemstack is not { StackSize: > 0 }) {
			item.WatchedAttributes.SetInt("stackCount", 0);
			item.Die(EnumDespawnReason.PickedUp);
		} else {
			item.WatchedAttributes.SetInt("stackCount", item.Slot.Itemstack.StackSize);
		}
	}

	public override WorldInteraction[] GetInteractionHelp(
		IClientWorldAccessor world,
		EntitySelection es,
		IClientPlayer player,
		ref EnumHandling handled) {
		return [
			new() {
				ActionLangCode = ActionLangCode,
				RequireFreeHand = PrecisePickedUpModSystem.Config.PickupConditions != PickupConditionsEnum.None,
				MouseButton = EnumMouseButton.Right
			}
		];
	}

	public override void GetInfoText(StringBuilder infotext) {
		if (!PrecisePickedUpModSystem.Config.ShowItemDescription) {
			return;
		}

		var item = (EntityItem)entity;
		infotext.Append(item.Slot.GetStackDescription((IClientWorldAccessor)item.World, ClientSettings.ExtendedDebugInfo));
	}
}