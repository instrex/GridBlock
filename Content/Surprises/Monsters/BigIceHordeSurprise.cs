using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class BigIceHordeSurprise : GridBlockSurprise.ProjectileSpawner<BigIceHordeSurpriseProjectile> {
    public override bool IsNegative => true;
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 2f;
    }

    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return Main.hardMode && !Main.raining && player.ZoneSnow && chunk.ContentAnalysis.SuitableForHordeEvents;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        Main.raining = true;
        Main.maxRaining = 0.8f;
        Main.maxRain = 140;
        Main.rainTime = 18000;

        base.Trigger(player, chunk);
    }
}

public class BigIceHordeSurpriseProjectile : IceHordeSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(4, 9) * (Main.rand.NextFloat() < 0.4f ? 2 : 1)) * SpawnInterval;
        NpcType = NPCID.IceSlime;
    }

    public override int GetNpcType() {
        if (CurrentNpcIndex == 3)
            return NPCID.IceGolem;

        return base.GetNpcType();
    }
}