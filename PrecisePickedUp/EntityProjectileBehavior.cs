using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace PrecisePickedUp;

public class EntityProjectileBehavior(Entity entity) : EntityBehavior(entity) {

	static private readonly string ActionLangCode = Lang.Get($"precisepickedup:{nameof(EnumDespawnReason.PickedUp)}");

	public override string PropertyName() { return nameof(EntityProjectileBehavior); }

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
}