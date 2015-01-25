﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Marksman
{
    internal class PotionManager
    {
        private enum PotionType
        {
            Health, Mana
        };

        private class Potion
        {
            public string Name { get; set; }
            public int MinCharges { get; set; }
            public ItemId ItemId { get; set; }
            public int Priority { get; set; }
            public List<PotionType> TypeList { get; set; }
        }

        private List<Potion> potions;

        public PotionManager()
        {
            potions = new List<Potion>
            {
                new Potion
                {
                    Name = "ItemCrystalFlask",
                    MinCharges = 1,
                    ItemId = (ItemId) 2041,
                    Priority = 1,
                    TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
                },
                new Potion
                {
                    Name = "RegenerationPotion",
                    MinCharges = 0,
                    ItemId = (ItemId) 2003,
                    Priority = 2,
                    TypeList = new List<PotionType> {PotionType.Health}
                },
                new Potion
                {
                    Name = "ItemMiniRegenPotion",
                    MinCharges = 0,
                    ItemId = (ItemId) 2010,
                    Priority = 4,
                    TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
                },
                new Potion
                {
                    Name = "FlaskOfCrystalWater",
                    MinCharges = 0,
                    ItemId = (ItemId) 2004,
                    Priority = 3,
                    TypeList = new List<PotionType> {PotionType.Mana}
                }
            };
            Load();
        }

        private void Load()
        {
            potions = potions.OrderBy(x => x.Priority).ToList();
            Program.Config.AddSubMenu(new Menu("Potion Manager", "PotionManager"));

            Program.Config.SubMenu("PotionManager").AddSubMenu(new Menu("Health", "Health"));
            Program.Config.SubMenu("PotionManager")
                .SubMenu("Health")
                .AddItem(new MenuItem("HealthPotion", "Use Health Potion").SetValue(true));
            Program.Config.SubMenu("PotionManager")
                .SubMenu("Health")
                .AddItem(new MenuItem("HealthPercent", "HP Trigger Percent").SetValue(new Slider(30)));

            Program.Config.SubMenu("PotionManager").AddSubMenu(new Menu("Mana", "Mana"));
            Program.Config.SubMenu("PotionManager")
                .SubMenu("Mana")
                .AddItem(new MenuItem("ManaPotion", "Use Mana Potion").SetValue(true));
            Program.Config.SubMenu("PotionManager")
                .SubMenu("Mana")
                .AddItem(new MenuItem("ManaPercent", "MP Trigger Percent").SetValue(new Slider(30)));

            Game.OnGameUpdate += OnGameUpdate;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if(ObjectManager.Player.HasBuff("Recall") || ObjectManager.Player.InFountain() && ObjectManager.Player.InShop())
                return;
                
            try
            {
                if (Program.Config.Item("HealthPotion").GetValue<bool>())
                {
                    if (ObjectManager.Player.HealthPercentage() <= Program.Config.Item("HealthPercent").GetValue<Slider>().Value)
                    {
                        var healthSlot = GetPotionSlot(PotionType.Health);
                        if (!IsBuffActive(PotionType.Health))
                            ObjectManager.Player.Spellbook.CastSpell(healthSlot.SpellSlot);
                    }
                }

                if (Program.Config.Item("ManaPotion").GetValue<bool>())
                {
                    if (ObjectManager.Player.ManaPercentage() <= Program.Config.Item("ManaPercent").GetValue<Slider>().Value)
                    {
                        var manaSlot = GetPotionSlot(PotionType.Mana);
                        if (!IsBuffActive(PotionType.Mana))
                            ObjectManager.Player.Spellbook.CastSpell(manaSlot.SpellSlot);
                    }
                }
            }

            catch (Exception)
            {

            }
        }

        private InventorySlot GetPotionSlot(PotionType type)
        {
            return (from potion in potions
                    where potion.TypeList.Contains(type)
                    from item in ObjectManager.Player.InventoryItems
                    where item.Id == potion.ItemId && item.Charges >= potion.MinCharges
                    select item).FirstOrDefault();
        }

        private bool IsBuffActive(PotionType type)
        {
            return (from potion in potions
                    where potion.TypeList.Contains(type)
                    from buff in ObjectManager.Player.Buffs
                    where buff.Name == potion.Name && buff.IsActive
                    select potion).Any();
        }
    }
}