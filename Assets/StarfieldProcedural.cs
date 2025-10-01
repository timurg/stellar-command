using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class StarfieldProcedural : MonoBehaviour
{
    [SerializeField] private Material starMaterial; // Назначь StarfieldMaterial
    [SerializeField] private int starCount = 500; // Количество звёзд
    [SerializeField] private float renderDistance = 20f; // Радиус видимости
    [SerializeField] private float starSizeMin = 0.01f; // Минимальный размер
    [SerializeField] private float starSizeMax = 0.1f; // Максимальный размер

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("StarfieldProcedural: No main camera found!");
            enabled = false;
            return;
        }
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer.material = starMaterial;

        Mesh mesh = new Mesh { name = "StarfieldMesh" };
        meshFilter.sharedMesh = mesh;
        GenerateStarfieldMesh();
    }

    private void Update()
    {
        Vector2 cameraPos = mainCamera.transform.position;
        meshRenderer.material.SetVector("_CameraOffset", new Vector4(cameraPos.x, cameraPos.y, 0, 0));
        // Перегенерируем мешик при значительном сдвиге камеры (опционально)
        if (Vector2.Distance(transform.position, cameraPos) > renderDistance * 0.5f)
        {
            GenerateStarfieldMesh();
            transform.position = cameraPos; // Сдвигаем мешик к камере
        }
    }

    private void GenerateStarfieldMesh()
    {
        Vector2 cameraPos = mainCamera.transform.position;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < starCount; i++)
        {
            Vector2 starPos = GetStarPosition(cameraPos);
            float size = Random.Range(starSizeMin, starSizeMax);

            Vector3 center = new Vector3(starPos.x, starPos.y, 0);
            Vector3 halfSize = new Vector3(size * 0.5f, size * 0.5f, 0);
            vertices.Add(center + new Vector3(-halfSize.x, -halfSize.y, 0)); // Нижний левый
            vertices.Add(center + new Vector3(halfSize.x, -halfSize.y, 0));  // Нижний правый
            vertices.Add(center + new Vector3(halfSize.x, halfSize.y, 0));   // Верхний правый
            vertices.Add(center + new Vector3(-halfSize.x, halfSize.y, 0));  // Верхний левый

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));

            int vertexIndex = i * 4;
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }

        meshFilter.sharedMesh.Clear(); // Очищаем перед обновлением
        meshFilter.sharedMesh.vertices = vertices.ToArray();
        meshFilter.sharedMesh.uv = uvs.ToArray();
        meshFilter.sharedMesh.triangles = triangles.ToArray();
        meshFilter.sharedMesh.RecalculateBounds(); // Обновляем границы для рендера
        Debug.Log($"Generated {starCount} stars around {cameraPos}");
    }

    private Vector2 GetStarPosition(Vector2 cameraPos)
    {
        float x = cameraPos.x + Random.Range(-renderDistance, renderDistance);
        float y = cameraPos.y + Random.Range(-renderDistance, renderDistance);
        return new Vector2(x, y);
    }
}