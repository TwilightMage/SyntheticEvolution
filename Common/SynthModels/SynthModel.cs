﻿using System;
using Terraria;

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

    public virtual void Update()
    {
        
    }
    
    public virtual void Grapple()
    {
        
    }

    public virtual void GrappleMovement()
    {
        
    }
}