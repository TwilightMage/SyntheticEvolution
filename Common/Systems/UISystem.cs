using Microsoft.Xna.Framework;
using SyntheticEvolution.Common.UI;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace SyntheticEvolution.Common.Systems;

public class UISystem : ModSystem
{
    public static SynthPartsFabricator Fabricator;
    public static UserInterface FabricatorUI;
    public static bool IsFabricatorVisible => FabricatorUI?.CurrentState == Fabricator && Fabricator != null;

    private LegacyGameInterfaceLayer _layer;
    
    private bool _wasMouseLeftDown;
    public static bool JustMouseLeftDown { get; private set; }

    public override void OnModLoad()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            Fabricator = new SynthPartsFabricator();
            FabricatorUI = new UserInterface();

            _layer = new LegacyGameInterfaceLayer("Synth Parts Fabricator UI", Draw, InterfaceScaleType.UI);
        }
    }

    public override void OnModUnload()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            _layer = null;
            
            FabricatorUI = null;
            Fabricator = null;
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        JustMouseLeftDown = Main.mouseLeft && !_wasMouseLeftDown;
        _wasMouseLeftDown = Main.mouseLeft;
        
        if (!Main.gameMenu)
        {
            FabricatorUI?.Update(gameTime);
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        layers.Insert(layers.FindIndex(layer => layer.Name == "Vanilla: Cursor"), _layer);
    }

    public override void OnWorldUnload()
    {
        CloseFabricator();
    }

    private bool Draw()
    {
        if (!Main.gameMenu)
        {
            FabricatorUI.Draw(Main.spriteBatch, new GameTime());
        }
        return true;
    }

    public static void OpenFabricator()
    {
        FabricatorUI.SetState(Fabricator);
        Fabricator.Setup(Main.LocalPlayer);
    }

    public static void CloseFabricator()
    {
        FabricatorUI.SetState(null);
    }
}