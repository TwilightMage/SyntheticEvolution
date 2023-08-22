using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SyntheticEvolution.Common;
using SyntheticEvolution.Common.SynthModels;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace SyntheticEvolution.Content.Projectiles;

public class StormMeleeAttack : ModProjectile
{
    public enum AttackTypeEnum
    {
        BladeSwing
    }

    public enum AnimationStateEnum
    {
        Sheathed,
        Attack,
        Raise,
        Descend,
        Push
    }

    // Item specific
    private Item _item;
    private Texture2D _itemTexture;
    private Vector2 _directionForward;
    private Vector2 _directionLeft;

    // State machine
    public AttackTypeEnum AttackType { get; private set; } = AttackTypeEnum.BladeSwing;
    public AnimationStateEnum AnimationState { get; private set; } = AnimationStateEnum.Sheathed;
    private float _animationStateTime;
    public AnimationStateEnum PrevAnimationState { get; private set; } = AnimationStateEnum.Sheathed;
    private float _prevAnimationStateTime;
    private float _stateTransitionTime;

    // Settings
    public bool OneHitPerEnemy;

    // State
    public int ComboCounter;

    private bool _isEvenCombo;

    // Attack state
    public int AttackFrameCount { get; private set; }
    public int ComboBreakFrameCount { get; private set; }

    // Any damaging state
    private List<NPC> _npcHit = new List<NPC>();
    private bool _isInAttack;

    public override void SetDefaults()
    {
        base.SetDefaults();

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
    }

    public void SetupItem(Storm storm, Item item, bool oneHitPerEnemy = true)
    {
        var player = storm.OwningPlayer;

        AttackType = AttackTypeEnum.BladeSwing;
        OneHitPerEnemy = oneHitPerEnemy;

        _item = item;
        _itemTexture = TextureAssets.Item[item.type].Value;

        Projectile.width = (int)(_itemTexture.Size().Length() * player.GetAdjustedItemScale(item) * 0.5f);
        Projectile.height = (int)(_itemTexture.Size().Length() * player.GetAdjustedItemScale(item) * 0.3f * 0.5f);

        (Projectile.Center, Projectile.rotation, Projectile.scale) = AnimateSheathed(player);
    }

    public void DoAttack(Vector2 direction, ref int stormMeleeComboCounter, ref int[] stormMeleeAnimationData)
    {
        Player player = Main.player[Projectile.owner];
        Storm storm = (Storm)player.GetSynth();

        player.ChangeDir(direction.X > 0 ? 1 : -1);

        AttackFrameCount = (int)(CombinedHooks.TotalAnimationTime(_item.useAnimation, player, _item) * 0.5f);
        ComboBreakFrameCount = AttackFrameCount + 20;

        _directionForward = direction;
        _directionLeft = new Vector2(_directionForward.Y, -_directionForward.X);

        if (AttackType == AttackTypeEnum.BladeSwing)
        {
            _isEvenCombo = stormMeleeAnimationData[0] == 1;

            if (storm.LastMeleeAttackType == AttackTypeEnum.BladeSwing && Main.GameUpdateCount - storm.LastMeleeAttackTime < ComboBreakFrameCount)
            {
                ComboCounter = stormMeleeComboCounter % 3;

                if (ComboCounter == 0)
                {
                    _isEvenCombo = !_isEvenCombo;
                }
            }
            else
            {
                ComboCounter = 0;
                _isEvenCombo = false;
            }

            stormMeleeComboCounter = ComboCounter + 1;

            Projectile.CritChance = ComboCounter switch
            {
                0 => player.GetWeaponCrit(_item),
                1 => (int)MathF.Round(player.GetWeaponCrit(_item) * 1.15f),
                2 => (int)MathF.Round(player.GetWeaponCrit(_item) * 1.3f)
            };

            Projectile.damage = (int)(player.GetWeaponDamage(_item) * 0.5f);
            Projectile.knockBack = (int)(player.GetWeaponKnockback(_item) * 0.5f);

            stormMeleeAnimationData[0] = _isEvenCombo ? 1 : 0;
        }

        _isInAttack = true;
        _npcHit.Clear();

        SetState(AnimationStateEnum.Attack);
    }

    private void SetState(AnimationStateEnum newState)
    {
        PrevAnimationState = AnimationState;
        _prevAnimationStateTime = _animationStateTime;

        AnimationState = newState;
        _animationStateTime = 0;

        _stateTransitionTime = 0;
    }

    public override bool? CanHitNPC(NPC target)
    {
        if (!_isInAttack) return false;

        if (OneHitPerEnemy && _npcHit.Contains(target)) return false;

        if (Projectile.TryGetOwner(out Player player))
        {
            bool? flag = CombinedHooks.CanPlayerHitNPCWithItem(player, _item, target);
            if (flag.HasValue) return flag;
        }

        return base.CanHitNPC(target);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        base.ModifyHitNPC(target, ref modifiers);

        if (Projectile.TryGetOwner(out Player player))
        {
            CombinedHooks.ModifyPlayerHitNPCWithItem(player, _item, target, ref modifiers);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);

        _npcHit.Add(target);

        if (Projectile.TryGetOwner(out Player player))
        {
            CombinedHooks.OnPlayerHitNPCWithItem(player, _item, target, hit, damageDone);

            ProcessVanillaHitNPC(player, target.whoAmI, _item);
        }
    }

