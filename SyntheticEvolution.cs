using Microsoft.Xna.Framework;
using ReLogic.Graphics;
using SyntheticEvolution.Common;
using SyntheticEvolution.Common.SynthModels;
using SyntheticEvolution.Common.Systems;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace SyntheticEvolution
{
    public partial class SyntheticEvolution : Mod
    {
        public static SyntheticEvolution Instance;
    
        public override void Load()
        {
            Instance = this;
            
            On_Player.QuickMount += QuickMount;
            On_Player.QuickGrapple += QuickGrapple;
            On_Player.ToggleInv += ToggleInv;
            On_Player.GrappleMovement += GrappleMovement;
            //On_Player.HorizontalMovement += HorizontalMovement;
            On_Main.DrawInterface_30_Hotbar += DrawHotbar;
        }

        public override void Unload()
        {
            Instance = null;
            
            On_Player.QuickMount -= QuickMount;
            On_Player.QuickGrapple -= QuickGrapple;
            On_Player.ToggleInv -= ToggleInv;
            On_Player.GrappleMovement -= GrappleMovement;
            //On_Player.HorizontalMovement -= HorizontalMovement;
            On_Main.DrawInterface_30_Hotbar -= DrawHotbar;
        }

        private void QuickMount(On_Player.orig_QuickMount orig, Player self)
        {
            if (SynthPlayer.LocalSynthModel != null)
            {
                return;
            }

            orig(self);
        }

        private void QuickGrapple(On_Player.orig_QuickGrapple orig, Player self)
        {
            if (self.GetSynth() is { CanUseConventionalItems: false } synth)
            {
                synth.Grapple();

                return;
            }

            orig(self);
        }

        private void ToggleInv(On_Player.orig_ToggleInv orig, Player self)
        {
            if (UISystem.IsFabricatorVisible)
            {
                UISystem.CloseFabricator();
            }
            else
            {
                orig(self);
            }
        }

        private void GrappleMovement(On_Player.orig_GrappleMovement orig, Player self)
        {
            if (self.GetSynth() is { CanUseConventionalItems: false } synth)
            {
                synth.GrappleMovement();

                return;
            }

            orig(self);
        }
        
        private void DrawHotbar(On_Main.orig_DrawInterface_30_Hotbar orig, Main self)
        {
            Main.spriteBatch.DrawString(FontAssets.MouseText.Value, Main.LocalPlayer.velocity.X.ToString(), new Vector2(700, 100), Color.White);
            Main.spriteBatch.DrawString(FontAssets.MouseText.Value, Main.LocalPlayer.velocity.Y.ToString(), new Vector2(700, 130), Color.White);

            if (SynthPlayer.LocalSynthModel is { HaveCustomHotbar: true } synth)
            {
                synth.DrawHotbar();
                
                return;
            }
            
            orig(self);
        }
        
        private void HorizontalMovement(On_Player.orig_HorizontalMovement orig, Player self)
        {
            if (SynthPlayer.LocalSynthModel != null)
            {
                SynthPlayer.LocalSynthModel.HorizontalMovement();
                
                return;
            }
            
            orig(self);
        }
    }
}