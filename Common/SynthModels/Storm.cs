using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SyntheticEvolution.Common.ChainPhysics;
using SyntheticEvolution.Content.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace SyntheticEvolution.Common.SynthModels;

[SynthModel(Key = "storm")]
public class Storm : SynthModel
{
    public static readonly float LegsArmorScale = 0.2f;
    public static readonly float BladesArmorScale = 0.2f;
    public static readonly float HeadArmorScale = 0.1f;
    public static readonly float ChestArmorScale = 0.3f;

    public static readonly string LegsFeatureDescription = "[c/00E2AD:Storm] blade legs are good for dealing [c/7F7F7F:damage] to\nenemies on a [c/7F7F7F:high speed] [c/7F7F7F:mid-air].\nAlthough, they're [c/7F7F7F:not] very good for [c/7F7F7F:running] on a [c/7F7F7F:ground].";

    public PartSlot HeadSlot => Equipment.GetSlot(0);
    public PartSlot ChestSlot => Equipment.GetSlot(1);
    public PartSlot LeftArmSlot => Equipment.GetSlot(2);
    public PartSlot RightArmSlot => Equipment.GetSlot(3);
    public PartSlot LegsSlot => Equipment.GetSlot(4);
    public PartSlot GrappleSlot => Equipment.GetSlot(5);
    
    private static Asset<Texture2D> _hotbarSlotTexture;
    private static Asset<Texture2D> _hotbarSlotTextureBorder;

    public override string Name => "Storm";
    public override bool CanUseConventionalItems => false;
    public override bool HaveCustomHotbar => true;

    private int _grappleProjId = -1;
    private float _grappleLengthMax = 16 * 50;
    private float _grappleLengthMin = 16 * 4;
    private Chain _grappleChain = null;
    private float _grappleReelSpeed = 0f;

    private bool _wasControlUp;
    private bool _wasControlDown;

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

    public override void Update()
    {
        base.Update();

        var player = OwningPlayer;

        //player.velocity = Vector2.Zero;

        if (_grappleChain != null)
        {
            if (player.controlUp && !_wasControlUp && !player.controlDown)
            {
                // >0 = reel in
                if (_grappleReelSpeed == 0) _grappleReelSpeed = 3;
                else if (_grappleReelSpeed > 0) _grappleReelSpeed = 6;
                else _grappleReelSpeed = 0;
            }
            else if (player.controlDown && !_wasControlDown && !player.controlUp)
            {
                // <0 = reel out
                if (_grappleReelSpeed == 0) _grappleReelSpeed = -3;
                else if (_grappleReelSpeed < 0) _grappleReelSpeed = -6;
                else _grappleReelSpeed = 0;
            }

            _wasControlUp = player.controlUp;
            _wasControlDown = player.controlDown;

            if (_grappleReelSpeed > 0)
            {
                float currentLen = _grappleChain.CalculateLength();
                float reelIn = MathF.Min(_grappleReelSpeed, currentLen - _grappleLengthMin);

                if (reelIn < 0.01f)
                {
                    _grappleReelSpeed = 0;
                }
                else
                {
                    _grappleChain.DecreaseFromStart(reelIn);
                }
            }
            else if (_grappleReelSpeed < 0)
            {
                float currentLen = _grappleChain.CalculateLength();
                float reelOut = MathF.Max(_grappleReelSpeed, currentLen - _grappleLengthMax);

                if (reelOut > -0.01f)
                {
                    _grappleReelSpeed = 0;
                }
                else
                {
                    _grappleChain.IncreaseFromStart(-reelOut);
                }
            }
        }
    }

    public override void Grapple()
    {
        base.Grapple();

        if (_grappleProjId >= 0)
        {
            KillGrapple();
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

            Vector2 Speed = vectorToMouse * actualShootSpeed;
            var projId = Projectile.NewProjectile(player.GetSource_ItemUse(item), startPosition, Speed, type, damage, knockBack, Main.myPlayer);
            var hook = Main.projectile[projId];

            _grappleLengthMax = 16 * 50;
        }
    }

