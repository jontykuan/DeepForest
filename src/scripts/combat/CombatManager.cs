using Godot;
using System;
using DeepForest.Core;
using DeepForest.Character;

namespace DeepForest.Combat;

public partial class CombatManager : Node
{
    public static CombatManager Instance { get; private set; } = null!;

    [Signal] public delegate void CombatStartedEventHandler(string enemyName);
    [Signal] public delegate void CombatEndedEventHandler(bool won);

    public Enemy? CurrentEnemy { get; private set; }
    public bool IsInCombat { get; private set; } = false;

    public override void _Ready()
    {
        Instance = this;
    }

    public void StartCombat(Enemy enemy)
    {
        CurrentEnemy = (Enemy)enemy.Duplicate();
        IsInCombat = true;
        
        GameState.Instance.AddLog($"【戰鬥開始】遭遇了 {CurrentEnemy.EnemyName}！");
        EmitSignal(SignalName.CombatStarted, CurrentEnemy.EnemyName);
    }

    public void ProcessPlayerAttack(int strPlayed)
    {
        if (!IsInCombat || CurrentEnemy == null) return;

        int damage = strPlayed * 3;
        CurrentEnemy.CurrentHp = Math.Max(0, CurrentEnemy.CurrentHp - damage);
        
        GameState.Instance.AddLog($"你對 {CurrentEnemy.EnemyName} 造成了 {damage} 點物理傷害！({CurrentEnemy.CurrentHp} HP 剩餘)");

        if (CurrentEnemy.CurrentHp <= 0)
        {
            EndCombat(true);
        }
        else
        {
            EnemyTurn();
        }
    }

    private void EnemyTurn()
    {
        if (!IsInCombat || CurrentEnemy == null) return;

        Player player = GameState.Instance.PlayerInstance;
        
        int dmg = CurrentEnemy.BaseDamage;
        int sanDmg = CurrentEnemy.BaseSanityDamage;

        if (player.Brutality > 30)
        {
            dmg = (int)(dmg * 1.2f);
        }

        player.CurrentHp -= dmg;
        player.CurrentSanity -= sanDmg;

        GameState.Instance.AddLog($"{CurrentEnemy.EnemyName} 發動襲擊！造成了 {dmg} 點體力傷害與 {sanDmg} 點理智打擊。");

        if (player.CurrentHp <= 0)
        {
            GameState.Instance.AddLog("你傷勢過重，倒在了森林深處...");
            EndCombat(false);
        }
    }

    private void EndCombat(bool won)
    {
        IsInCombat = false;
        string enemyName = CurrentEnemy?.EnemyName ?? "未知";
        GameState.Instance.AddLog(won ? $"【戰鬥勝利】擊敗了 {enemyName}！" : "【戰鬥失敗】");
        
        if (won)
        {
            Player player = GameState.Instance.PlayerInstance;
            player.Brutality += 5;
            GameState.Instance.AddLog("戰鬥激發了你的暴戾氣息。");
        }

        EmitSignal(SignalName.CombatEnded, won);
    }
}
