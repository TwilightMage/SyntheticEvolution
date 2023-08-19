using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace SyntheticEvolution.Common.UI;

public class UIPartSlot : UIElement
{
    private PartSlot _slot;
    
    public UIPartSlot(PartSlot slot)
    {
        _slot = slot;
        
        Width.Set(40, 0);
        Height.Set(40, 0);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);

        _slot.HandleMouseClick();
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        
        _slot.HandleMouseHover();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        
        var dimensions = GetDimensions();

        Terraria.Utils.DrawSplicedPanel(spriteBatch, TextureAssets.InventoryBack.Value, (int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height, 10, 10, 10, 10, Color.White);
        ItemSlot.DrawItemIcon(_slot.TargetItem, 0, spriteBatch, dimensions.Center(), 1, 32, Color.White);
    }
}