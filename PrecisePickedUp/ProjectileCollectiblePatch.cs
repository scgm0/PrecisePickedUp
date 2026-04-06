using Vintagestory.GameContent;

namespace PrecisePickedUp;

public static class ProjectileCollectiblePatch {
	public static bool GetPreFix(EntityProjectile __instance, ref bool __result) {
		__result = __instance.WatchedAttributes.GetBool("collectible");
		return false;
	}

	public static bool SetPreFix(EntityProjectile __instance, bool value) {
		__instance.WatchedAttributes.SetBool("collectible", value);
		return false;
	}
}