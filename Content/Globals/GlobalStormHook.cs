using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SyntheticEvolution.Content.Globals;

public class GlobalStormHook : GlobalProjectile
{
    public override bool? CanHitNPC(Projectile projectile, NPC target)
    {
        if (projectile.aiStyle == ProjAIStyleID.Hook)
        {
            return true;
        }
        
        return base.CanHitNPC(projectile, target);
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (projectile.aiStyle == ProjAIStyleID.Hook)
        {
            
        }
        
        base.OnHitNPC(projectile, target, hit, damageDone);
    }
}