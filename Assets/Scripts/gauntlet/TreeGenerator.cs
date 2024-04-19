using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardMatch;

public class TreeGenerator : MonoBehaviour
{
    [SerializeField] private List<LevelInfo> levels = new List<LevelInfo>();
    [SerializeField] private GameObject levelPointObject;
    [SerializeField] private GameObject waitingPointObject;
    [SerializeField] private Vector2 beginningSize;



    
    void Awake()
    {
        if (!levelPointObject)
        {
            Debug.LogError("ERROR : Tree Generator does not have a prefab for level points! Aborting...");
            Destroy(gameObject);
        }
        if (levels.Count == 0)
        {
            Debug.LogError("ERROR : Tree Generator does not have any levels! Aborting...");
            Destroy(gameObject);
        }
        if (beginningSize == Vector2.zero)
        {
            Debug.LogWarning("ERROR : Tree Generator does not have a beginning scale vector! Using default value of (5.0f,5.0f)");
            beginningSize = new Vector2(5.0f,5.0f);
        }
    }

    void GenerateLevels()
    {

    }

}
