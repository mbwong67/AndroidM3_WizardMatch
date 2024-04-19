using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardMatch
{
    [CreateAssetMenu(fileName = "SpawnList", menuName = "WizardMatch/SpawnList",order = 2)]
    public class LevelInfo : ScriptableObject
    {
        public List<GameObject> spawnableCharacters = new List<GameObject>();
        public List<Vector3> spawnPositions = new List<Vector3>();
        public string SpawnlistName = "Default";
    }
}