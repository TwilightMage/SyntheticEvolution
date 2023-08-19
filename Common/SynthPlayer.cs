﻿using ReLogic.Utilities;
using SyntheticEvolution.Common.SynthModels;
using SyntheticEvolution.Content.Mounts;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SyntheticEvolution.Common;

public class SynthPlayer : ModPlayer
{
public static SynthModel LocalSynthModel;

    private static Dictionary<string, Type> _synthTypes = new Dictionary<string, Type>();

    public SynthModel SynthModel => _synthModel;

    internal SynthModel _synthModel;

    static SynthPlayer()
    {
        foreach (var type in typeof(SynthPlayer).Assembly.GetTypes())
        {
            TryRegisterSynthModel(type);
        }
    }

    public static void TryRegisterSynthModel(Type type)
    {
        var attribute = type.GetAttribute<SynthModelAttribute>();
        if (attribute != null)
        {
            _synthTypes[attribute.Key] = type;
        }
    }

    public bool SetSynthModel(string key)
    {
        if (_synthModel?.GetType().GetAttribute<SynthModelAttribute>().Key == key)
        {
            if (!Player.mount.Active || Player.mount.Type != ModContent.MountType<SynthMount>())
            {
                Player.mount.SetMount(ModContent.MountType<SynthMount>(), Player);
            }
            
            _synthModel.Equipment.SetupFrom(_synthModel);

            return true;
        }
        
        if (Player.frozen || Player.tongued || Player.webbed || Player.stoned || Player.gravDir == -1f || Player.dead || Player.noItems)
            return false;
        
        
        if (_synthTypes.TryGetValue(key, out Type type))
        {
            _synthModel = (SynthModel)Activator.CreateInstance(type);
            _synthModel.Equipment.SetupFrom(_synthModel);
            _synthModel.playerId = Player.whoAmI;

            if (Player == Main.LocalPlayer)
            {
                LocalSynthModel = _synthModel;
            }
            
            Player.mount.SetMount(ModContent.MountType<SynthMount>(), Player);

            return true;
        }

        return false;
    }
    
    public bool SetSynthModel<T>() where T : SynthModel
    {
        if (_synthModel?.GetType() == typeof(T))
        {
            if (!Player.mount.Active || Player.mount.Type != ModContent.MountType<SynthMount>())
            {
                Player.mount.SetMount(ModContent.MountType<SynthMount>(), Player);
            }

            _synthModel.Equipment.SetupFrom(_synthModel);
            
            return true;
        }
        
        if (Player.frozen || Player.tongued || Player.webbed || Player.stoned || Player.gravDir == -1f || Player.dead || Player.noItems)
            return false;

        if (typeof(T).GetAttribute<SynthModelAttribute>() != null)
        {
            _synthModel = Activator.CreateInstance<T>();
            _synthModel.Equipment.SetupFrom(_synthModel);
            _synthModel.playerId = Player.whoAmI;
            
            if (Player == Main.LocalPlayer)
            {
                LocalSynthModel = _synthModel;
            }
            
            Player.mount.SetMount(ModContent.MountType<SynthMount>(), Player);

            return true;
        }

        return false;
    }

    public void ClearSynthModel()
    {
        _synthModel.playerId = -1;
        _synthModel = null;
        
        if (Player == Main.LocalPlayer)
        {
            LocalSynthModel = null;
        }
        
        if (Player.mount._active) Player.mount.Dismount(Player);
    }

    public override void SaveData(TagCompound tag)
    {
        tag["SynthModel"] = _synthModel?.GetType().GetAttribute<SynthModelAttribute>().Key;
    }

    public override void LoadData(TagCompound tag)
    {
        var modelKey = tag.GetString("SynthModel");
        if (modelKey != null)
        {
            SetSynthModel(modelKey);
        }
        else
        {
            ClearSynthModel();
        }
    }

    public override void PostUpdate()
    {
        SynthModel?.Update();
    }
}