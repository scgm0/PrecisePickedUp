using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.Common;
using Vintagestory.GameContent;

[assembly: ModInfo("精确拾取", "precisepickedup", Authors = ["神麤詭末"], Description = "允许空手时鼠标右键拾取掉落物，同时会显示掉落物的名称和数量。")]

namespace PrecisePickedUp;

public sealed class PrecisePickedUpModSystem : ModSystem {
	public static readonly MethodInfo GameMainRayTraceForSelection = AccessTools.Method(typeof(GameMain),
		nameof(GameMain.RayTraceForSelection),
		[
			typeof(IWorldIntersectionSupplier),
			typeof(Ray),
			typeof(BlockSelection).MakeByRefType(),
			typeof(EntitySelection).MakeByRefType(),
			typeof(BlockFilter),
			typeof(EntityFilter)
		]);

	public static readonly MethodInfo GameMainRayTraceForSelectionPreFix =
		AccessTools.Method(typeof(GameMainRayTraceForSelectionPatch), "PreFix");

	public static readonly MethodInfo GameMainGetIntersectingEntities =
		AccessTools.Method(typeof(GameMain), nameof(GameMain.GetIntersectingEntities));

	public static readonly MethodInfo GameMainGetIntersectingEntitiesPreFix =
		AccessTools.Method(typeof(GameMainGetIntersectingEntitiesPatch), "PreFix");

	public static readonly MethodInfo EntityGetName = AccessTools.Method(typeof(Entity), nameof(Entity.GetName));

	public static readonly MethodInfo EntityGetNamePreFix =
		AccessTools.Method(typeof(EntityGetNamePatch), "PreFix");

	public static readonly MethodInfo EntityItemCanCollect =
		AccessTools.Method(typeof(EntityItem), nameof(EntityItem.CanCollect));

	public static readonly MethodInfo EntityItemCanCollectPreFix =
		AccessTools.Method(typeof(EntityItemCanCollectPatch), "PreFix");

	public static readonly MethodInfo EntityItemInitialize =
		AccessTools.Method(typeof(EntityItem), nameof(EntityItem.Initialize));

	public static readonly MethodInfo EntityItemInitializePosFix =
		AccessTools.Method(typeof(EntityItemInitializePatch), "PosFix");

	public static readonly MethodInfo EntityProjectileInitialize =
		AccessTools.Method(typeof(EntityProjectile), nameof(EntityProjectile.Initialize));

	public static readonly MethodInfo EntityProjectileInitializePosFix =
		AccessTools.Method(typeof(ProjectileInitializePatch), "PosFix");

	public static readonly MethodInfo DoRender3DOpaque =
		AccessTools.Method(typeof(EntityItemRenderer), nameof(EntityItemRenderer.DoRender3DOpaque));

	public static readonly MethodInfo DoRender3DOpaquePreFix =
		AccessTools.Method(typeof(EntityItemRendererPatch), nameof(EntityItemRendererPatch.DoRender3DOpaquePreFix));

	public static readonly MethodInfo ProjectileNonCollectibleGet =
		AccessTools.PropertyGetter(typeof(EntityProjectile), nameof(EntityProjectile.NonCollectible));

	public static readonly MethodInfo ProjectileNonCollectibleGetPreFix =
		AccessTools.Method(typeof(ProjectileNonCollectiblePatch), nameof(ProjectileNonCollectiblePatch.GetPreFix));

	public static readonly MethodInfo ProjectileNonCollectibleSet =
		AccessTools.PropertySetter(typeof(EntityProjectile), nameof(EntityProjectile.NonCollectible));

	public static readonly MethodInfo ProjectileNonCollectibleSetPreFix =
		AccessTools.Method(typeof(ProjectileNonCollectiblePatch), nameof(ProjectileNonCollectiblePatch.SetPreFix));

	public string HarmonyId => Mod.Info.ModID;

	public Harmony HarmonyInstance => new(HarmonyId);

	public static Config Config { get; private set; }

	public static ICoreAPI? Api { get; private set; }

	public static bool EnableOverhaulCompat => Api?.ModLoader.IsModEnabled("overhaullib") ?? false;

	public static PrecisePickedUpModSystem? Instance { get; private set; }

	public PrecisePickedUpModSystem() { Instance = this; }

	public override void Start(ICoreAPI? api) {
		Api = api;
		LoadConfig();
		if (EnableOverhaulCompat) {
			OverhaulCompat.Patch();
		}

		HarmonyInstance.Patch(EntityItemInitialize,
			postfix: EntityItemInitializePosFix);
		HarmonyInstance.Patch(EntityProjectileInitialize,
			postfix: EntityProjectileInitializePosFix);
		HarmonyInstance.Patch(ProjectileNonCollectibleGet,
			prefix: ProjectileNonCollectibleGetPreFix);
		HarmonyInstance.Patch(ProjectileNonCollectibleSet,
			prefix: ProjectileNonCollectibleSetPreFix);
	}

	static private void LoadConfig() {
		try {
			if (Api != null) {
				Config = Api.LoadModConfig<Config?>("PrecisePickedUp.json") ?? new Config();
			}
		} catch {
			Config = new();
		}

		Config = Config with { PickupRange = Config.MergeRange ?? new Vec2f(1.5f, 0.2f) };
		Config = Config with { PickupRange = Config.PickupRange ?? new Vec2f(1, 0.2f) };
		Api?.StoreModConfig(Config, "PrecisePickedUp.json");
	}

	public override void StartClientSide(ICoreClientAPI api) {
		GameMainRayTraceForSelectionPatch.api = api;

		HarmonyInstance.Patch(EntityItemCanCollect,
			EntityItemCanCollectPreFix);

		HarmonyInstance.Patch(GameMainRayTraceForSelection,
			GameMainRayTraceForSelectionPreFix);

		HarmonyInstance.Patch(EntityGetName,
			EntityGetNamePreFix);

		HarmonyInstance.Patch(GameMainGetIntersectingEntities,
			GameMainGetIntersectingEntitiesPreFix);

		HarmonyInstance.Patch(DoRender3DOpaque,
			DoRender3DOpaquePreFix);
	}

	public override void Dispose() {
		base.Dispose();

		if (EnableOverhaulCompat) {
			OverhaulCompat.Unpatch();
		}

		HarmonyInstance.Unpatch(EntityItemInitialize,
			EntityItemInitializePosFix);

		HarmonyInstance.Unpatch(EntityProjectileInitialize,
			EntityProjectileInitializePosFix);

		HarmonyInstance.Unpatch(EntityItemCanCollect,
			EntityItemCanCollectPreFix);

		HarmonyInstance.Unpatch(GameMainRayTraceForSelection,
			GameMainRayTraceForSelectionPreFix);

		HarmonyInstance.Unpatch(EntityGetName,
			EntityGetNamePreFix);

		HarmonyInstance.Unpatch(GameMainGetIntersectingEntities,
			GameMainGetIntersectingEntitiesPreFix);

		HarmonyInstance.Unpatch(DoRender3DOpaque,
			DoRender3DOpaquePreFix);

		HarmonyInstance.Unpatch(ProjectileNonCollectibleGet,
			ProjectileNonCollectibleGetPreFix);

		HarmonyInstance.Unpatch(ProjectileNonCollectibleSet,
			ProjectileNonCollectibleSetPreFix);
	}
}