    public override void GrappleMovement()
    {
        base.GrappleMovement();

        var player = OwningPlayer;

        if (player.grappling[0] < 0) return;

        var oldHookProj = Main.projectile[player.grappling[0]];

        KillGrapple();
        SpawnGrapple(oldHookProj);

        player.RemoveAllGrapplingHooks();
    }

    private void KillGrapple()
    {
        if (_grappleProjId < 0) return;

        ((StormHook)Main.projectile[_grappleProjId].ModProjectile).ReelBack();
        _grappleProjId = -1;
        _grappleChain.HoldBack = null;
        _grappleChain = null;
        _grappleReelSpeed = 0;
    }

    private void SpawnGrapple(Projectile vanillaGrapple)
    {
        var player = OwningPlayer;
        
        (Texture2D chainTexture, Texture2D chainGlowTexture, Color chainGlowColor) = StormHook.GetChainTexture(vanillaGrapple.type);
        var hookTexture = TextureAssets.Projectile[vanillaGrapple.type].Value;

        _grappleChain = Chain.Create(player.Center, vanillaGrapple.Center, chainTexture.Height / StormHook.TextureSizeToSplitAmount(chainTexture.Size().ToPoint()));
        _grappleChain.LastPoint.Fixed = true;
        _grappleChain.HoldPosition = player.Center;
        _grappleChain.HoldBack = (force) =>
        {
            float receiveX = 0.5f;
            float receiveY = 0.5f;

            bool onGround = Collision.SolidTiles(player.BottomLeft + new Vector2(0, 1), player.width, 1, true);
            
            if (onGround)
            {
                if (force.Y > -1) receiveY = 0;
            
                receiveX = MathHelper.Clamp(force.X * 0.8f, 0, 1);
            }

            player.velocity.X += force.X * receiveX * 0.5f;
            player.velocity.Y += force.Y * receiveY * 0.5f;

            return (receiveX, receiveY);
        };

        _grappleProjId = Projectile.NewProjectile(null, vanillaGrapple.Center, Vector2.Zero, ModContent.ProjectileType<StormHook>(), 0, 0, playerId);

        var newHookProj = (StormHook)Main.projectile[_grappleProjId].ModProjectile;

        newHookProj.Projectile.rotation = vanillaGrapple.rotation;
        newHookProj.Setup(_grappleChain, hookTexture, chainTexture, chainGlowTexture, chainGlowColor, vanillaGrapple.Size.ToPoint());
    }

    public override void DrawHotbar()
    {
        base.DrawHotbar();

        var player = OwningPlayer;

        _hotbarSlotTexture ??= ModContent.Request<Texture2D>("SyntheticEvolution/Assets/Textures/UI/StormHotbarSlot", AssetRequestMode.ImmediateLoad);
        _hotbarSlotTextureBorder ??= ModContent.Request<Texture2D>("SyntheticEvolution/Assets/Textures/UI/StormHotbarSlotBorder", AssetRequestMode.ImmediateLoad);

        for (int i = 0; i < 4; i++)
        {
            int slotSize = 42;
            Rectangle rect = new Rectangle(20 + (slotSize + 10) * i, 20, slotSize, slotSize);
            Terraria.Utils.DrawSplicedPanel(Main.spriteBatch, _hotbarSlotTexture.Value, rect.X, rect.Y, rect.Width, rect.Height, 12, 12, 12, 12, Color.White * 0.25f);
            if (i == player.selectedItem) Terraria.Utils.DrawSplicedPanel(Main.spriteBatch, _hotbarSlotTextureBorder.Value, rect.X, rect.Y, rect.Width, rect.Height, 12, 12, 12, 12, Main.OurFavoriteColor);
            ItemSlot.DrawItemIcon(player.inventory[i], 0, Main.spriteBatch, rect.Center(), 1f, 28f, Color.White);
        }
    }
}