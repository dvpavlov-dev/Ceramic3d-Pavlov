using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ceramic_3d_Pavlov.Scripts
{
    public class MatrixMatcher : MonoBehaviour
    {
        public int IterationCount = 100;
        
        [SerializeField] private GameObject _cubePrefab;
        [SerializeField] private GameObject _linePrefab;

        private List<Matrix4x4> _modelMatrices;
        private List<Matrix4x4> _spaceMatrices;
        
        private GameObject _modelCube;
        private readonly List<GameObject> _lines = new();
        
        private CancellationTokenSource _cancellationTokenSource;

        private void Start()
        {
            LoadMatrices();
            
            _cancellationTokenSource = new CancellationTokenSource();
            _ = FindShiftsUseInverse(IterationCount, _cancellationTokenSource.Token);
        }

        private void LoadMatrices()
        {
            string modelPath = Path.Combine(Application.streamingAssetsPath, "model.json");
            string spacePath = Path.Combine(Application.streamingAssetsPath, "space.json");

            _modelMatrices = LoadMatrixData(modelPath);
            _spaceMatrices = LoadMatrixData(spacePath);
        }

        private List<Matrix4x4> LoadMatrixData(string path)
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonHelper.FromJson(json);
            }
            else
            {
                Debug.LogError("Cannot find file: " + path);
                return new List<Matrix4x4>(Array.Empty<Matrix4x4>());
            }
        }

        private async Task FindShiftsUseInverse(int iterationCount, CancellationToken  cancellationToken)
        {
            int oneIterationNumber = _spaceMatrices.Count / iterationCount;
            
            foreach (Matrix4x4 modelMatrix in _modelMatrices)
            {
                for (int i = 1; i <= iterationCount; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    Vector3 modelPosition = new Vector3(modelMatrix.m03, modelMatrix.m13, modelMatrix.m23);
                    _modelCube = Instantiate(_cubePrefab, modelPosition, Quaternion.identity);
                    _modelCube.GetComponent<Renderer>().material.color = Color.blue;

                    await FindShift(modelMatrix, oneIterationNumber, i);
                    await Task.Delay(50);

                    foreach (GameObject line in _lines)
                    {
                        DestroyImmediate(line);
                    }

                    DestroyImmediate(_modelCube);

                    _lines.Clear();
                }
                
                await Task.Delay(50);
            }
        }

        private Task FindShift(Matrix4x4 modelMatrix, int oneIterationNumber, int index)
        {
            for (int j = oneIterationNumber * (index - 1); j < oneIterationNumber * index; j++)
            {
                Matrix4x4 modelInverse = Matrix4x4.Inverse(modelMatrix);
            
                Matrix4x4 offsetMatrix = modelInverse * _spaceMatrices[j];
            
                VisualizeShift(offsetMatrix);
            }
            
            return Task.CompletedTask;
        }

        private void VisualizeShift(Matrix4x4 shift)
        {
            Vector3 spacePosition = new Vector3(shift.m03, shift.m13, shift.m23);
            
            VisualizeLine(_modelCube.transform.position, spacePosition);
        }

        private void VisualizeLine(Vector3 start, Vector3 end)
        {
            if (_linePrefab != null)
            {
                GameObject lineObject = Instantiate(_linePrefab);
                LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
                lineRenderer.SetPosition(0, start);
                lineRenderer.SetPosition(1, end);
                
                _lines.Add(lineObject);
            }
        }

        private void OnDestroy()
        {
            _cancellationTokenSource.Cancel();
        }
    }
    
    [Serializable]
    public class Matrix4x4Data
    {
        public float m00;
        public float m10;
        public float m20;
        public float m30;
        public float m01;
        public float m11;
        public float m21;
        public float m31;
        public float m02;
        public float m12;
        public float m22;
        public float m32;
        public float m03;
        public float m13;
        public float m23;
        public float m33;
    }
    
    [Serializable]
    public class Matrix4x4Array
    {
        public Matrix4x4Data[] matrices;
    }
    
    public static class JsonHelper
    {
        public static List<Matrix4x4> FromJson(string json)
        {
            string wrappedJson = "{\"matrices\":" + json + "}";
            Matrix4x4Array array = JsonUtility.FromJson<Matrix4x4Array>(wrappedJson);

            List<Matrix4x4> matrices = new List<Matrix4x4>();
            foreach (var matrixData in array.matrices)
            {
                matrices.Add(new Matrix4x4(
                    new Vector4(matrixData.m00, matrixData.m10, matrixData.m20, matrixData.m30),
                    new Vector4(matrixData.m01, matrixData.m11, matrixData.m21, matrixData.m31),
                    new Vector4(matrixData.m02, matrixData.m12, matrixData.m22, matrixData.m32),
                    new Vector4(matrixData.m03, matrixData.m13, matrixData.m23, matrixData.m33)
                ));
            }
            
            return matrices;
        }
    }
}
