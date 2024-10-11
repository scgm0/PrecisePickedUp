using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace PrecisePickedUp;

public static class EntityGetNamePatch {
	public static bool PreFix(Entity __instance, ref string __result, MethodBase __originalMethod) {
		switch (__instance) {
			case EntityItem item:
				__result =
					$"{item.Itemstack.GetName()} (x{item.WatchedAttributes.GetInt("stackCount", item.Itemstack.StackSize)})";
				return false;
			case EntityProjectile projectile:
				var stack = projectile.ProjectileStack;
				__result = Lang.GetMatching($"{projectile.Code.Domain}:{stack.Class.Name()}-{projectile.Code.Path}");
				return false;
			default: return true;
		}
	}
}