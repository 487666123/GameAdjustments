using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GameAdjustments.Common.GlobalItems;

internal class PrefixEnhancement_GlobalItem : GlobalItem
{
    private static int GetArmorPenetration(Item item)
    {
        return item.prefix switch
        {
            67 => 6,
            PrefixID.Lucky => 10,
            69 => 2,
            70 => 4,
            71 => 6,
            PrefixID.Menacing => 8,
            _ => 0,
        };
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual)
    {
        player.GetArmorPenetration(DamageClass.Generic) += GetArmorPenetration(item);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        var penetration = GetArmorPenetration(item);
        if (penetration <= 0) return;

        int prefixIndex = tooltips.FindLastIndex(line => line.Mod == "Terraria" && line.Name.StartsWith("Prefix"));

        if (prefixIndex < 0) return;

        tooltips.Insert(prefixIndex + 1, new TooltipLine(
            Mod,
            "PrefixDamage",
            Mod.GetLocalization("Common.ArmorPenetration").Format(penetration))
        {
            OverrideColor = new Color(120, 190, 120)
        });
    }
}
