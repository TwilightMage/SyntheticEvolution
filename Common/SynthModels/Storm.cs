using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SyntheticEvolution.Common.ChainPhysics;
using SyntheticEvolution.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

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

    public override string Name => "Storm";
    public override bool CanUseConventionalItems => false;

    private int _grappleProjId = -1;
    private float _grappleDistanceLimit = 0;
    private Chain _grappleChain = null;

    public override PartSlot[] CreateEquipmentSlots()
    {
        return new[] { new PartSlot(PartSlot.SocketTypeEnum.Head, GetType(), new Vector2(0, 0)), new PartSlot(PartSlot.SocketTypeEnum.Chest, GetType(), new Vector2(0, 50)), new PartSlot(PartSlot.SocketTypeEnum.Arm, GetType(), new Vector2(0, 100)), new PartSlot(PartSlot.SocketTypeEnum.Arm, GetType(), new Vector2(0, 150)), new PartSlot(PartSlot.SocketTypeEnum.Legs, GetType(), new Vector2(0, 200)), new PartSlot(PartSlot.SocketTypeEnum.Module, null, new Vector2(75, 0), moduleFitCheck: (item) => ContentSamples.ProjectilesByType[item.shoot].aiStyle == ProjAIStyleID.Hook) };
    }

    public override void Update()
    {
        base.Update();

        if (_grappleChain != null)
        {
            _grappleChain.HoldPosition = OwningPlayer.Center;
            _grappleChain.UpdatePhysics();
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

            _grappleDistanceLimit = 16 * 10;
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
        
        Main.projectile[_grappleProjId].Kill();
        _grappleProjId = -1;
    }

    private void SpawnGrapple(Projectile vanillaGrapple)
    {
        (Texture2D chainTexture, Texture2D chainGlowTexture, Color chainGlowColor) = StormHook.GetChainTexture(vanillaGrapple.type);
        var hookTexture = TextureAssets.Projectile[vanillaGrapple.type].Value;
    
        _grappleChain = Chain.Create(OwningPlayer.Center, vanillaGrapple.Center, chainTexture.Height);
        _grappleChain.Last.Fixed = true;
        _grappleChain.HoldPosition = OwningPlayer.Center;
        
        _grappleProjId = Projectile.NewProjectile(null, vanillaGrapple.Center, Vector2.Zero, ModContent.ProjectileType<StormHook>(), 0, 0, playerId);
        
        var newHookProj = (StormHook)Main.projectile[_grappleProjId].ModProjectile;

        newHookProj.Projectile.rotation = vanillaGrapple.rotation;
        newHookProj.GrappleChain = _grappleChain;
        newHookProj.Setup(hookTexture, chainTexture, chainGlowTexture, chainGlowColor, hookTexture.Size().ToPoint());
    }
}