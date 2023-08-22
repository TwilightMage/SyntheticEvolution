using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SyntheticEvolution.Common;
using SyntheticEvolution.Common.SynthModels;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace SyntheticEvolution.Content.Projectiles;

public class StormMeleeAttack : ModProjectile
{
    public enum AnimationType
    {
        BladeSwing
    }

    public bool OneHitPerEnemy;

    private Item _item;
    private Texture2D _itemTexture;
    private Vector2 _directionForward;
    private Vector2 _directionLeft;
    private Vector2 _homePosition;

    public AnimationType Animation { get; private set; } = AnimationType.BladeSwing;
    public int ComboCounter;
    private int[] _animationData;
    private float _animationTime;
    private float _animationDuration;
    private List<NPC> _npcHit = new List<NPC>();

    public override void SetDefaults()
    {
        base.SetDefaults();

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
    }

    public void AnimateSwingAttack(Storm storm, Item item, Vector2 direction, ref int stormComboCounter, ref int[] stormMeleeAnimationData, bool oneHitPerEnemy = true)
    {
        var player = storm.OwningPlayer;

        _itemTexture = TextureAssets.Item[item.type].Value;

        Projectile.width = (int)(_itemTexture.Size().Length() * player.GetAdjustedItemScale(item));
        Projectile.height = (int)(_itemTexture.Size().Length() * player.GetAdjustedItemScale(item) * 0.3f);
        Projectile.Center = player.Center + direction * Projectile.width / 2;
        Projectile.rotation = direction.ToRotation();

        _homePosition = Projectile.Center;

        AnimateAttack(AnimationType.BladeSwing, storm, item, direction, ref stormComboCounter, ref stormMeleeAnimationData, oneHitPerEnemy);
    }

    public void AnimateAttack(AnimationType animation, Storm storm, Item item, Vector2 direction, ref int stormComboCounter, ref int[] stormMeleeAnimationData, bool oneHitPerEnemy = true)
    {
        var player = storm.OwningPlayer;

        Animation = animation;
        OneHitPerEnemy = oneHitPerEnemy;

        _item = item;
        _directionForward = direction;
        _directionLeft = new Vector2(_directionForward.Y, -_directionForward.X);

        if (Animation == AnimationType.BladeSwing)
        {
            if (storm.LastMeleeAnimation == AnimationType.BladeSwing && Main.GameUpdateCount - storm.LastMeleeAttackTime < CombinedHooks.TotalUseTime(item.useTime, player, item) + 30)
            {
                ComboCounter = stormComboCounter % 3;
                stormComboCounter = ComboCounter + 1;
            }
            else
            {
                ComboCounter = 0;
                stormComboCounter = 0;
            }

            _animationData = stormMeleeAnimationData.Copy();
            if (ComboCounter == 0 && storm.LastMeleeAnimation == AnimationType.BladeSwing)
            {
                if (_animationData[0] == 0) _animationData[0] = 1;
                else _animationData[0] = 0;

                stormMeleeAnimationData[0] = _animationData[0];
            }
        }

        _animationDuration = CombinedHooks.TotalUseTime(item.useTime, player, item) / 60f;

        Projectile.CritChance = GetCrit(player);
    }

    public int GetCrit(Player player)
    {
        switch (Animation)
        {
            case AnimationType.BladeSwing:
                switch (ComboCounter)
                {
                    case 0:
                        return player.GetWeaponCrit(_item);
                    case 1:
                        return (int)MathF.Round(player.GetWeaponCrit(_item) * 1.2f);
                    case 2:
                        return (int)MathF.Round(player.GetWeaponCrit(_item) * 1.5f);
                }
                break;
        }

        return player.GetWeaponCrit(_item);
    }

    public override bool? CanHitNPC(NPC target)
    {
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
        }
    }

    public override void AI()
    {
        base.AI();

        _animationTime += 1 / 60f;

        if (_animationTime > _animationDuration)
        {
            Projectile.Kill();
            return;
        }

        float animationAlpha = _animationTime / _animationDuration;

        Player player = Projectile.TryGetOwner(out Player owner) ? owner : null;
        Storm storm = player?.GetSynth() as Storm ?? null;

        if (player != null)
        {
            _homePosition = player.Center + _directionForward * (_itemTexture.Size().Length() / (1.5f *1.8f) + player.height / 2f);
        }

        switch (Animation)
        {
            case AnimationType.BladeSwing:
                float swingSine = MathF.Sin(2 * animationAlpha * MathF.PI);
                float swingArc = MathF.Sin(animationAlpha * MathF.PI);
                int subAnim = ComboCounter;
                if (_animationData[0] == 1)
                {
                    if (subAnim == 0) subAnim = 1;
                    else if (subAnim == 1) subAnim = 0;
                }

                switch (subAnim)
                {
                    case 0:
                        Projectile.rotation = _directionForward.ToRotation() + (animationAlpha * 2 - 1) * MathF.PI / 3f;
                        Projectile.Center = _homePosition - _directionForward * Projectile.width * 1f
                                            + _directionForward.RotatedBy((animationAlpha * 2 - 1) * MathF.PI / 3f) * Projectile.width * 1f;
                        break;
                    case 1:
                        Projectile.rotation = _directionForward.ToRotation() + (animationAlpha * 2 - 1) * MathF.PI / -3f;
                        Projectile.Center = _homePosition - _directionForward * Projectile.width * 1f
                                            + _directionForward.RotatedBy((animationAlpha * 2 - 1) * MathF.PI / -3f) * Projectile.width * 1f;
                        break;
                    case 2:
                        Projectile.Center = _homePosition
                                            + _directionForward * _itemTexture.Size().Length() * 0.3f * swingArc;
                        Projectile.scale = 1f + swingArc * 0.1f;
                        break;
                }

                break;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        float opacity = 1;
        float animationAlpha = _animationTime / _animationDuration;

        switch (Animation)
        {
            case AnimationType.BladeSwing:
                // Magic formulas time
                opacity = 1 - MathF.Pow(animationAlpha - 0.5f, 4) / 0.062f;
                break;
        }

        //Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Red, Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(Projectile.width, Projectile.height), SpriteEffects.None);
        Main.EntitySpriteDraw(_itemTexture, Projectile.Center - Main.screenPosition, null, Color.White * opacity, Projectile.rotation + MathF.PI / 4f, _itemTexture.Size() / 2, 1.5f * Projectile.scale, SpriteEffects.None);

        return false;
    }
}