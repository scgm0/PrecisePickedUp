using Vintagestory.API.MathTools;

namespace PrecisePickedUp;

public enum PickupConditionsEnum {
	OnlyRightHand,
	LeftOrRightHand,
	None
}

public record struct Config() {
	public PickupConditionsEnum PickupConditions { get; set; } = PickupConditionsEnum.LeftOrRightHand;
	public bool CanAutoCollect { get; set; } = true;
	public bool CanPlaceBlock { get; set; } = true;
	public bool ShowItemDescription { get; set; } = true;
	public bool AutoMerge { get; set; } = true;
	public Vec2f MergeRange { get; set; } = new(1.5f, 0.2f);
	public float MergeInterval { get; set; } = 5;
	public bool RangePickup { get; set; } = true;
	public Vec2f PickupRange { get; set; } = new(1, 0.2f);
}