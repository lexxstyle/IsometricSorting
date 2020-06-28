using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapConstructor : MonoBehaviour
{
    [SerializeField] private GameObject PrefabCell;

    [SerializeField] private int Columns;
    [SerializeField] private int Rows;

    [SerializeField] private float spriteWidth = 2.56f;
    [SerializeField] private float spriteHeight = 1.29f;

    [SerializeField] [HideInInspector] private GameObject[] generatedObjects;

    [SerializeField] [HideInInspector] private bool isGenerated = false;
    
    private void Start()
    {
        if (!isGenerated)
            GenerateNewMap();
    }

    public void GenerateNewMap()
    {
        CleanMap();
        
        generatedObjects = new GameObject[Columns * Rows];

        int TotalObjects = 0;
        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                float posX = x * spriteWidth;

                if (y % 2 == 1)
                {
                    posX += spriteWidth / 2;
                }

                GameObject _newCell = Instantiate(PrefabCell, Vector3.zero, Quaternion.identity);
                
                Transform _transform = _newCell.transform;
                _transform.parent = transform;
                _transform.localPosition = new Vector3(posX, y * spriteHeight / 2, 0);
                _transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));

                generatedObjects[TotalObjects] = _newCell;
                TotalObjects++;
            }
        }

        isGenerated = true;
    }

    public void CleanMap()
    {
        if (generatedObjects != null && generatedObjects.Length > 0)
        {
            for (int i = 0; i < generatedObjects.Length; i++)
            {
                GameObject _gameObject = generatedObjects[i];
                if (_gameObject != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(_gameObject);
                    else
                        Destroy(_gameObject);
#else
                    Destroy(_gameObject);
#endif
                }
            }
        }
    }
}
