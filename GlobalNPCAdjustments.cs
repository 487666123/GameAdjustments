using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using Terraria;
using Terraria.ModLoader;

namespace GameAdjustments;

/// <summary>
/// 调整游戏中所有 NPC 的防御生效方式
/// </summary>
internal class GlobalNPCAdjustments : GlobalNPC
{
    private static float ApplyCustomDefense(ref NPC.HitModifiers modifiers, float damageReduction)
    {
        float defenseScalingConstant = 100f;
        if (Main.masterMode) defenseScalingConstant = 50f;
        else if (Main.expertMode) defenseScalingConstant = 75f;

        float customDamageReduction =
            damageReduction / (defenseScalingConstant + damageReduction);

        modifiers.FinalDamage *= 1f - customDamageReduction;

        return 0f;
    }

    public override void Load()
    {
        IL_NPC.HitModifiers.GetDamage += static (il) =>
        {
            var c = new ILCursor(il);

            if (!c.TryGotoNext(
                    MoveType.After,
                    instruction => instruction.MatchLdloc(2),
                    instruction => instruction.MatchSub()))
            {
                throw new InvalidOperationException(
                    "无法定位护甲穿透后的防御计算");
            }

            if (!c.TryGotoNext(
                    MoveType.Before,
                    instruction => instruction.MatchLdcR4(0f),
                    instruction => instruction.MatchCall(
                        typeof(Math),
                        nameof(Math.Max))))
            {
                throw new InvalidOperationException(
                    "无法定位防御值的 Math.Max");
            }

            c.RemoveRange(2);

            if (!c.TryGotoNext(
                    MoveType.After,
                    instruction => instruction.MatchStloc(3)))
            {
                throw new InvalidOperationException(
                    "无法定位 NPC.HitModifiers.GetDamage 的 damageReduction");
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
