using System;
using DeepForest.Cards;
using DeepForest.Core;
using DeepForest.Character;

namespace DeepForest.Combat
{
    public static class EnemyFactory
    {
        public static EnemyData GetEnemyDataForDecal(string decalType)
        {
            var enemy = new EnemyData();
            if (decalType == "combat_wolf")
            {
                enemy.EnemyName = "野狼";
                enemy.MaxHp = 3;
                enemy.AttackPower = 10;
                enemy.IsAggressive = true;
                enemy.HideHp = false;
                enemy.DecalName = "combat_wolf_right";
                
                enemy.ActionDeck.Add(new Card { CardName = "撕咬", CardType = CardType.ActionStr, StrValue = 2 });
                enemy.ActionDeck.Add(new Card { CardName = "猛撲", CardType = CardType.ActionDex, DexValue = 3 });
                enemy.ActionDeck.Add(new Card { CardName = "試探", CardType = CardType.ActionWis, WisValue = 1 });

                enemy.LootTable.Add(new Card { CardName = "狼肉", CardType = CardType.Consumable, Description = "新鮮帶血的狼肉。", HpCost = -8, HungerCost = -20 });
                enemy.LootTable.Add(new Card { CardName = "狼皮", CardType = CardType.Equipment, Description = "保暖粗糙的狼皮。", Weight = 2 });
            }
            else if (decalType == "combat_ghost")
            {
                enemy.EnemyName = "怨靈";
                enemy.MaxHp = 4;
                enemy.AttackPower = 15;
                enemy.IsAggressive = false;
                enemy.HideHp = true;
                enemy.DecalName = "combat_ghost_left";

                enemy.ActionDeck.Add(new Card { CardName = "陰冷低語", CardType = CardType.ActionWis, WisValue = 3 });
                enemy.ActionDeck.Add(new Card { CardName = "鬼影重重", CardType = CardType.ActionDex, DexValue = 2 });
            }
            else if (decalType == "combat_cultist")
            {
                enemy.EnemyName = "邪教徒";
                enemy.MaxHp = 4;
                enemy.AttackPower = 12;
                enemy.IsAggressive = true;
                enemy.HideHp = false;
                enemy.DecalName = "combat_cultist_left";

                enemy.ActionDeck.Add(new Card { CardName = "精神鞭笞", CardType = CardType.ActionWis, WisValue = 2 });
                enemy.ActionDeck.Add(new Card { CardName = "儀式匕首", CardType = CardType.ActionStr, StrValue = 2 });

                if (GameState.Instance.PlayerInstance.CharacterData?.CharacterId == CharacterId.Leo && Random.Shared.NextDouble() < 0.25)
                {
                    var scripture = CardFactory.CreateCard(CardId.KeyOldScripture);
                    if (scripture != null) enemy.LootTable.Add(scripture);
                }
            }
            else if (decalType == "combat_sarah")
            {
                enemy.EnemyName = "母親的幻影";
                enemy.MaxHp = 4;
                enemy.AttackPower = 12;
                enemy.IsAggressive = true;
                enemy.HideHp = false;
                enemy.DecalName = "combat_sarah_right";

                enemy.ActionDeck.Add(new Card { CardName = "巴掌", CardType = CardType.ActionStr, StrValue = 2 });
                enemy.ActionDeck.Add(new Card { CardName = "厲聲責備", CardType = CardType.ActionWis, WisValue = 2 });
                enemy.ActionDeck.Add(new Card { CardName = "拉扯", CardType = CardType.ActionDex, DexValue = 1 });

                var footprints = CardFactory.CreateCard(CardId.KeyTinyFootprints);
                if (footprints != null) enemy.LootTable.Add(footprints);
            }
            return enemy;
        }
    }
}
