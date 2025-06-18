using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace PrecisePickedUp;

public static class EntityItemRendererPatch {
	public static readonly float[] XOffsets = [
		-0.37f,
		-0.24f,
		0.31f,
		-0.28f,
		-0.25f,
		0.32f,
		-0.19f,
		0.16f,
		-0.13f,
		0.1f
	];

	public static readonly float[] YOffsets = [
		0.03f,
		0.06f,
		0.09f,
		0.12f,
		0.15f,
		0.18f,
		0.21f,
		0.24f,
		0.27f,
		0.3f
	];

	public static readonly float[] ZOffsets = [
		0.31f,
		-0.14f,
		0.31f,
		-0.28f,
		0.25f,
		-0.22f,
		-0.19f,
		0.16f,
		-0.13f,
		-0.1f
	];

	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "LoadModelMatrix")]
	public static extern void LoadModelMatrix(
		EntityItemRenderer renderer,
		ItemRenderInfo renderInfo,
		bool isShadowPass,
		float dt);

	public static bool DoRender3DOpaquePreFix(
		float dt,
		bool isShadowPass,
		EntityItemRenderer __instance,
		ref Entity ___entity,
		ref EntityItem ___entityitem,
		ref ICoreClientAPI ___capi,
		ref Vec3d ___lerpedPos,
		ref ItemSlot ___inslot,
		ref Vec4f ___glowRgb,
		ref Vec4f ___particleOutTransform,
		ref float ___accum) {
		if (isShadowPass && !___entity.IsRendered) {
			return false;
		}

		var stackCount = ___entity.WatchedAttributes.GetInt("stackCount", ___inslot.Itemstack?.StackSize ?? 0);
		if (stackCount <= 0) {
			return false;
		}

		if (EntityItemRenderer.RunWittySkipRenderAlgorithm) {
			var x = (int)___entity.Pos.X;
			var y = (int)___entity.Pos.Y;
			var z = (int)___entity.Pos.Z;
			var num = (___entityitem.Itemstack.Class == EnumItemClass.Block ? -1 : 1) * ___entityitem.Itemstack.Id;
			if (EntityItemRenderer.LastPos.X == x && EntityItemRenderer.LastPos.Y == y && EntityItemRenderer.LastPos.Z == z &&
				EntityItemRenderer.LastCollectibleId == num) {
				if (___entity.EntityId % EntityItemRenderer.RenderModulo != 0L) {
					return false;
				}
			} else {
				EntityItemRenderer.LastPos.Set(x, y, z);
			}

			EntityItemRenderer.LastCollectibleId = num;
		}

		var render = ___capi.Render;
		___lerpedPos.X += (___entity.Pos.X - ___lerpedPos.X) * 22.0 * dt;
		___lerpedPos.Y += (___entity.Pos.InternalY - ___lerpedPos.Y) * 22.0 * dt;
		___lerpedPos.Z += (___entity.Pos.Z - ___lerpedPos.Z) * 22.0 * dt;
		var itemStackRenderInfo = render.GetItemStackRenderInfo(___inslot, EnumItemRenderTarget.Ground, dt);
		if (itemStackRenderInfo.ModelRef == null || itemStackRenderInfo.Transform == null) {
			return false;
		}

		IStandardShaderProgram? standardShaderProgram = null;
		LoadModelMatrix(__instance, itemStackRenderInfo, isShadowPass, dt);
		var textureSampleName = "tex";
		if (isShadowPass) {
			textureSampleName = "tex2d";
			var numArray = Mat4f.Mul(__instance.ModelMat, ___capi.Render.CurrentModelviewMatrix, __instance.ModelMat);
			Mat4f.Mul(numArray, ___capi.Render.CurrentProjectionMatrix, numArray);
			___capi.Render.CurrentActiveShader.UniformMatrix("mvpMatrix", numArray);
			___capi.Render.CurrentActiveShader.Uniform("origin", new Vec3f());
		} else {
			standardShaderProgram = render.StandardShader;
			standardShaderProgram.Use();
			standardShaderProgram.RgbaTint = ___entity.Swimming ? new(0.5f, 0.5f, 0.5f, 1f) : ColorUtil.WhiteArgbVec;
			standardShaderProgram.DontWarpVertices = 0;
			standardShaderProgram.NormalShaded = 1;
			standardShaderProgram.AlphaTest = itemStackRenderInfo.AlphaTest;
			standardShaderProgram.DamageEffect = itemStackRenderInfo.DamageEffect;
			if (___entity.Swimming) {
				standardShaderProgram.AddRenderFlags =
					(___entityitem.Itemstack.Collectible.MaterialDensity <= 1000 ? 1 : 0) << 12;
				standardShaderProgram.WaterWaveCounter = ___capi.Render.ShaderUniforms.WaterWaveCounter;
			} else {
				standardShaderProgram.AddRenderFlags = 0;
			}

			standardShaderProgram.OverlayOpacity = itemStackRenderInfo.OverlayOpacity;
			if (itemStackRenderInfo.OverlayTexture != null && itemStackRenderInfo.OverlayOpacity > 0.0) {
				standardShaderProgram.Tex2dOverlay2D = itemStackRenderInfo.OverlayTexture.TextureId;
				standardShaderProgram.OverlayTextureSize = new(itemStackRenderInfo.OverlayTexture.Width,
					itemStackRenderInfo.OverlayTexture.Height);
				standardShaderProgram.BaseTextureSize = new(itemStackRenderInfo.TextureSize.Width,
					itemStackRenderInfo.TextureSize.Height);
				var textureAtlasPosition = render.GetTextureAtlasPosition(___entityitem.Itemstack);
				standardShaderProgram.BaseUvOrigin = new(textureAtlasPosition.x1, textureAtlasPosition.y1);
			}

			var asBlockPos = ___entityitem.Pos.AsBlockPos;
			var lightRgBs = ___capi.World.BlockAccessor.GetLightRGBs(asBlockPos.X, asBlockPos.InternalY, asBlockPos.Z);
			var temperature =
				(int)___entityitem.Itemstack.Collectible.GetTemperature(___capi.World,
					___entityitem.Itemstack);
			var incandescenceColorAsColor4F = ColorUtil.GetIncandescenceColorAsColor4f(temperature);
			var num = GameMath.Clamp((temperature - 550) / 2, 0, byte.MaxValue);
			___glowRgb.R = incandescenceColorAsColor4F[0];
			___glowRgb.G = incandescenceColorAsColor4F[1];
			___glowRgb.B = incandescenceColorAsColor4F[2];
			___glowRgb.A = num / (float)byte.MaxValue;
			standardShaderProgram.ExtraGlow = num;
			standardShaderProgram.RgbaAmbientIn = render.AmbientColor;
			standardShaderProgram.RgbaLightIn = lightRgBs;
			standardShaderProgram.RgbaGlowIn = ___glowRgb;
			standardShaderProgram.RgbaFogIn = render.FogColor;
			standardShaderProgram.FogMinIn = render.FogMin;
			standardShaderProgram.FogDensityIn = render.FogDensity;
			standardShaderProgram.ExtraGodray = 0.0f;
			standardShaderProgram.NormalShaded = itemStackRenderInfo.NormalShaded ? 1 : 0;
			standardShaderProgram.ProjectionMatrix = render.CurrentProjectionMatrix;
			standardShaderProgram.ViewMatrix = render.CameraMatrixOriginf;
			standardShaderProgram.ModelMatrix = __instance.ModelMat;
			var itemStack = ___entityitem.Itemstack;
			AdvancedParticleProperties[]? particleProperties = itemStack.Block?.ParticleProperties;
			if (itemStack.Block != null && !___capi.IsGamePaused) {
				Mat4f.MulWithVec4(__instance.ModelMat,
					new Vec4f(itemStack.Block.TopMiddlePos.X,
						itemStack.Block.TopMiddlePos.Y - 0.4f,
						itemStack.Block.TopMiddlePos.Z - 0.5f,
						0.0f),
					___particleOutTransform);
				___accum += dt;
				if (particleProperties != null && particleProperties.Length != 0 && ___accum > 0.02500000037252903) {
					___accum %= 0.025f;
					foreach (var particlePropertiesProvider in particleProperties) {
						particlePropertiesProvider.basePos.X = ___particleOutTransform.X + ___entity.Pos.X;
						particlePropertiesProvider.basePos.Y = ___particleOutTransform.Y + ___entity.Pos.InternalY;
						particlePropertiesProvider.basePos.Z = ___particleOutTransform.Z + ___entity.Pos.Z;
						___entityitem.World.SpawnParticles(particlePropertiesProvider);
					}
				}
			}
		}

		if (!itemStackRenderInfo.CullFaces) {
			render.GlDisableCullFace();
		}

		render.RenderMultiTextureMesh(itemStackRenderInfo.ModelRef, textureSampleName);
		if (stackCount > 1) {
			var output = Mat4f.Create();
			var radius = (float)___entity.FrustumSphereRadius / 10;
			for (var i = 0; i < Math.Min(stackCount - 1, 9); i++) {
				Mat4f.Translate(output, __instance.ModelMat, XOffsets[i] * radius, YOffsets[i] * radius, ZOffsets[i] * radius);
				Mat4f.RotateY(output, output, XOffsets[i] / 4 * radius);
				if (!isShadowPass) {
					standardShaderProgram?.ModelMatrix = output;
				} else {
					render.CurrentActiveShader.UniformMatrix("mvpMatrix", output);
				}

				render.RenderMultiTextureMesh(itemStackRenderInfo.ModelRef, textureSampleName);
			}
		}

		if (!itemStackRenderInfo.CullFaces) {
			render.GlEnableCullFace();
		}

		if (isShadowPass) {
			return false;
		}

		standardShaderProgram?.AddRenderFlags = 0;
		standardShaderProgram?.DamageEffect = 0.0f;
		standardShaderProgram?.Stop();
		return false;
	}
}