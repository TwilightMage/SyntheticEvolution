using SyntheticEvolution.Common.SynthModels;
using SyntheticEvolution.Common.UI;
using System;
using System.Linq;
using Terraria;
using Terraria.UI;

namespace SyntheticEvolution.Common;

public class SynthEquipment
{
    public event Action OnEquipmentChanged;
    
    public Type SynthType;

    private Item[] _parts = null;
    private PartSlot[] _slots = null;

    public int NumParts => _parts.Length;

    public Item GetPart(int slotIndex) => _parts[slotIndex];

    public void SetPart(Item item, int slotIndex)
    {
        _parts[slotIndex] = item;
        OnEquipmentChanged?.Invoke();
    }

    public PartSlot GetSlot(int slotIndex) => _slots[slotIndex];
    public PartSlot GetSlot(string slotName) => _slots.FirstOrDefault(slot => slot.Name == slotName);

    public UIPartSlot CreateUIItemSlot(int slotIndex) => new UIPartSlot(_slots[slotIndex]);

    public void SetupFrom(SynthModel synth)
    {
        if (_parts != null) return;
        
        _slots = synth.CreateEquipmentSlots();

        _parts = new Item[_slots.Length];

        for (int i = 0; i < _parts.Length; i++)
        {
            _parts[i] = new Item();
            _slots[i].SetTarget(this, i);
        }
    }

    public void HandleMouseHover(int slotIndex)
    {
        ItemSlot.MouseHover(_parts, 13, slotIndex);
    }
}