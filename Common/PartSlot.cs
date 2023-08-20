using Microsoft.Xna.Framework;
using SyntheticEvolution.Content.Items.SynthParts;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace SyntheticEvolution.Common;

public class PartSlot
{
    public enum SocketTypeEnum
    {
        Program,
        Module,
        Head,
        Chest,
        Legs,
        Arm,
        Segment,
        Visor,
    }

    public string Name;
    public Type SynthType;
    public SocketTypeEnum SocketType;
    public Vector2 Position;
    public Func<Item, bool> ModuleFitCheck = null;

    public Item TargetItem
    {
        get => _target.GetPart(_targetIndex);
        set => _target.SetPart(value, _targetIndex);
    }

    public SynthPart TargetPart => TargetItem.ModItem as SynthPart;

    private SynthEquipment _target;
    private int _targetIndex;

    public PartSlot(string name, SocketTypeEnum socketType, Type synthType, Vector2 position, Func<Item, bool> moduleFitCheck = null)
    {
        Name = name;
        SocketType = socketType;
        SynthType = synthType;
        Position = position;
        ModuleFitCheck = moduleFitCheck;
    }

    public void SetTarget(SynthEquipment target, int targetIndex)
    {
        _target = target;
        _targetIndex = targetIndex;
    }

    public bool CanFitPart(Item item)
    {
        if (item.ModItem is SynthPart part && SocketType == part.SocketType && (part.SynthTypes.Length == 0 || part.SynthTypes.Contains(SynthType))) return true;
        if (SocketType == SocketTypeEnum.Module && (ModuleFitCheck?.Invoke(item) == true)) return true;

        return false;
    }

    public void HandleMouseClick()
    {
        if (Main.mouseItem.IsAir || CanFitPart(Main.mouseItem))
        {
            (Main.mouseItem, TargetItem) = (TargetItem, Main.mouseItem);
            SoundEngine.PlaySound(SoundID.Grab);
        }
    }

    public void HandleMouseHover()
    {
        _target.HandleMouseHover(_targetIndex);
    }
}