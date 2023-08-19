using SyntheticEvolution.Common;
using SyntheticEvolution.Common.SynthModels;
using SyntheticEvolution.Common.Systems;
using Terraria;
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
        }

        public override void Unload()
        {
            Instance = null;
            
            On_Player.QuickMount -= QuickMount;
            On_Player.QuickGrapple -= QuickGrapple;
            On_Player.ToggleInv -= ToggleInv;
            On_Player.GrappleMovement -= GrappleMovement;
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
    }
}