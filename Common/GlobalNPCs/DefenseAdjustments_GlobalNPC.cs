using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using Terraria;
using Terraria.ModLoader;

namespace GameAdjustments.Common.GlobalNPCs;

/// <summary>
/// 调整游戏中所有 NPC 的防御生效方式
/// </summary>
internal class DefenseAdjustments_GlobalNPC : GlobalNPC
{
    private static float ApplyCustomDefense(ref NPC.HitModifiers modifiers, float damageReduction)
    {
        float defenseScalingConstant = 100f;

        if (Main.masterMode)
        {
            defenseScalingConstant = 50f;
        }
        else if (Main.expertMode)
        {
            defenseScalingConstant = 75f;
        }

        float customDamageReduction =
            damageReduction / (defenseScalingConstant + MathF.Abs(damageReduction));

        modifiers.FinalDamage *= 1f - customDamageReduction;

        return 0f;
    }

    // 防御加强
    private static void ModifyDefanse(ref NPC.HitModifiers modifiers)
    {
        if (Main.masterMode)
        {
            modifiers.Defense.Base += 20;
            modifiers.Defense *= 1.5f;
        }
        else if (Main.expertMode)
        {
            modifiers.Defense.Base += 10;
            modifiers.Defense *= 1.25f;
        }
    }

    public override void Load()
    {
        var defenseNotFound = Mod.GetLocalization("Errors.DefenseNotFound");
        var armorPenetrationDefenseCalculationNotFound =
            Mod.GetLocalization("Errors.ArmorPenetrationDefenseCalculationNotFound");
        var defenseMathMaxNotFound = Mod.GetLocalization("Errors.DefenseMathMaxNotFound");
        var damageReductionNotFound = Mod.GetLocalization("Errors.DamageReductionNotFound");

        IL_NPC.HitModifiers.GetDamage += (il) =>
        {
            var c = new ILCursor(il);

            if (!c.TryGotoNext(
                MoveType.Before,
                instruction => instruction.MatchLdarg(0),
                instruction => instruction.MatchLdfld(typeof(NPC.HitModifiers), nameof(NPC.HitModifiers.Defense))))
            {
                throw new InvalidOperationException(
                    defenseNotFound.Value);
            }

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(ModifyDefanse);

            if (!c.TryGotoNext(
                    MoveType.After,
                    instruction => instruction.MatchLdloc(2),
                    instruction => instruction.MatchSub()))
            {
                throw new InvalidOperationException(
                    armorPenetrationDefenseCalculationNotFound.Value);
            }

            if (!c.TryGotoNext(
                    MoveType.Before,
                    instruction => instruction.MatchLdcR4(0f),
                    instruction => instruction.MatchCall(
                        typeof(Math),
                        nameof(Math.Max))))
            {
                throw new InvalidOperationException(
                    defenseMathMaxNotFound.Value);
            }

            c.RemoveRange(2);

            if (!c.TryGotoNext(
                    MoveType.After,
                    instruction => instruction.MatchStloc(3)))
            {
                throw new InvalidOperationException(
                    damageReductionNotFound.Value);
            }

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc_3);
            c.EmitDelegate(ApplyCustomDefense);
            c.Emit(OpCodes.Stloc_3);
        };
    }

    /// <summary>
    /// 旧版实现，已调整为 IL 实现
    /// </summary>
    //public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    //{
    //    return;
    //    // 记录防御
    //    var defense = Math.Max(modifiers.Defense.ApplyTo(0), 0);

    //    // 取消护甲
    //    modifiers.Defense = new();

    //    var defenseScalingConstant = 100f;
    //    if (Main.masterMode) defenseScalingConstant = 50f;
    //    else if (Main.expertMode) defenseScalingConstant = 75f;

    //    // 转换为百分比减伤算法
    //    var damageReduction = defense / (defenseScalingConstant + defense);
    //    modifiers.FinalDamage *= 1 - damageReduction;
    //}
}
