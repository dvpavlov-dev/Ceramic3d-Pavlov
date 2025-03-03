using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ceramic_3d_Pavlov.Scripts
{
    public static class JsonService
    {
        public static void SaveMatrices(List<Matrix4x4> matrices)
        {
            Matrix4x4Array matrixArray = new Matrix4x4Array();
            matrixArray.matrices = new Matrix4x4Data[matrices.Count];

            for (int i = 0; i < matrices.Count; i++)
            {
                matrixArray.matrices[i] = ConvertToMatrix4x4Data(matrices[i]);
            }

            string json = JsonUtility.ToJson(matrixArray, true);

            File.WriteAllText(Application.streamingAssetsPath + "/offsets.json", json);

            Debug.Log("Matrices saved to: " + Application.streamingAssetsPath + "/offsets.json");
        }
        
        public static List<Matrix4x4> LoadMatrixData(string path)
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonHelper.FromJson(json);
            }
            
            Debug.LogError("Cannot find file: " + path);
            return new List<Matrix4x4>(Array.Empty<Matrix4x4>());
        }

        private static Matrix4x4Data ConvertToMatrix4x4Data(Matrix4x4 matrix)
        {
            return new Matrix4x4Data
            {
                m00 = matrix.m00,
                m10 = matrix.m10,
                m20 = matrix.m20,
                m30 = matrix.m30,
                m01 = matrix.m01,
                m11 = matrix.m11,
                m21 = matrix.m21,
                m31 = matrix.m31,
                m02 = matrix.m02,
                m12 = matrix.m12,
                m22 = matrix.m22,
                m32 = matrix.m32,
                m03 = matrix.m03,
                m13 = matrix.m13,
                m23 = matrix.m23,
                m33 = matrix.m33
            };
        }
        
        [Serializable]
        public struct Matrix4x4Data
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
        public struct Matrix4x4Array
        {
            public Matrix4x4Data[] matrices;
        }

        private static class JsonHelper
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
}
