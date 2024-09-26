using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
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
		if (!PrecisePickedUpModSystem.Config.AutoMerge || entity.Api is ICoreClientAPI) return;
		_cumulativeTime += deltaTime;
		if (_cumulativeTime < PrecisePickedUpModSystem.Config.MergeInterval) return;
		_cumulativeTime = 0;
		var item = (EntityItem)entity;
		if (item.Slot.Itemstack == null || !item.Collided) return;
		var quantity = item.Slot.Itemstack.Collectible.MaxStackSize - item.Slot.Itemstack.StackSize;
		if (quantity <= 0) return;
		foreach (var entity1 in item.Api.World.GetEntitiesAround(item.SidedPos.XYZ,
			PrecisePickedUpModSystem.Config.MergeRange.X,
			PrecisePickedUpModSystem.Config.MergeRange.Y,
			e => e is EntityItem {
				Collided: true, Slot: { } slot
			} && slot.StackSize <= item.Slot.StackSize && e != item)) {
			var entityItem = (EntityItem)entity1;
			quantity -= entityItem.Slot.TryPutInto(item.Api.World, item.Slot, quantity);
			item.WatchedAttributes.SetInt("stackCount", item.Itemstack.StackSize);
			if (entityItem.Slot.Itemstack == null)
				entityItem.Die(EnumDespawnReason.Expire);
			if (quantity <= 0)
				break;
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
				player.Player.InventoryManager.ActiveHotbarSlot?.Itemstack is not null) return;
		var collect = (EntityBehaviorCollectEntities)player.GetBehavior("collectitems");
		collect.OnFoundCollectible(entity);
		var item = (EntityItem)entity;
		if (item.Itemstack is not { StackSize: > 0 }) {
			item.WatchedAttributes.SetInt("stackCount", 0);
			item.Die(EnumDespawnReason.PickedUp);
		} else {
			item.WatchedAttributes.SetInt("stackCount", item.Itemstack.StackSize);
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
		if (!PrecisePickedUpModSystem.Config.ShowItemDescription) return;
		var item = (EntityItem)entity;
		infotext.Append(item.Slot.GetStackDescription((IClientWorldAccessor)item.World, ClientSettings.ExtendedDebugInfo));
	}
}