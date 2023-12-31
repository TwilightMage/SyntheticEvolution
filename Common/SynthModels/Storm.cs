﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SyntheticEvolution.Common.ChainPhysics;
using SyntheticEvolution.Content.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace SyntheticEvolution.Common.SynthModels;

[SynthModel(Key = "storm")]
public class Storm : SynthModel
{
    private class DrawSpeedState
    {
        public float Age;
        public float Angle;
        public Vector2 Offset;
    }

    public static readonly float LegsArmorScale = 0.2f;
    public static readonly float BladesArmorScale = 0.2f;
    public static readonly float HeadArmorScale = 0.1f;
    public static readonly float ChestArmorScale = 0.3f;

    public static readonly string LegsFeatureDescription = Language.GetTextValue("Mods.SyntheticEvolution.PartDescriptions.Legs");

    public PartSlot HeadSlot => Equipment.GetSlot(0);
    public PartSlot ChestSlot => Equipment.GetSlot(1);
    public PartSlot LeftArmSlot => Equipment.GetSlot(2);
    public PartSlot RightArmSlot => Equipment.GetSlot(3);
    public PartSlot LegsSlot => Equipment.GetSlot(4);
    public PartSlot GrappleSlot => Equipment.GetSlot(5);

    public StormMeleeAttack.AttackTypeEnum LastMeleeAttackType { get; private set; }
    public int MeleeComboCounter => _meleeComboCounter;
    public int[] MeleeAnimationData => _meleeAnimationData;
    public uint LastMeleeAttackTime { get; private set; }
    public int CurrentWeaponUseTime { get; private set; }

    private static Asset<Texture2D> _hotbarSlotTexture;
    private static Asset<Texture2D> _hotbarSlotTextureBorder;

    private static Asset<Texture2D> _characterBodyTexture;
    private static Asset<Texture2D> _characterLegTexture;
    private static Asset<Texture2D> _characterArmTexture;
    private static Asset<Texture2D> _characterHeadTexture;
    private static Asset<Texture2D> _characterLimbTexture;

    private static Asset<Texture2D> _speedEffectTexture;

    public override string Name => "Storm";
    public override bool CanUseConventionalItems => false;
    public override bool HaveCustomHotbar => true;

    public override bool CanWalkInAir => false;

    private int _grappleProjId = -1;
    private float _grappleLengthMax = 16 * 50;
    private float _grappleLengthMin = 16 * 1;
    private Chain _grappleChain = null;

    private int _lastSelectedItem = -1;
    private StormMeleeAttack _weapon;

    private int _meleeComboCounter;
    private int[] _meleeAnimationData = { 0, 0, 0 };

    private List<DrawSpeedState> _drawSpeedStates = new List<DrawSpeedState>();

    private bool _onGround;

    public override PartSlot[] CreateEquipmentSlots()
    {
        return new[]
        {
            new PartSlot("head", PartSlot.SocketTypeEnum.Head, GetType(), new Vector2(0, 0)),
            new PartSlot("chest", PartSlot.SocketTypeEnum.Chest, GetType(), new Vector2(0, 50)),
            new PartSlot("left_arm", PartSlot.SocketTypeEnum.Arm, GetType(), new Vector2(0, 100)),
            new PartSlot("right_arm", PartSlot.SocketTypeEnum.Arm, GetType(), new Vector2(0, 150)),
            new PartSlot("legs", PartSlot.SocketTypeEnum.Legs, GetType(), new Vector2(0, 200)),
            new PartSlot("hook", PartSlot.SocketTypeEnum.Module, null, new Vector2(75, 0), moduleFitCheck: (item) => ContentSamples.ProjectilesByType[item.shoot].aiStyle == ProjAIStyleID.Hook)
        };
    }

    private Vector2? _grappleNormal;
    private Vector2? _clockwiseNormal;
    private Vector2? _swingBoostNormal;
    private bool? _movingClockWise;
    private float? _dot;

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        _grappleNormal = null;
        _clockwiseNormal = null;
        _swingBoostNormal = null;
        _movingClockWise = null;
        _dot = null;

        var player = OwningPlayer;

        _onGround = Collision.SolidTiles(player.BottomLeft + new Vector2(1, 1), player.width - 2, 1, true);

