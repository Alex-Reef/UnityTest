using UnityEngine;

namespace LevelsSystem.SO
{
    [CreateAssetMenu(fileName = "Levels List Config", menuName = "Game/Levels List", order = 0)]
    public class LevelsListConfig : ScriptableObject
    {
        [SerializeField] private string categoryName;
        [field:SerializeField] public LevelConfig[] Levels { get; private set; }

        public string CategoryName => $"Trace {categoryName}";
    }
}