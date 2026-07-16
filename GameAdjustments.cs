using System;
using Terraria;
using Terraria.ModLoader;

namespace GameAdjustments;

public class GameAdjustments : Mod
{
    public override void Load()
    {
        // 禁用伤害波动
        On_Main.DamageVar_float_int_float += On_Main_DamageVar_float_int_float;
    }

    private static int On_Main_DamageVar_float_int_float(On_Main.orig_DamageVar_float_int_float orig, float dmg, int percent, float luck)
    {
        return (int)Math.Round(dmg);
    }
}
