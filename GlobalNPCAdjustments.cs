using Terraria;
using Terraria.ModLoader;

namespace GameAdjustments;

/// <summary>
/// 调整游戏中所有 NPC 的防御生效方式
/// </summary>
internal class GlobalNPCAdjustments : GlobalNPC
{
    /// <summary>
    /// 调整防御算法
    /// </summary>
    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        // 记录防御
        var defense = modifiers.Defense.Base;

        // 取消护甲
        modifiers.Defense.Base = 0;

        // 转换为百分比减伤算法
        var damageReduction = defense / (50 + defense);
        modifiers.FinalDamage *= 1 - damageReduction;
    }
}
