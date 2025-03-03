using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ceramic_3d_Pavlov.Scripts
{
    public class MatrixMatcher : MonoBehaviour
    {
        public int IterationCount = 10;
        
        [SerializeField] private GameObject _cubePrefab;
        [SerializeField] private GameObject _linePrefab;

        private List<Matrix4x4> _modelMatrices;
        private List<Matrix4x4> _spaceMatrices;
        private List<Matrix4x4> _offsetMatrices = new();
        
        private GameObject _modelCube;
        private readonly List<GameObject> _lines = new();
        
        private CancellationTokenSource _cancellationTokenSource;

        private void Start()
        {
            LoadMatrices();
            
            Debug.Log("Starting the search for offsets");
            _cancellationTokenSource = new CancellationTokenSource();
            _ = FindOffsetsUseInverse(IterationCount, _cancellationTokenSource.Token);
        }

        private void LoadMatrices()
        {
            string modelPath = Path.Combine(Application.streamingAssetsPath, "model.json");
            string spacePath = Path.Combine(Application.streamingAssetsPath, "space.json");

            _modelMatrices = JsonService.LoadMatrixData(modelPath);
            _spaceMatrices = JsonService.LoadMatrixData(spacePath);
        }

        private async Task FindOffsetsUseInverse(int iterationCount, CancellationToken  cancellationToken)
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

                    await FindOffsets(modelMatrix, oneIterationNumber, i);
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

            Debug.Log("Matrix calculated!");
            JsonService.SaveMatrices(_offsetMatrices);
        }

        private Task FindOffsets(Matrix4x4 modelMatrix, int oneIterationNumber, int index)
        {
            for (int j = oneIterationNumber * (index - 1); j < oneIterationNumber * index; j++)
            {
                Matrix4x4 modelInverse = Matrix4x4.Inverse(modelMatrix);
            
                Matrix4x4 offsetMatrix = modelInverse * _spaceMatrices[j];
                _offsetMatrices.Add(offsetMatrix);
                
                VisualizeFind(offsetMatrix);
            }
            
            return Task.CompletedTask;
        }

        private void VisualizeFind(Matrix4x4 shift)
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
}