    private void ProcessVanillaHitNPC(Player player, int npcIndex, Item item)
    {
        NPC npc = Main.npc[npcIndex];
        if (player.parryDamageBuff && item.CountsAsClass(DamageClass.Melee))
        {
            player.parryDamageBuff = false;
            player.ClearBuff(BuffID.ParryDamageBuff);
        }

        Vector2 hitPoint = npc.Hitbox.ClosestPointInRect(Projectile.Center);
        switch (item.type)
        {
            case ItemID.Keybrand:
                ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings() { PositionInWorld = hitPoint }, player.whoAmI);
                break;
            case ItemID.SlapHand:
                ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.SlapHand, new ParticleOrchestraSettings() { PositionInWorld = hitPoint }, player.whoAmI);
                break;
            case ItemID.WaffleIron:
                ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.WaffleIron, new ParticleOrchestraSettings() { PositionInWorld = hitPoint }, player.whoAmI);
                break;
            case ItemID.Flymeal:
                ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.FlyMeal, new ParticleOrchestraSettings() { PositionInWorld = npc.Center }, player.whoAmI);
                break;
        }

        player.StatusToNPC(item.type, npcIndex);
    }

    public override void AI()
    {
        base.AI();

        Player player = Projectile.TryGetOwner(out Player owner) ? owner : null;
        Storm storm = player?.GetSynth() as Storm ?? null;

        if (player == null || storm == null)
        {
            Projectile.Kill();
            return;
        }

        if (_isInAttack && Main.GameUpdateCount - storm.LastMeleeAttackTime > AttackFrameCount)
        {
            _isInAttack = false;
        }

        if (AnimationState == AnimationStateEnum.Attack && Main.GameUpdateCount - storm.LastMeleeAttackTime > ComboBreakFrameCount)
        {
            SetState(AnimationStateEnum.Sheathed);
        }

        DoAnimation(player);

        Projectile.timeLeft = 3600;
    }

    private void DoAnimation(Player player)
    {
        if (AnimationState != PrevAnimationState)
        {
            _stateTransitionTime += 1 / 60f;
            _animationStateTime += 1 / 60f;
            _prevAnimationStateTime += 1 / 60f;
            (Vector2 newCenter, float newRotation, float newScale) = Animate(player, AnimationState, _animationStateTime);
            (Vector2 oldCenter, float oldRotation, float oldScale) = Animate(player, PrevAnimationState, _prevAnimationStateTime);

            float stateTransitionAlpha = MathHelper.Clamp(_stateTransitionTime, 0, 0.2f) / 0.2f;

            Projectile.Center = Vector2.Lerp(oldCenter, newCenter, stateTransitionAlpha);
            Projectile.rotation = MathHelper.Lerp(oldRotation, newRotation, stateTransitionAlpha);
            Projectile.scale = MathHelper.Lerp(oldScale, newScale, stateTransitionAlpha);

            if (stateTransitionAlpha >= 1)
            {
                PrevAnimationState = AnimationState;
            }
        }
        else
        {
            _animationStateTime += 1 / 60f;
            (Projectile.Center, Projectile.rotation, Projectile.scale) = Animate(player, AnimationState, _animationStateTime);
        }
    }

    private (Vector2 center, float rotation, float scale) Animate(Player player, AnimationStateEnum state, float time) => state switch
    {
        AnimationStateEnum.Sheathed => AnimateSheathed(player),
        AnimationStateEnum.Attack => AnimateAttack(player, time)
    };

    private (Vector2 center, float rotation, float scale) AnimateAttack(Player player, float time)
    {
        float alpha = MathF.Min(time / (AttackFrameCount / 60f), 1);

        float handLength = player.height / 2f;

        switch (AttackType)
        {
            case AttackTypeEnum.BladeSwing:
                float swingSine = MathF.Sin(2 * alpha * MathF.PI);
                float swingArc = MathF.Sin(alpha * MathF.PI);
                int subAnim = ComboCounter;
                if (_isEvenCombo)
                {
                    if (subAnim == 0) subAnim = 1;
                    else if (subAnim == 1) subAnim = 0;
                }

                if (player.direction == -1)
                {
                    if (subAnim == 0) subAnim = 1;
                    else if (subAnim == 1) subAnim = 0;
                }

                switch (subAnim)
                {
                    case 0:
                        return (
                            player.Center
                            + _directionForward.RotatedBy((alpha * 2 - 1) * MathF.PI / -3f) * (Projectile.width + handLength),
                            _directionForward.ToRotation() + (alpha * alpha * alpha * 2 - 1) * MathF.PI / -3f,
                            1);
                    case 1:
                        return (
                            player.Center
                            + _directionForward.RotatedBy((alpha * 2 - 1) * MathF.PI / 3f) * (Projectile.width + handLength),
                            _directionForward.ToRotation() + (alpha * alpha * alpha * 2 - 1) * MathF.PI / 3f,
                            1);
                    case 2:
                        return (
                            player.Center
                            + _directionForward * (swingArc * Projectile.width + handLength),
                            _directionForward.ToRotation(),
                            1f + swingArc * 0.2f);
                    default:
                        return (player.Center, 0, 1);
                }
            default:
                throw new NotImplementedException();
        }
    }

    private (Vector2 center, float rotation, float scale) AnimateSheathed(Player player)
    {
        switch (AttackType)
        {
            case AttackTypeEnum.BladeSwing:
                return (
                    player.Center + new Vector2(0, player.height * 0.25f),
                    player.direction == 1 ? MathHelper.ToRadians(180 - 20) : MathHelper.ToRadians(20),
                    1);
            default:
                return (player.Center, 0, 1);
        }
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        base.ModifyDamageHitbox(ref hitbox);

        hitbox.Inflate((int)(hitbox.Width * (Projectile.scale - 1) * 2), (int)(hitbox.Height * (Projectile.scale - 1) * 2));
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates());

        //Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Red, Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(Projectile.width, Projectile.height), SpriteEffects.None);
        Main.EntitySpriteDraw(_itemTexture, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation + MathF.PI / 4f, _itemTexture.Size() / 2, 1.5f * Projectile.scale, SpriteEffects.None);

        return false;
    }
}