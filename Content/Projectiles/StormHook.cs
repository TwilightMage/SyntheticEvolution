﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SyntheticEvolution.Common.ChainPhysics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SyntheticEvolution.Content.Projectiles;

public class StormHook : ModProjectile
{
    private struct HookChainTexture
    {
        public Texture2D Texture;
        public Texture2D GlowTexture;
        public Color GlowColor;
    }

    private static Dictionary<int, HookChainTexture> ExternalChainTextures = new Dictionary<int, HookChainTexture>();

    public Chain GrappleChain;

    private Texture2D _texture;
    private Texture2D _chainTexture;
    private Texture2D _chainGlowTexture;
    private Color _chainGlowColor;

    public override void SetDefaults()
    {
        base.SetDefaults();
    }

    // Use that for external mod compatibility
    public static void RegisterExternalChainTexture(int projectileType, Texture2D texture, Texture2D glowtexture, Color glowColor) => ExternalChainTextures[projectileType] = new HookChainTexture { Texture = texture, GlowTexture = glowtexture, GlowColor = glowColor };

    public static (Texture2D, Texture2D, Color) GetChainTexture(int projectileType)
    {
        if (ExternalChainTextures.TryGetValue(projectileType, out HookChainTexture tex)) return (tex.Texture, tex.GlowTexture, tex.GlowColor);

        switch (projectileType)
        {
            case ProjectileID.Hook:
                return (TextureAssets.Chain.Value, null, Color.Transparent);
            case ProjectileID.IvyWhip:
                return (TextureAssets.Chain5.Value, null, Color.Transparent);
            case ProjectileID.DualHookBlue:
                return (TextureAssets.Chain8.Value, null, Color.Transparent);
            case ProjectileID.DualHookRed:
                return (TextureAssets.Chain9.Value, null, Color.Transparent);
            case ProjectileID.GemHookAmethyst:
                return (TextureAssets.GemChain[0].Value, null, Color.Transparent);
            case ProjectileID.GemHookTopaz:
                return (TextureAssets.GemChain[1].Value, null, Color.Transparent);
            case ProjectileID.GemHookSapphire:
                return (TextureAssets.GemChain[2].Value, null, Color.Transparent);
            case ProjectileID.GemHookEmerald:
                return (TextureAssets.GemChain[3].Value, null, Color.Transparent);
            case ProjectileID.GemHookRuby:
                return (TextureAssets.GemChain[4].Value, null, Color.Transparent);
            case ProjectileID.GemHookDiamond:
                return (TextureAssets.GemChain[5].Value, null, Color.Transparent);
            case ProjectileID.BatHook:
                return (TextureAssets.Chain28.Value, null, Color.Transparent);
            case ProjectileID.WoodHook:
                return (TextureAssets.Chain29.Value, null, Color.Transparent);
            case ProjectileID.CandyCaneHook:
                return (TextureAssets.Chain30.Value, null, Color.Transparent);
            case ProjectileID.ChristmasHook:
                return (TextureAssets.Chain31.Value, TextureAssets.Chain32.Value, new Color(200, 200, 200, 0));
            case ProjectileID.FishHook:
                return (TextureAssets.Chain33.Value, null, Color.Transparent);
            case ProjectileID.SlimeHook:
                return (TextureAssets.Chain35.Value, null, Color.Transparent);
            case ProjectileID.TrackHook:
                return (TextureAssets.Chain36.Value, null, Color.Transparent);
            case ProjectileID.AntiGravityHook:
                return (TextureAssets.Extra[3].Value, null, Color.Transparent);
            case ProjectileID.TendonHook:
                return (TextureAssets.Chains[0].Value, null, Color.Transparent);
            case ProjectileID.ThornHook:
                return (TextureAssets.Chains[1].Value, null, Color.Transparent);
            case ProjectileID.IlluminantHook:
                return (TextureAssets.Chains[2].Value, null, Color.Transparent);
            case ProjectileID.WormHook:
                return (TextureAssets.Chains[3].Value, null, Color.Transparent);
            case ProjectileID.LunarHookSolar:
                return (TextureAssets.Chains[8].Value, TextureAssets.Chains[12].Value, Color.White);
            case ProjectileID.LunarHookVortex:
                return (TextureAssets.Chains[9].Value, TextureAssets.Chains[13].Value, Color.White);
            case ProjectileID.LunarHookNebula:
                return (TextureAssets.Chains[10].Value, TextureAssets.Chains[14].Value, Color.White);
            case ProjectileID.LunarHookStardust:
                return (TextureAssets.Chains[11].Value, TextureAssets.Chains[15].Value, Color.White);
            case ProjectileID.StaticHook:
                return (TextureAssets.Chains[16].Value, null, Color.Transparent);
            case ProjectileID.AmberHook:
                return (TextureAssets.Extra[95].Value, null, Color.Transparent);
            case ProjectileID.SquirrelHook:
                return (TextureAssets.Extra[154].Value, null, Color.Transparent);
            case ProjectileID.QueenSlimeHook:
                return (TextureAssets.Extra[208].Value, null, Color.Transparent);
            default:
                SyntheticEvolution.Instance.Logger.Error($"Failed to find chain texture for projectile of type {projectileType}");
                return (TextureAssets.Chain.Value, null, Color.Transparent);
        }
    }

    public void Setup(Texture2D texture, Texture2D chainTexture, Texture2D chainGlowTexture, Color chainGlowColor, Point size)
    {
        _texture = texture;
        _chainTexture = chainTexture;
        _chainGlowTexture = chainGlowTexture;
        _chainGlowColor = chainGlowColor;
        Projectile.width = size.X;
        Projectile.height = size.Y;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (GrappleChain != null)
        {
            var p1 = GrappleChain.Points[^2].Position;
            var p2 = GrappleChain.Points[^1].Position;
            Projectile.rotation = p1.AngleTo(p2) + MathF.PI / 2;
        }

        Main.EntitySpriteDraw(_texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, _texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

        return false;
    }

    public override bool PreDrawExtras()
    {
        for (int i = 1; i < GrappleChain.Points.Length; i++)
        {
            var p1 = GrappleChain.Points[i - 1].Position - Main.screenPosition;
            var p2 = GrappleChain.Points[i].Position - Main.screenPosition;
            var center = (p1 + p2) / 2f;
            Color color = Lighting.GetColor((center + Main.screenPosition).ToTileCoordinates());
            Main.EntitySpriteDraw(_chainTexture, center, null, color, p1.AngleTo(p2) + MathF.PI / 2f, _chainTexture.Size() / 2f, 1, SpriteEffects.None);

            if (_chainGlowTexture != null)
            {
                Main.EntitySpriteDraw(_chainGlowTexture, center, null, _chainGlowColor, p1.AngleTo(p2) + MathF.PI / 2f, _chainTexture.Size() / 2f, 1, SpriteEffects.None);
            }
        }

        return false;
    }
}