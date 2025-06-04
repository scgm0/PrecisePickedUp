using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace PrecisePickedUp;

public class MultiEntityItemRenderer : EntityItemRenderer {
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

	private EntityItem entityItem;
	private long touchGroundMs;
	public new float[] ModelMat = Mat4f.Create();
	private float scaleRand;
	private float yRotRand;
	private Vec3d lerpedPos;
	private ItemSlot inslot;
	private float accum;
	private Vec4f particleOutTransform = new();
	private Vec4f glowRgb = new();
	private bool rotateWhenFalling;
	private float xAngle;
	private float yAngle;
	private float zAngle;

	public MultiEntityItemRenderer(Entity entity, ICoreClientAPI api)
		: base(entity, api) {
		entityItem = (EntityItem)entity;
		inslot = entityItem.Slot;
		rotateWhenFalling = inslot.Itemstack?.Collectible?.Attributes?[nameof(rotateWhenFalling)].AsBool(true) ?? true;
		scaleRand = (float)(api.World.Rand.NextDouble() / 20.0 - 0.02500000037252903);
		touchGroundMs = entityItem.itemSpawnedMilliseconds - api.World.Rand.Next(5000);
		yRotRand = (float)api.World.Rand.NextDouble() * 6.2831855f;
		lerpedPos = entity.Pos.XYZ;
	}

