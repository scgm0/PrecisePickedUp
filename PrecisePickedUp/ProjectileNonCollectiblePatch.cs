using Vintagestory.GameContent;

namespace PrecisePickedUp;

public static class ProjectileNonCollectiblePatch {
	public static bool GetPreFix(EntityProjectile __instance, ref bool __result) {
		__result = __instance.WatchedAttributes.GetBool("nonCollectible");
		return false;
	}

	public static bool SetPreFix(EntityProjectile __instance, bool value) {
		__instance.WatchedAttributes.SetBool("nonCollectible", value);
		return false;
	}
}