using SyntheticEvolution.Common;
using Terraria;
using Terraria.ModLoader;

namespace SyntheticEvolution.Content.Buffs
{
    public class SynthBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            var synthBody = Main.LocalPlayer.GetModPlayer<SynthPlayer>().SynthModel;
            
            tip = $"Model: {synthBody.Name}";
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.mount.SetMount(ModContent.MountType<Mounts.SynthMount>(), player);
            player.buffTime[buffIndex] = 10;
        }

        public override bool RightClick(int buffIndex) => false;
    }
}