        if (_grappleChain != null)
        {
            if (player.controlUp && !player.controlDown)
            {
                float currentLen = _grappleChain.CalculateLength() * 1.1f;
                float reelIn = MathF.Min(6, currentLen - _grappleLengthMin);

                _grappleChain.DecreaseFromStart(reelIn);
            }
            else if (player.controlDown && !player.controlUp)
            {
                float currentLen = _grappleChain.CalculateLength() * 1.1f;
                float reelOut = MathF.Min(6, _grappleLengthMax - currentLen);

                _grappleChain.IncreaseFromStart(reelOut);
            }

            Vector2 grapplePoint = Main.projectile[_grappleProjId].Center;
            float currentGrappleDistance = player.Center.Distance(grapplePoint);
            float desiredGrappleDistance = _grappleChain.CalculateLength() * 1.1f; // TODO: make separate property for that, not rely on chain length

            if (desiredGrappleDistance < currentGrappleDistance)
            {
                Vector2 grappleNormal = player.Center.DirectionTo(grapplePoint);
                float grappleStrength = (currentGrappleDistance - desiredGrappleDistance) / currentGrappleDistance * 4;
                Vector2 grappleForce = grappleNormal * grappleStrength;

                _grappleNormal = grappleNormal;

                player.velocity += grappleForce;

                if (player.controlLeft || player.controlRight)
                {
                    Vector2 velocityNormal = player.velocity.Normalized();
                    Vector2 clockwiseNormal = new Vector2(grappleNormal.Y, -grappleNormal.X);

                    _clockwiseNormal = clockwiseNormal;

                    // dot == 1  - same
                    // dot == 0  - perpendicular
                    // dot == -1 - opposite
                    bool movingClockWise = Vector2.Dot(velocityNormal, clockwiseNormal) > 0;
                    _movingClockWise = movingClockWise;
                    _dot = Vector2.Dot(velocityNormal, clockwiseNormal);

                    bool shouldControlLeft = movingClockWise;

                    float swingStrength = 0;
                    if (player.controlLeft) swingStrength = shouldControlLeft ? 0.2f : 0.05f;
                    else if (player.controlRight) swingStrength = !shouldControlLeft ? 0.2f : 0.05f;

                    player.velocity += (movingClockWise ? clockwiseNormal : (clockwiseNormal * -1)) * swingStrength;

                    _swingBoostNormal = (movingClockWise ? clockwiseNormal : (clockwiseNormal * -1));

                    if (Main.rand.NextFloat() < 0.05f) _drawSpeedStates.Add(new DrawSpeedState
                    {
                        Angle = Main.rand.NextFloat(-0.3f, 0.3f),
                        Offset = new Vector2(Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3))
                    });
                }
            }
        }

        if (player.selectedItem != _lastSelectedItem)
        {
            if (_weapon?.Projectile.active == true)
            {
                _weapon.Projectile.Kill();
                _weapon = null;
            }

            Item item = player.inventory[player.selectedItem];
            var projectile = Main.projectile[Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<StormMeleeAttack>(), item.damage, item.knockBack, player.whoAmI)];
            _weapon = (StormMeleeAttack)projectile.ModProjectile;
            _weapon.SetupItem(this, item);

            LastMeleeAttackType = _weapon.AttackType;

            _lastSelectedItem = player.selectedItem;
        }
    }

    public override void Update(GameTime deltaTime)
    {
        base.Update(deltaTime);

        for (int i = 0; i < _drawSpeedStates.Count; i++)
        {
            _drawSpeedStates[i].Age = (float)(_drawSpeedStates[i].Age + deltaTime.ElapsedGameTime.TotalSeconds);
            if (_drawSpeedStates[i].Age >= 2) _drawSpeedStates.RemoveAt(i--);
        }
    }

    public override void Grapple()
    {
        base.Grapple();

        if (_grappleProjId >= 0)
        {
            ReelBackGrapple();
            return;
        }

        if (GrappleSlot.TargetItem != null)
        {
            var player = OwningPlayer;
            var item = GrappleSlot.TargetItem;

            SoundEngine.PlaySound(item.UseSound, player.position);

            // TODO: Network sync

            int type = item.shoot;
            float shootSpeed = item.shootSpeed;
            int damage = item.damage;
            float knockBack = item.knockBack;
            Vector2 startPosition = new Vector2(player.position.X + player.width * 0.5f, player.position.Y + player.height * 0.5f);
            Vector2 vectorToMouse = Main.MouseWorld - startPosition;
            if (player.gravDir == -1f)
                vectorToMouse.Y = Main.screenPosition.Y + Main.screenHeight - Main.mouseY - startPosition.Y;
            float distanceToMouse = vectorToMouse.Length();
            float actualShootSpeed;
            if (float.IsNaN(vectorToMouse.X) && float.IsNaN(vectorToMouse.Y) || vectorToMouse.X == 0.0 && vectorToMouse.Y == 0.0)
            {
                vectorToMouse.X = player.direction;
                vectorToMouse.Y = 0.0f;
                actualShootSpeed = shootSpeed;
            }
            else
                actualShootSpeed = shootSpeed / distanceToMouse;

            if (damage == 0) damage = 1;

            Vector2 Speed = vectorToMouse * actualShootSpeed;
            var projId = Projectile.NewProjectile(player.GetSource_ItemUse(item), startPosition, Speed, type, damage, knockBack, Main.myPlayer);
            var hook = Main.projectile[projId];
            hook.friendly = true;

            _grappleLengthMax = 16 * 50;
        }
    }

    public override void GrappleMovement()
    {
        base.GrappleMovement();

        var player = OwningPlayer;

        // Check if player have any vanilla hooks present, then kill them and spawn ours on their place
        if (player.grappling[0] < 0) return;

        var oldHookProj = Main.projectile[player.grappling[0]];

        ReelBackGrapple();
        SpawnGrappleOnTile(oldHookProj.type, oldHookProj.Center);

        player.RemoveAllGrapplingHooks();
    }

    public void ReelBackGrapple()
    {
        if (_grappleProjId < 0) return;

        ((StormHook)Main.projectile[_grappleProjId].ModProjectile).ReelBack();
        _grappleProjId = -1;
        _grappleChain = null;
    }

    public void SpawnGrappleOnTile(int hookType, Vector2 location)
    {
        var player = OwningPlayer;

        (Texture2D chainTexture, Texture2D chainGlowTexture, Color chainGlowColor) = StormHook.GetChainTexture(hookType);
        var hookTexture = TextureAssets.Projectile[hookType].Value;

        _grappleChain = Chain.Create(player.Center, location, chainTexture.Height / StormHook.TextureSizeToSplitAmount(chainTexture.Size().ToPoint()));
        _grappleChain.ScaleSegments(1 / 1.1f);
        _grappleChain.LastPoint.Fixed = true;
        _grappleChain.HoldStart = player.Center;

        _grappleProjId = Projectile.NewProjectile(null, location, Vector2.Zero, ModContent.ProjectileType<StormHook>(), 0, 0, playerId);

        var newHookProj = (StormHook)Main.projectile[_grappleProjId].ModProjectile;

        newHookProj.Setup(_grappleChain, hookTexture, chainTexture, chainGlowTexture, chainGlowColor, hookTexture.Size().ToPoint());
    }

    public void SpawnGrappleOnEnemy(int hookType, NPC enemy, Vector2 locationOffset, float angleOffset)
    {
        var player = OwningPlayer;

        (Texture2D chainTexture, Texture2D chainGlowTexture, Color chainGlowColor) = StormHook.GetChainTexture(hookType);
        var hookTexture = TextureAssets.Projectile[hookType].Value;

        _grappleChain = Chain.Create(player.Center, enemy.Center, chainTexture.Height / StormHook.TextureSizeToSplitAmount(chainTexture.Size().ToPoint()));
        _grappleChain.ScaleSegments(1 / 1.1f);
        _grappleChain.HoldStart = player.Center;

        _grappleProjId = Projectile.NewProjectile(null, enemy.Center, Vector2.Zero, ModContent.ProjectileType<StormHook>(), 0, 0, playerId);

        var newHookProj = (StormHook)Main.projectile[_grappleProjId].ModProjectile;

        newHookProj.Setup(_grappleChain, hookTexture, chainTexture, chainGlowTexture, chainGlowColor, hookTexture.Size().ToPoint());
        newHookProj.SetFollowEnemy(enemy, locationOffset, angleOffset);
    }

    public override bool PreHorizontalMovement()
    {
        if (!_onGround)
        {
            // That makes vanilla code don't slow down character
            OwningPlayer.runSlowdown = 0;
        }

        return true;
    }

    public override void PostHorizontalMovement()
    {
        if (!_onGround)
        {
            // We perform our (proper) drag implementation
            OwningPlayer.velocity *= 0.99f;
        }
    }

    public override void DrawHotbar()
    {
        base.DrawHotbar();

        var player = OwningPlayer;

        _hotbarSlotTexture ??= ModContent.Request<Texture2D>("SyntheticEvolution/Assets/Textures/UI/StormHotbarSlot", AssetRequestMode.ImmediateLoad);
        _hotbarSlotTextureBorder ??= ModContent.Request<Texture2D>("SyntheticEvolution/Assets/Textures/UI/StormHotbarSlotBorder", AssetRequestMode.ImmediateLoad);

        _speedEffectTexture ??= ModContent.Request<Texture2D>("SyntheticEvolution/Assets/Textures/Effects/Speed", AssetRequestMode.ImmediateLoad);

        for (int i = 0; i < 4; i++)
        {
            int slotSize = 42;
            Rectangle rect = new Rectangle(20 + (slotSize + 3) * i, 20, slotSize, slotSize);
            Terraria.Utils.DrawSplicedPanel(Main.spriteBatch, _hotbarSlotTexture.Value, rect.X, rect.Y, rect.Width, rect.Height, 14, 14, 14, 14, Color.White * 0.25f);
            if (i == player.selectedItem) Terraria.Utils.DrawSplicedPanel(Main.spriteBatch, _hotbarSlotTextureBorder.Value, rect.X, rect.Y, rect.Width, rect.Height, 14, 14, 14, 14, Main.OurFavoriteColor);
            ItemSlot.DrawItemIcon(player.inventory[i], 0, Main.spriteBatch, rect.Center(), 1f, 28f, i == player.selectedItem ? Color.White : (Color.White * 0.75f));
        }

        Vector2 center = Main.screenPosition + Main.ScreenSize.ToVector2() / 2;

        //Terraria.Utils.DrawLine(Main.spriteBatch, center, center + player.velocity * 32, Color.Red, Color.White, 2);
        //if (_grappleNormal.HasValue) Terraria.Utils.DrawLine(Main.spriteBatch, center, center + _grappleNormal.Value * 32, Color.Blue, Color.White, 2);
        ////if (_clockwiseNormal.HasValue) Terraria.Utils.DrawLine(Main.spriteBatch, center, center + _clockwiseNormal.Value * 32, Color.Orange, Color.White, 2);
        //if (_swingBoostNormal.HasValue) Terraria.Utils.DrawLine(Main.spriteBatch, center, center + _swingBoostNormal.Value * 32, Color.Magenta, Color.White, 2);
        //if (_movingClockWise.HasValue) Main.spriteBatch.DrawString(FontAssets.MouseText.Value, _movingClockWise.Value ? "left" : "right", new Vector2(700, 200), Color.Magenta);
        //if (_dot.HasValue) Main.spriteBatch.DrawString(FontAssets.MouseText.Value, _dot.Value.ToString(), new Vector2(700, 230), Color.Magenta);

        // TODO: that should not be in UI code
        Vector2 velocityNormal = player.velocity.Normalized();
        foreach (var drawSpeedState in _drawSpeedStates)
        {
            Main.spriteBatch.Draw(_speedEffectTexture.Value, Main.ScreenSize.ToVector2() / 2 + drawSpeedState.Offset, null, Color.White * 0.1f * (1 - MathF.Pow(drawSpeedState.Age / 2, 5)), MathF.Atan2(velocityNormal.Y, velocityNormal.X) + drawSpeedState.Angle, new Vector2(13, 27), Vector2.One, SpriteEffects.None, 0);
        }
    }

    public override bool StartUseItem(Item item)
    {
        var player = OwningPlayer;

        if (item.CountsAsClass(DamageClass.Melee) && item.pick == 0 && item.axe == 0 && item.hammer == 0)
        {
            if ((Main.GameUpdateCount - LastMeleeAttackTime) >= CurrentWeaponUseTime)
            {
                bool isGravediggerShovel = item.type == 4711;
                if (item.pick > 0 || item.axe > 0 || item.hammer > 0 || isGravediggerShovel)
                    player.toolTime = 1;
                player.StartChanneling(item);
                player.attackCD = 0;
                player.ResetMeleeHitCooldowns();

                //player.ApplyItemAnimation(item);
                //player.SetItemAnimation(CombinedHooks.TotalAnimationTime(item.useAnimation, player, item));
                player.reuseDelay = (int)(item.reuseDelay / CombinedHooks.TotalUseSpeedMultiplier(player, item));
                //player.ItemUsesThisAnimation = 0;

                _weapon.DoAttack((Main.MouseWorld - player.Center).Normalized(), ref _meleeComboCounter, ref _meleeAnimationData);
                CurrentWeaponUseTime = _weapon.AttackFrameCount;

                LastMeleeAttackTime = Main.GameUpdateCount;

                if (item.UseSound.HasValue && !ItemID.Sets.SkipsInitialUseSound[item.type])
                {
                    SoundEngine.PlaySound(item.UseSound, player.Center);
                }
            }

            return true;
        }

        return base.StartUseItem(item);
    }

    public override bool DrawPlayer(ref PlayerDrawSet drawSet)
    {
        return false;

        _characterBodyTexture ??= ModContent.Request<Texture2D>("SyntheticEvolution/Assets/Textures/Characters/Storm/Torso");
        _characterLegTexture ??= ModContent.Request<Texture2D>("SyntheticEvolution/Assets/Textures/Characters/Storm/Leg");
        _characterArmTexture ??= ModContent.Request<Texture2D>("SyntheticEvolution/Assets/Textures/Characters/Storm/Arm");
        _characterHeadTexture ??= ModContent.Request<Texture2D>("SyntheticEvolution/Assets/Textures/Characters/Storm/Head");
        _characterLimbTexture ??= ModContent.Request<Texture2D>("SyntheticEvolution/Assets/Textures/Characters/Storm/Limb");

        //drawSet = new PlayerDrawSet();
        //drawSet.DrawDataCache = new List<DrawData>();

        drawSet.DrawDataCache.Add(new DrawData(_characterBodyTexture.Value, _characterBodyTexture.Value.Bounds, Color.White));
        drawSet.cBody = 0;

        return true;
    }
}