using Microsoft.Xna.Framework;
using SyntheticEvolution.Common;
using SyntheticEvolution.Common.SynthModels;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SyntheticEvolution.Content.Globals;

public class GlobalStormHook : GlobalProjectile
{
    public override bool? CanHitNPC(Projectile projectile, NPC target)
    {
        if (projectile.aiStyle == ProjAIStyleID.Hook && projectile.TryGetOwner(out Player player) && player.GetSynth() != null)
        {
            if (target.friendly) return false;
            if (!target.boss) return false;
            return true;
        }

        return base.CanHitNPC(projectile, target);
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (projectile.aiStyle == ProjAIStyleID.Hook && projectile.TryGetOwner(out Player player) && player.GetSynth() != null)
        {
            modifiers.Knockback.Base = 0;
        }

        base.ModifyHitNPC(projectile, target, ref modifiers);
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (projectile.aiStyle == ProjAIStyleID.Hook && projectile.TryGetOwner(out Player player) && player.GetSynth() is Storm storm)
        {
            if (target.boss)
            {
                Vector2 hookPosDelta = projectile.Center - target.Center;
                storm.SpawnGrappleOnEnemy(projectile.type, target, hookPosDelta, target.rotation);
            }
        }

        base.OnHitNPC(projectile, target, hit, damageDone);
    }
}