using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.Common;

[assembly: ModInfo(name: "精确拾取", modID: "precisepickedup", Authors = ["神麤詭末"], Description = "允许空手时鼠标右键拾取掉落物，同时会显示掉落物的名称和数量。")]

namespace PrecisePickedUp;

public sealed class PrecisePickedUpModSystem : ModSystem {
	static private readonly MethodInfo GameMainRayTraceForSelection = AccessTools.Method(typeof(GameMain),
		nameof(GameMain.RayTraceForSelection),
		[
			typeof(IWorldIntersectionSupplier),
			typeof(Ray),
			typeof(BlockSelection).MakeByRefType(),
			typeof(EntitySelection).MakeByRefType(),
			typeof(BlockFilter),
			typeof(EntityFilter)
		]);

	static private readonly MethodInfo GameMainRayTraceForSelectionPreFix =
		AccessTools.Method(typeof(GameMainRayTraceForSelectionPatch), "PreFix");

	static private readonly MethodInfo GameMainGetIntersectingEntities =
		AccessTools.Method(typeof(GameMain), nameof(GameMain.GetIntersectingEntities));

	static private readonly MethodInfo GameMainGetIntersectingEntitiesPreFix =
		AccessTools.Method(typeof(GameMainGetIntersectingEntitiesPatch), "PreFix");

	static private readonly MethodInfo EntityGetName = AccessTools.Method(typeof(Entity), nameof(Entity.GetName));

	static private readonly MethodInfo EntityGetNamePreFix =
		AccessTools.Method(typeof(EntityGetNamePatch), "PreFix");

	static private readonly MethodInfo EntityItemCanCollect =
		AccessTools.Method(typeof(EntityItem), nameof(EntityItem.CanCollect));

	static private readonly MethodInfo EntityItemCanCollectPreFix =
		AccessTools.Method(typeof(EntityItemCanCollectPatch), "PreFix");

	static private readonly MethodInfo EntityItemInitialize =
		AccessTools.Method(typeof(EntityItem), nameof(EntityItem.Initialize));

	static private readonly MethodInfo EntityItemInitializePosFix =
		AccessTools.Method(typeof(EntityItemInitializePatch), "PosFix");

	public string HarmonyId => Mod.Info.ModID;

	public Harmony HarmonyInstance => new(HarmonyId);
	public static Config Config { get; private set; }

	public override void Start(ICoreAPI api) {
		LoadConfig(api);
		HarmonyInstance.Patch(original: EntityItemInitialize,
			postfix: EntityItemInitializePosFix);
	}

	static private void LoadConfig(ICoreAPI api) {
		try {
			Config = api.LoadModConfig<Config?>("PrecisePickedUp.json") ?? new Config();
		} catch {
			Config = new();
		}

		api.StoreModConfig(Config, "PrecisePickedUp.json");
	}

	public override void StartClientSide(ICoreClientAPI api) {
		GameMainRayTraceForSelectionPatch.api = api;
		api.RegisterEntityRendererClass("Item", typeof(MultiEntityItemRenderer));

		HarmonyInstance.Patch(original: EntityItemCanCollect,
			prefix: EntityItemCanCollectPreFix);

		HarmonyInstance.Patch(original: GameMainRayTraceForSelection,
			prefix: GameMainRayTraceForSelectionPreFix);

		HarmonyInstance.Patch(original: EntityGetName,
			prefix: EntityGetNamePreFix);

		HarmonyInstance.Patch(original: GameMainGetIntersectingEntities,
			prefix: GameMainGetIntersectingEntitiesPreFix);
	}

	public override void Dispose() {
		base.Dispose();

		HarmonyInstance.Unpatch(original: EntityItemInitialize,
			patch: EntityItemInitializePosFix);

		HarmonyInstance.Unpatch(original: EntityItemCanCollect,
			patch: EntityItemCanCollectPreFix);

		HarmonyInstance.Unpatch(original: GameMainRayTraceForSelection,
			patch: GameMainRayTraceForSelectionPreFix);

		HarmonyInstance.Unpatch(original: EntityGetName,
			patch: EntityGetNamePreFix);

		HarmonyInstance.Unpatch(original: GameMainGetIntersectingEntities,
			patch: GameMainGetIntersectingEntitiesPreFix);
	}
}