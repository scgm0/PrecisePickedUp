using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;

[assembly: ModInfo(name: "精确拾取", modID: "precisepickedup", Authors = ["神麤詭末"], Description = "允许空手时鼠标右键拾取掉落物，同时会显示掉落物的名称和数量。")]

namespace PrecisePickedUp;

public sealed class PrecisePickedUpModSystem : ModSystem {
	static private readonly MethodInfo SystemMouseInWorldInteractionsUpdateCurrentSelectionEntityFilter =
		typeof(SystemMouseInWorldInteractions).GetMethod("<UpdateCurrentSelection>b__22_1",
			BindingFlags.NonPublic | BindingFlags.Instance);

	static private readonly MethodInfo SystemMouseInWorldInteractionsUpdateCurrentSelectionEntityFilterPrefix =
		typeof(SystemMouseInWorldInteractionsUpdateCurrentSelectionEntityFilterPrefix).GetMethod("Prefix");

	static private readonly MethodInfo EntityGetName = typeof(Entity).GetMethod(nameof(Entity.GetName));

	static private readonly MethodInfo EntityGetNamePrefix =
		typeof(EntityGetNamePrefix).GetMethod("Prefix");

	static private readonly MethodInfo BlockCanPlaceBlockActionConsumable =
		typeof(Block).GetNestedType("<>c", BindingFlags.NonPublic)!.GetMethod("<CanPlaceBlock>b__124_0",
			BindingFlags.NonPublic | BindingFlags.Instance);

	static private readonly MethodInfo BlockCanPlaceBlockActionConsumablePrefix =
		typeof(BlockCanPlaceBlockActionConsumablePrefix).GetMethod("Prefix");

	static private readonly MethodInfo EntityItemCanCollect = typeof(EntityItem).GetMethod(nameof(EntityItem.CanCollect));

	static private readonly MethodInfo EntityItemCanCollectPrefix =
		typeof(EntityItemCanCollectPrefix).GetMethod("Prefix");

	private ICoreAPI _api;
	public string HarmonyId => Mod.Info.ModID;

	public Harmony HarmonyInstance => new(HarmonyId);
	public static Config Config { get; set; }

	public override void Start(ICoreAPI api) {
		_api = api;
		api.Event.OnEntityLoaded += OnEntityLoaded;
		api.Event.OnEntitySpawn += OnEntityLoaded;
		switch (api) {
			case ICoreServerAPI:
				LoadConfig(api);
				break;
			case ICoreClientAPI client:
				client.Event.LevelFinalize += () => LoadConfig(client);
				break;
		}
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
		api.RegisterEntityRendererClass("Item", typeof(MultiEntityItemRenderer));

		HarmonyInstance.Patch(original: EntityItemCanCollect,
			prefix: EntityItemCanCollectPrefix);

		HarmonyInstance.Patch(original: SystemMouseInWorldInteractionsUpdateCurrentSelectionEntityFilter,
			prefix: SystemMouseInWorldInteractionsUpdateCurrentSelectionEntityFilterPrefix);

		HarmonyInstance.Patch(original: EntityGetName,
			prefix: EntityGetNamePrefix);

		HarmonyInstance.Patch(
			original: BlockCanPlaceBlockActionConsumable,
			prefix: BlockCanPlaceBlockActionConsumablePrefix);
	}

	public override void Dispose() {
		base.Dispose();
		_api.Event.OnEntityLoaded -= OnEntityLoaded;
		_api.Event.OnEntitySpawn -= OnEntityLoaded;

		HarmonyInstance.Unpatch(original: EntityItemCanCollect,
			patch: EntityItemCanCollectPrefix);

		HarmonyInstance.Unpatch(original: SystemMouseInWorldInteractionsUpdateCurrentSelectionEntityFilter,
			patch: SystemMouseInWorldInteractionsUpdateCurrentSelectionEntityFilterPrefix);

		HarmonyInstance.Unpatch(original: EntityGetName,
			patch: EntityGetNamePrefix);

		HarmonyInstance.Unpatch(
			original: BlockCanPlaceBlockActionConsumable,
			patch: BlockCanPlaceBlockActionConsumablePrefix);
	}

	static private void OnEntityLoaded(Entity entity) {
		if (entity is not EntityItem entityItem) return;
		entityItem.SidedProperties.Behaviors.Add(new EntityItemBehavior(entity));
	}
}