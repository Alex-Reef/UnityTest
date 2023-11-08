using AlexDevTools.Models;
using LevelsSystem.Models;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils;

namespace LevelsSystem.SO
{
    [CreateAssetMenu(fileName = "Level Config", menuName = "Game/Level Config", order = 0)]
    public class LevelConfig : ScriptableObject
    {
        public DrawPath[] DrawRoads;
        public KeyValue<Sprite, Image>[] Parts;
        public KeyValue<Sprite, Image>[] Hints;
        public Sprite FullImage;

        public AudioClip TaskAudio;
    }
}