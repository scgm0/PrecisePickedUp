using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;

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
		if (entity.Api is ICoreServerAPI && byEntity is EntityPlayer player && mode == EnumInteractMode.Interact &&
			(PrecisePickedUpModSystem.Config.PickupConditions == PickupConditionsEnum.OnlyRightHand &&
				player.Player.InventoryManager.ActiveHotbarSlot?.Itemstack is null ||
				PrecisePickedUpModSystem.Config.PickupConditions == PickupConditionsEnum.LeftOrRightHand &&
				(player.Player.InventoryManager.GetHotbarItemstack(10) is null || player.Player.InventoryManager.ActiveHotbarSlot?.Itemstack is null) ||
				PrecisePickedUpModSystem.Config.PickupConditions == PickupConditionsEnum.None)) {
			var item = (EntityItem)entity;
			if (player.Player.InventoryManager.TryGiveItemstack(item.Itemstack, true)) {
				item.WatchedAttributes.SetInt("stackCount", item.Itemstack.StackSize);
				if (item.Itemstack.StackSize <= 0) {
					entity.Die(EnumDespawnReason.PickedUp);
				}
			}
		}


		handled = EnumHandling.PreventSubsequent;
	}

	public override WorldInteraction[] GetInteractionHelp(
		IClientWorldAccessor world,
		EntitySelection es,
		IClientPlayer player,
		ref EnumHandling handled) {
		return [
			new() {
				ActionLangCode = ActionLangCode,
				RequireFreeHand = true,
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