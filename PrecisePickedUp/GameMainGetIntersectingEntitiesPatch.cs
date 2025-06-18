using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace PrecisePickedUp;

public class GameMainGetIntersectingEntitiesPatch {
	public static void PreFix(ref ActionConsumable<Entity> matches) {
		if (PrecisePickedUpModSystem.Config.CanPlaceBlock || matches is null ||
			!matches.Method.Name.StartsWith("<CanPlaceBlock>")) {
			return;
		}

		var oldMatches = matches;
		matches = e => e is EntityItem || oldMatches(e);
	}
}