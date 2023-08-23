using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace SyntheticEvolution.Common.SynthModels;

[AttributeUsage(AttributeTargets.Class)]
public class SynthModelAttribute : Attribute
{
    public string Key;
}

public abstract class SynthModel
{
    public abstract string Name { get; }
    public virtual bool CanUseConventionalItems => true;
    public virtual bool HaveCustomHotbar => false;

    public virtual bool CanWalkInAir => true;

    public readonly SynthEquipment Equipment = new SynthEquipment();

    public int playerId;

    public Player OwningPlayer => Main.player[playerId];

    public virtual void SetDefaults()
    {
    }

    public virtual PartSlot[] CreateEquipmentSlots()
    {
        return Array.Empty<PartSlot>();
    }

    public virtual void Save(TagCompound tag)
    {
        TagCompound equipmentTag = new TagCompound();

        for (int i = 0; i < Equipment.NumParts; i++)
        {
            equipmentTag.Set(Equipment.GetSlot(i).Name, Equipment.GetPart(i));
        }

        tag.Set("Equipment", equipmentTag);
    }

    public virtual void Load(TagCompound tag)
    {
        TagCompound equipmentTag = tag.Get<TagCompound>("Equipment");

        for (int i = 0; i < Equipment.NumParts; i++)
        {
            var slot = Equipment.GetSlot(i);
            slot.TargetItem = equipmentTag.Get<Item>(slot.Name);
        }
    }

    public virtual void FixedUpdate()
    {
    }

    public virtual void Update(GameTime deltaTime)
    {
    }

    public virtual void Grapple()
    {
    }

    public virtual void GrappleMovement()
    {
    }

    public virtual bool PreHorizontalMovement()
    {
        return true;
    }

    public virtual void PostHorizontalMovement()
    {
    }

    public virtual void DrawHotbar()
    {
    }

    // If returns false then default usage will be performed
    public virtual bool StartUseItem(Item item)
    {
        return false;
    }

    public virtual bool OverrideItemUse(Item item)
    {
        return false;
    }

    public virtual bool OverrideItemAnimation(Item item)
    {
        return false;
    }

    public virtual bool DrawPlayer(ref PlayerDrawSet drawSet)
    {
        return false;
    }
}