	public override void DoRender3DOpaque(float dt, bool isShadowPass) {
		var stackCount = entityItem.WatchedAttributes.GetInt("stackCount", inslot.Itemstack?.StackSize ?? 0);

		if (stackCount <= 0) {
			return;
		}

		if (isShadowPass && !entity.IsRendered) {
			return;
		}

		if (RunWittySkipRenderAlgorithm) {
			var x = (int)entity.Pos.X;
			var y = (int)entity.Pos.Y;
			var z = (int)entity.Pos.Z;
			var num = (entityItem.Itemstack.Class == EnumItemClass.Block ? -1 : 1) * entityItem.Itemstack.Id;
			if (LastPos.X == x && LastPos.Y == y && LastPos.Z == z &&
				LastCollectibleId == num) {
				if (entity.EntityId % RenderModulo != 0L) {
					return;
				}
			} else {
				LastPos.Set(x, y, z);
			}

			LastCollectibleId = num;
		}

		var render = capi.Render;
		lerpedPos.X += (entity.Pos.X - lerpedPos.X) * 22.0 * dt;
		lerpedPos.Y += (entity.Pos.Y - lerpedPos.Y) * 22.0 * dt;
		lerpedPos.Z += (entity.Pos.Z - lerpedPos.Z) * 22.0 * dt;
		var itemStackRenderInfo = render.GetItemStackRenderInfo(inslot, EnumItemRenderTarget.Ground, dt);
		if (itemStackRenderInfo.ModelRef == null || itemStackRenderInfo.Transform == null) {
			return;
		}

		IStandardShaderProgram standardShaderProgram = null;
		LoadModelMatrix(itemStackRenderInfo, isShadowPass, dt);
		var textureSampleName = "tex";
		if (isShadowPass) {
			textureSampleName = "tex2d";
			var numArray = Mat4f.Mul(ModelMat, capi.Render.CurrentModelviewMatrix, ModelMat);
			Mat4f.Mul(numArray, capi.Render.CurrentProjectionMatrix, numArray);
			render.CurrentActiveShader.UniformMatrix("mvpMatrix", numArray);
			render.CurrentActiveShader.Uniform("origin", new Vec3f());
		} else {
			standardShaderProgram = render.StandardShader;
			standardShaderProgram.Use();
			standardShaderProgram.RgbaTint = entity.Swimming ? new(0.5f, 0.5f, 0.5f, 1f) : ColorUtil.WhiteArgbVec;
			standardShaderProgram.DontWarpVertices = 0;
			standardShaderProgram.NormalShaded = 1;
			standardShaderProgram.AlphaTest = itemStackRenderInfo.AlphaTest;
			standardShaderProgram.DamageEffect = itemStackRenderInfo.DamageEffect;
			if (entity.Swimming) {
				standardShaderProgram.AddRenderFlags =
					(entityItem.Itemstack.Collectible.MaterialDensity <= 1000 ? 1 : 0) << 12;
				standardShaderProgram.WaterWaveCounter = capi.Render.ShaderUniforms.WaterWaveCounter;
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
				var textureAtlasPosition = render.GetTextureAtlasPosition(entityItem.Slot.Itemstack);
				standardShaderProgram.BaseUvOrigin = new(textureAtlasPosition.x1, textureAtlasPosition.y1);
			}

			var asBlockPos = entityItem.Pos.AsBlockPos;
			var lightRgBs = capi.World.BlockAccessor.GetLightRGBs(asBlockPos.X, asBlockPos.Y, asBlockPos.Z);
			var temperature =
				(int)entityItem.Slot.Itemstack.Collectible.GetTemperature(capi.World,
					entityItem.Slot.Itemstack);
			var incandescenceColorAsColor4F = ColorUtil.GetIncandescenceColorAsColor4f(temperature);
			var num = GameMath.Clamp((temperature - 550) / 2, 0, byte.MaxValue);
			glowRgb.R = incandescenceColorAsColor4F[0];
			glowRgb.G = incandescenceColorAsColor4F[1];
			glowRgb.B = incandescenceColorAsColor4F[2];
			glowRgb.A = num / (float)byte.MaxValue;
			standardShaderProgram.ExtraGlow = num;
			standardShaderProgram.RgbaAmbientIn = render.AmbientColor;
			standardShaderProgram.RgbaLightIn = lightRgBs;
			standardShaderProgram.RgbaGlowIn = glowRgb;
			standardShaderProgram.RgbaFogIn = render.FogColor;
			standardShaderProgram.FogMinIn = render.FogMin;
			standardShaderProgram.FogDensityIn = render.FogDensity;
			standardShaderProgram.ExtraGodray = 0.0f;
			standardShaderProgram.NormalShaded = itemStackRenderInfo.NormalShaded ? 1 : 0;
			standardShaderProgram.ProjectionMatrix = render.CurrentProjectionMatrix;
			standardShaderProgram.ViewMatrix = render.CameraMatrixOriginf;
			standardShaderProgram.ModelMatrix = ModelMat;
			var itemStack = entityItem.Slot.Itemstack;
			var particleProperties = itemStack.Block?.ParticleProperties;
			if (itemStack.Block != null && !capi.IsGamePaused) {
				Mat4f.MulWithVec4(ModelMat,
					new Vec4f(itemStack.Block.TopMiddlePos.X,
						itemStack.Block.TopMiddlePos.Y - 0.4f,
						itemStack.Block.TopMiddlePos.Z - 0.5f,
						0.0f),
					particleOutTransform);
				accum += dt;
				if (particleProperties != null && particleProperties.Length != 0 && accum > 0.02500000037252903) {
					accum %= 0.025f;
					foreach (var particlePropertiesProvider in particleProperties) {
						particlePropertiesProvider.basePos.X = particleOutTransform.X + entity.Pos.X;
						particlePropertiesProvider.basePos.Y = particleOutTransform.Y + entity.Pos.Y;
						particlePropertiesProvider.basePos.Z = particleOutTransform.Z + entity.Pos.Z;
						entityItem.World.SpawnParticles(particlePropertiesProvider);
					}
				}
			}
		}

		if (!itemStackRenderInfo.CullFaces)
			render.GlDisableCullFace();
		render.RenderMultiTextureMesh(itemStackRenderInfo.ModelRef, textureSampleName);
		if (stackCount > 1) {
			var output = Mat4f.Create();
			var radius = (float)entityItem.FrustumSphereRadius * 0.1f;
			for (var i = 0; i < Math.Min(stackCount - 1, 9); i++) {
				Mat4f.Translate(output, ModelMat, XOffsets[i] * radius , YOffsets[i] * radius, ZOffsets[i] * radius);
				Mat4f.RotateY(output, output, XOffsets[i] / 4 * radius);
				if (!isShadowPass) {
					standardShaderProgram.ModelMatrix = output;
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
			return;
		}

		standardShaderProgram.DamageEffect = 0.0f;
		standardShaderProgram.Stop();
	}

	private void LoadModelMatrix(ItemRenderInfo renderInfo, bool isShadowPass, float dt) {
		var playerEntity = capi.World.Player.Entity;
		Mat4f.Identity(ModelMat);
		Mat4f.Translate(ModelMat,
			ModelMat,
			(float)(lerpedPos.X - playerEntity.CameraPos.X),
			(float)(lerpedPos.Y - playerEntity.CameraPos.Y),
			(float)(lerpedPos.Z - playerEntity.CameraPos.Z));
		var num1 = 0.2f * renderInfo.Transform.ScaleXYZ.X;
		var num2 = 0.2f * renderInfo.Transform.ScaleXYZ.Y;
		var num3 = 0.2f * renderInfo.Transform.ScaleXYZ.Z;
		var num4 = 0.0f;
		var num5 = 0.0f;
		if (!isShadowPass) {
			var elapsedMilliseconds = capi.World.ElapsedMilliseconds;
			var flag = !entity.Collided && !entity.Swimming && !capi.IsGamePaused;
			if (!flag)
				touchGroundMs = elapsedMilliseconds;
			if (entity.Collided) {
				xAngle *= 0.55f;
				yAngle *= 0.55f;
				zAngle *= 0.55f;
			} else if (rotateWhenFalling) {
				float num6 = Math.Min(1L, (elapsedMilliseconds - touchGroundMs) / 200L);
				var num7 = flag ? (float)(1000.0 * dt / 7.0) * num6 : 0.0f;
				yAngle += num7;
				xAngle += num7;
				zAngle += num7;
			}

			if (entity.Swimming) {
				var num8 = 1f;
				if (entityItem.Itemstack.Collectible.MaterialDensity > 1000) {
					num4 = GameMath.Sin(elapsedMilliseconds / 1000f) / 50f;
					num5 = (float)(-(double)GameMath.Sin(elapsedMilliseconds / 3000f) / 50.0);
					num8 = 0.1f;
				}

				xAngle = GameMath.Sin(elapsedMilliseconds / 1000f) * 8f * num8;
				yAngle = GameMath.Cos(elapsedMilliseconds / 2000f) * 3f * num8;
				zAngle = (float)(-(double)GameMath.Sin(elapsedMilliseconds / 3000f) * 8.0) * num8;
			}
		}

		Mat4f.Translate(ModelMat,
			ModelMat,
			num4 + renderInfo.Transform.Translation.X,
			renderInfo.Transform.Translation.Y,
			num5 + renderInfo.Transform.Translation.Z);
		Mat4f.Scale(ModelMat,
			ModelMat,
			new float[3] {
				num1 + scaleRand,
				num2 + scaleRand,
				num3 + scaleRand
			});
		Mat4f.RotateY(ModelMat,
			ModelMat,
			(float)(Math.PI / 180.0 * (renderInfo.Transform.Rotation.Y + (double)yAngle) +
				(renderInfo.Transform.Rotate ? yRotRand : 0.0)));
		Mat4f.RotateZ(ModelMat,
			ModelMat,
			(float)(Math.PI / 180.0 * (renderInfo.Transform.Rotation.Z + (double)zAngle)));
		Mat4f.RotateX(ModelMat,
			ModelMat,
			(float)(Math.PI / 180.0 * (renderInfo.Transform.Rotation.X + (double)xAngle)));
		Mat4f.Translate(ModelMat,
			ModelMat,
			-renderInfo.Transform.Origin.X,
			-renderInfo.Transform.Origin.Y,
			-renderInfo.Transform.Origin.Z);
	}
}