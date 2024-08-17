using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace PrecisePickedUp;

public class MultiEntityItemRenderer : EntityRenderer {
	public static float[] XOffsets = [
		-0.37f,
		-0.24f,
		0.31f,
		-0.28f,
		-0.25f,
		0.22f,
		-0.19f,
		0.16f,
		-0.13f,
		0.1f
	];

	public static float[] YOffsets = [
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

	public static float[] ZOffsets = [
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

	private readonly EntityItem _entityItem;
	private long _touchGroundMs;
	public readonly float[] ModelMat = Mat4f.Create();
	private readonly float _scaleRand;
	private readonly float _yRotRand;
	private readonly Vec3d _lerpedPos;
	private readonly ItemSlot _inslot;
	private float _accum;
	private readonly Vec4f _particleOutTransform = new();
	private readonly Vec4f _glowRgb = new();
	private bool _rotateWhenFalling;
	private float _xAngle;
	private float _yAngle;
	private float _zAngle;

	public MultiEntityItemRenderer(Entity entity, ICoreClientAPI api)
		: base(entity, api) {
		_entityItem = (EntityItem)entity;
		_inslot = _entityItem.Slot;
		_rotateWhenFalling = _inslot.Itemstack?.Collectible?.Attributes?[nameof(_rotateWhenFalling)].AsBool(true) ?? true;
		_scaleRand = (float)(api.World.Rand.NextDouble() / 20.0 - 0.02500000037252903);
		_touchGroundMs = _entityItem.itemSpawnedMilliseconds - api.World.Rand.Next(5000);
		_yRotRand = (float)api.World.Rand.NextDouble() * 6.2831855f;
		_lerpedPos = entity.Pos.XYZ;
	}

	public override void DoRender3DOpaque(float dt, bool isShadowPass) {
		var stackCount = _entityItem.WatchedAttributes.GetInt("stackCount", _entityItem.Itemstack.StackSize);
		if (stackCount == 0) return;
		if (_entityItem.Itemstack.StackSize != stackCount) {
			_entityItem.Itemstack.StackSize = stackCount;
			entity.IsRendered = true;
		}
		if (isShadowPass && !entity.IsRendered)
			return;

		if (EntityItemRenderer.RunWittySkipRenderAlgorithm) {
			var x = (int)entity.Pos.X;
			var y = (int)entity.Pos.Y;
			var z = (int)entity.Pos.Z;
			var num = (_entityItem.Itemstack.Class == EnumItemClass.Block ? -1 : 1) * _entityItem.Itemstack.Id;
			if (EntityItemRenderer.LastPos.X == x && EntityItemRenderer.LastPos.Y == y && EntityItemRenderer.LastPos.Z == z &&
				EntityItemRenderer.LastCollectibleId == num) {
				if (entity.EntityId % EntityItemRenderer.RenderModulo != 0L)
					return;
			} else
				EntityItemRenderer.LastPos.Set(x, y, z);

			EntityItemRenderer.LastCollectibleId = num;
		}

		var render = capi.Render;
		_lerpedPos.X += (entity.Pos.X - _lerpedPos.X) * 22.0 * dt;
		_lerpedPos.Y += (entity.Pos.Y - _lerpedPos.Y) * 22.0 * dt;
		_lerpedPos.Z += (entity.Pos.Z - _lerpedPos.Z) * 22.0 * dt;
		var itemStackRenderInfo = render.GetItemStackRenderInfo(_inslot, EnumItemRenderTarget.Ground, dt);
		if (itemStackRenderInfo.ModelRef == null || itemStackRenderInfo.Transform == null)
			return;
		IStandardShaderProgram standardShaderProgram = null;
		LoadModelMatrix(itemStackRenderInfo, isShadowPass, dt);
		var textureSampleName = "tex";
		if (isShadowPass) {
			textureSampleName = "tex2d";
			var numArray = Mat4f.Mul(ModelMat, capi.Render.CurrentModelviewMatrix, ModelMat);
			Mat4f.Mul(numArray, capi.Render.CurrentProjectionMatrix, numArray);
			capi.Render.CurrentActiveShader.UniformMatrix("mvpMatrix", numArray);
			capi.Render.CurrentActiveShader.Uniform("origin", new Vec3f());
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
					(_entityItem.Itemstack.Collectible.MaterialDensity <= 1000 ? 1 : 0) << 12;
				standardShaderProgram.WaterWaveCounter = capi.Render.ShaderUniforms.WaterWaveCounter;
			} else
				standardShaderProgram.AddRenderFlags = 0;

			standardShaderProgram.OverlayOpacity = itemStackRenderInfo.OverlayOpacity;
			if (itemStackRenderInfo.OverlayTexture != null && itemStackRenderInfo.OverlayOpacity > 0.0) {
				standardShaderProgram.Tex2dOverlay2D = itemStackRenderInfo.OverlayTexture.TextureId;
				standardShaderProgram.OverlayTextureSize = new(itemStackRenderInfo.OverlayTexture.Width,
					itemStackRenderInfo.OverlayTexture.Height);
				standardShaderProgram.BaseTextureSize = new(itemStackRenderInfo.TextureSize.Width,
					itemStackRenderInfo.TextureSize.Height);
				var textureAtlasPosition = render.GetTextureAtlasPosition(_entityItem.Itemstack);
				standardShaderProgram.BaseUvOrigin = new(textureAtlasPosition.x1, textureAtlasPosition.y1);
			}

			var asBlockPos = _entityItem.Pos.AsBlockPos;
			var lightRgBs = capi.World.BlockAccessor.GetLightRGBs(asBlockPos.X, asBlockPos.Y, asBlockPos.Z);
			var temperature =
				(int)_entityItem.Itemstack.Collectible.GetTemperature(capi.World,
					_entityItem.Itemstack);
			var incandescenceColorAsColor4F = ColorUtil.GetIncandescenceColorAsColor4f(temperature);
			var num = GameMath.Clamp((temperature - 550) / 2, 0, byte.MaxValue);
			_glowRgb.R = incandescenceColorAsColor4F[0];
			_glowRgb.G = incandescenceColorAsColor4F[1];
			_glowRgb.B = incandescenceColorAsColor4F[2];
			_glowRgb.A = num / (float)byte.MaxValue;
			standardShaderProgram.ExtraGlow = num;
			standardShaderProgram.RgbaAmbientIn = render.AmbientColor;
			standardShaderProgram.RgbaLightIn = lightRgBs;
			standardShaderProgram.RgbaGlowIn = _glowRgb;
			standardShaderProgram.RgbaFogIn = render.FogColor;
			standardShaderProgram.FogMinIn = render.FogMin;
			standardShaderProgram.FogDensityIn = render.FogDensity;
			standardShaderProgram.ExtraGodray = 0.0f;
			standardShaderProgram.NormalShaded = itemStackRenderInfo.NormalShaded ? 1 : 0;
			standardShaderProgram.ProjectionMatrix = render.CurrentProjectionMatrix;
			standardShaderProgram.ViewMatrix = render.CameraMatrixOriginf;
			standardShaderProgram.ModelMatrix = ModelMat;
			var itemStack = _entityItem.Itemstack;
			var particleProperties = itemStack.Block?.ParticleProperties;
			if (itemStack.Block != null && !capi.IsGamePaused) {
				Mat4f.MulWithVec4(ModelMat,
					new Vec4f(itemStack.Block.TopMiddlePos.X,
						itemStack.Block.TopMiddlePos.Y - 0.4f,
						itemStack.Block.TopMiddlePos.Z - 0.5f,
						0.0f),
					_particleOutTransform);
				_accum += dt;
				if (particleProperties != null && particleProperties.Length != 0 && _accum > 0.02500000037252903) {
					_accum %= 0.025f;
					foreach (var particlePropertiesProvider in particleProperties) {
						particlePropertiesProvider.basePos.X = _particleOutTransform.X + entity.Pos.X;
						particlePropertiesProvider.basePos.Y = _particleOutTransform.Y + entity.Pos.Y;
						particlePropertiesProvider.basePos.Z = _particleOutTransform.Z + entity.Pos.Z;
						_entityItem.World.SpawnParticles(particlePropertiesProvider);
					}
				}
			}
		}

		if (!itemStackRenderInfo.CullFaces)
			render.GlDisableCullFace();
		if (stackCount > 1) {
			var output = Mat4f.Create();
			for (var i = 0; i < Math.Min(stackCount, 10); i++) {
				Mat4f.Translate(output, ModelMat, XOffsets[i], YOffsets[i], ZOffsets[i]);
				Mat4f.RotateY(output, output, ZOffsets[i] / 4);
				if (standardShaderProgram != null) standardShaderProgram.ModelMatrix = output;
				render.RenderMultiTextureMesh(itemStackRenderInfo.ModelRef, textureSampleName);
			}
		} else {
			render.RenderMultiTextureMesh(itemStackRenderInfo.ModelRef, textureSampleName);
		}

		if (!itemStackRenderInfo.CullFaces)
			render.GlEnableCullFace();
		if (isShadowPass)
			return;
		standardShaderProgram.DamageEffect = 0.0f;
		standardShaderProgram.Stop();
	}

	private void LoadModelMatrix(ItemRenderInfo renderInfo, bool isShadowPass, float dt) {
		var playerEntity = capi.World.Player.Entity;
		Mat4f.Identity(ModelMat);
		Mat4f.Translate(ModelMat,
			ModelMat,
			(float)(_lerpedPos.X - playerEntity.CameraPos.X),
			(float)(_lerpedPos.Y - playerEntity.CameraPos.Y),
			(float)(_lerpedPos.Z - playerEntity.CameraPos.Z));
		var num1 = 0.2f * renderInfo.Transform.ScaleXYZ.X;
		var num2 = 0.2f * renderInfo.Transform.ScaleXYZ.Y;
		var num3 = 0.2f * renderInfo.Transform.ScaleXYZ.Z;
		var num4 = 0.0f;
		var num5 = 0.0f;
		if (!isShadowPass) {
			var elapsedMilliseconds = capi.World.ElapsedMilliseconds;
			var flag = !entity.Collided && !entity.Swimming && !capi.IsGamePaused;
			if (!flag)
				_touchGroundMs = elapsedMilliseconds;
			if (entity.Collided) {
				_xAngle *= 0.55f;
				_yAngle *= 0.55f;
				_zAngle *= 0.55f;
			} else if (_rotateWhenFalling) {
				float num6 = Math.Min(1L, (elapsedMilliseconds - _touchGroundMs) / 200L);
				var num7 = flag ? (float)(1000.0 * dt / 7.0) * num6 : 0.0f;
				_yAngle += num7;
				_xAngle += num7;
				_zAngle += num7;
			}

			if (entity.Swimming) {
				var num8 = 1f;
				if (_entityItem.Itemstack.Collectible.MaterialDensity > 1000) {
					num4 = GameMath.Sin(elapsedMilliseconds / 1000f) / 50f;
					num5 = (float)(-(double)GameMath.Sin(elapsedMilliseconds / 3000f) / 50.0);
					num8 = 0.1f;
				}

				_xAngle = GameMath.Sin(elapsedMilliseconds / 1000f) * 8f * num8;
				_yAngle = GameMath.Cos(elapsedMilliseconds / 2000f) * 3f * num8;
				_zAngle = (float)(-(double)GameMath.Sin(elapsedMilliseconds / 3000f) * 8.0) * num8;
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
				num1 + _scaleRand,
				num2 + _scaleRand,
				num3 + _scaleRand
			});
		Mat4f.RotateY(ModelMat,
			ModelMat,
			(float)(Math.PI / 180.0 * (renderInfo.Transform.Rotation.Y + (double)_yAngle) +
				(renderInfo.Transform.Rotate ? _yRotRand : 0.0)));
		Mat4f.RotateZ(ModelMat,
			ModelMat,
			(float)(Math.PI / 180.0 * (renderInfo.Transform.Rotation.Z + (double)_zAngle)));
		Mat4f.RotateX(ModelMat,
			ModelMat,
			(float)(Math.PI / 180.0 * (renderInfo.Transform.Rotation.X + (double)_xAngle)));
		Mat4f.Translate(ModelMat,
			ModelMat,
			-renderInfo.Transform.Origin.X,
			-renderInfo.Transform.Origin.Y,
			-renderInfo.Transform.Origin.Z);
	}

	public override void Dispose() { }
}