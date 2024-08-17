using Vintagestory.API.MathTools;

namespace PrecisePickedUp;

public enum PickupConditionsEnum {
	OnlyRightHand,
	LeftOrRightHand,
	None
}

public struct Config() {
	public PickupConditionsEnum PickupConditions { get; set; } = PickupConditionsEnum.OnlyRightHand;
	public bool CanAutoCollect { get; set; } = true;
	public bool ShowItemDescription { get; set; } = true;
	public bool AutoMerge { get; set; } = true;
	public Vec2f MergeRange { get; set; } = new(1, 0.2f);
	public float MergeInterval { get; set; } = 5;
}