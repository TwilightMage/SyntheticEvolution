using Microsoft.Xna.Framework;
using SyntheticEvolution.Common.SynthModels;
using SyntheticEvolution.Content.Items.SynthParts;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace SyntheticEvolution.Common.UI;

public class SynthPartsFabricator : UIState
{
    private UIPanel _panel;
    private UIText _modelName;
    private UIElement _slotsPanel;

    public override void OnInitialize()
    {
        base.OnInitialize();
        
        _panel = new UIPanel();
        _panel.Width.Set(500, 0);
        _panel.Height.Set(500, 0);
        _panel.HAlign = 0.5f;
        _panel.VAlign = 0.5f;
        _panel.SetPadding(0);
        Append(_panel);

        _modelName = new UIText(LocalizedText.Empty);
        _modelName.Width.Set(0, 1);
        _modelName.Height.Set(30, 0);
        _modelName.TextOriginX = 0.5f;
        _modelName.TextOriginY = 0.5f;
        _panel.Append(_modelName);

        _slotsPanel = new UIElement();
        _slotsPanel.Width.Set(0, 1);
        _slotsPanel.Height.Set(-30, 1);
        _slotsPanel.VAlign = 1;
        _slotsPanel.SetPadding(10);
        _panel.Append(_slotsPanel);
    }

    public void Setup(Player player)
    {
        var synth = player.GetModPlayer<SynthPlayer>().SynthModel;

        if (synth != null)
        {
            _modelName.SetText(Language.GetText("Mods.SyntheticEvolution.UI.Fabricator.Title").Format(synth.Name));
            
            for (int i = 0; i < synth.Equipment.NumParts; i++)
            {
                UIPartSlot slotUI = synth.Equipment.CreateItemSlot(i);
                slotUI.Left.Set(synth.Equipment.GetSlot(i).Position.X, 0);
                slotUI.Top.Set(synth.Equipment.GetSlot(i).Position.Y, 0);
                _slotsPanel.Append(slotUI);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_panel?.ContainsPoint(Main.MouseScreen) ?? false)
        {
            Main.LocalPlayer.mouseInterface = true;
        }
    }
}