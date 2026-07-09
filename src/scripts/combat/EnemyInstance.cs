using Godot;

namespace DeepForest.Combat
{
    [GlobalClass]
    public partial class EnemyInstance : Resource
    {
        [Export] public EnemyData EnemyData { get; set; } = null!;
        [Export] public int CurrentHp { get; set; }

        public EnemyInstance() {}

        public EnemyInstance(EnemyData data)
        {
            EnemyData = data;
            CurrentHp = data.MaxHp;
        }
    }
}
