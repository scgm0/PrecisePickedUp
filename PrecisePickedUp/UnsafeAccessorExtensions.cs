using System.Runtime.CompilerServices;
using Vintagestory.API.Common;

namespace PrecisePickedUp;

public static class UnsafeAccessorExtensions {
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "item")]
	public static extern ref Item GetItemStack_item(ItemStack stack);
}