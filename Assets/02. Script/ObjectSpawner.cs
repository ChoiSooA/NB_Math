using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectSpawner : MonoBehaviour
{
    [HideInInspector] public GameObject objectPrefab;
    [HideInInspector] public Transform spawnCenter;
    [HideInInspector] public int maxObjects = 10;
    [HideInInspector] public float radius = 0.6f;
    [HideInInspector] public float objectScale = 0.3f;
    [HideInInspector] public Material objectMat;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    TouchObjectDetector touchObjectDetector;

    public int NowCount => spawnedObjects.Count;

    private void Awake()
    {
        touchObjectDetector = FindObjectOfType<TouchObjectDetector>();
        if (touchObjectDetector != null)
        {
            touchObjectDetector.touchObj = gameObject;
        }
    }

    public void SpawnOne()
    {
        if (spawnedObjects.Count >= maxObjects) return;

        GameObject obj = Instantiate(objectPrefab, spawnCenter.position, Quaternion.identity);
        obj.transform.localScale = Vector3.zero;
        obj.transform.SetParent(spawnCenter, true);

        // GlassBall 찾아서 Material 적용
        Transform glassBall = obj.transform.Find("GlassBall");
        if (glassBall != null && objectMat != null)
        {
            var renderer = glassBall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = objectMat;
            }
        }

        spawnedObjects.Add(obj);
        RearrangeObjects();
    }

    public void DespawnOne()
    {
        if (spawnedObjects.Count == 0) return;

        GameObject obj = spawnedObjects[spawnedObjects.Count - 1];
        spawnedObjects.RemoveAt(spawnedObjects.Count - 1);
        obj.transform.DOKill();
        obj.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => Destroy(obj));
        RearrangeObjects();
    }

    public void RemoveObject()
    {
        if (touchObjectDetector == null || !spawnedObjects.Contains(touchObjectDetector.touchObj)) return;

        GameObject obj = touchObjectDetector.touchObj;
        spawnedObjects.Remove(obj);
        obj.transform.DOKill();
        obj.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(obj);
            RearrangeObjects();
        });
    }

    public void ResetAll()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                obj.transform.DOKill();
                obj.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                    .OnComplete(() => Destroy(obj));
            }
        }

        spawnedObjects.Clear();
    }

    public void SpawnMultiple(int count)
    {
        ResetAll();
        int spawnCount = Mathf.Min(count, maxObjects);

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject obj = Instantiate(objectPrefab, spawnCenter.position, Quaternion.identity);
            obj.transform.localScale = Vector3.zero;
            obj.transform.SetParent(spawnCenter, true);

            // GlassBall 찾아서 Material 적용
            Transform glassBall = obj.transform.Find("GlassBall");
            if (glassBall != null && objectMat != null)
            {
                var renderer = glassBall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = objectMat;
                }
            }

            spawnedObjects.Add(obj);
        }

        RearrangeObjects();
    }

    void RearrangeObjects()
    {
        int count = spawnedObjects.Count;
        Vector3 center = spawnCenter.position;

        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            Vector3 targetPos = center + new Vector3(x, y, 0f);

            GameObject obj = spawnedObjects[i];
            obj.transform.DOKill();
            obj.transform.DOScale(objectScale, 0.3f);
            obj.transform.DOMove(targetPos, 0.5f).SetEase(Ease.OutCubic);
        }
    }
}
