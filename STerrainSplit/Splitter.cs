using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

namespace STerrainSplit
{
    public class Splitter
    {

        public int terrainsCountX = 1;
        public int terrainsCountZ = 1;

        string progressCaptionBase;
        string progressCaption;

        //string num = "4";

        List<TerrainData> terrainData = new List<TerrainData>();
        List<GameObject> terrainGo = new List<GameObject>();

        //Terrain parentTerrain;

        public Splitter(int terrainsCountX, int terrainsCountZ)
        {
            this.terrainsCountX = terrainsCountX;
            this.terrainsCountZ = terrainsCountZ;
        }


        int currentObjectIndex;

        public void SplitIt()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogWarning("No terrain was selected");
                return;
            }

            var objects = Selection.objects;
            currentObjectIndex = 0;

            foreach (Object obj in objects)
            {
                SplitObject(obj, objects.Length);
            }
        }

        /// <summary>
        /// Split one object
        /// </summary>
        /// <param name="obj"></param>
        public void SplitObject(Object obj, int length)
        {
            currentObjectIndex++;

            GameObject tObj = obj as GameObject;
            if (!Utils.CheckValidGameObject(tObj)) return;


            progressCaptionBase = "Spliting terrain " + tObj.name + " (" + currentObjectIndex.ToString() + " of " + length.ToString() + ")";



            for (int j = 0; j < terrainsCountZ; j++)
            {
                for (int i = 0; i < terrainsCountZ; i++)
                {

                    int number = (terrainsCountZ * j) + i;
                    progressCaption = progressCaptionBase + " - " + number + "/" + i * j;
                    EditorUtility.DisplayProgressBar(progressCaption, "Process " + number, (float)number / terrainsCountX * terrainsCountZ);

                    TerrainData td = new TerrainData();
                    GameObject tgo = Terrain.CreateTerrainGameObject(td);
                    ProcessTerrainData(td, tgo, number, i, j, tObj);

                    AssetDatabase.SaveAssets();

                }
            }
            EditorUtility.ClearProgressBar();

        }

        public void ProcessTerrainData(TerrainData td, GameObject tgo, int number, int x, int z, GameObject tObj)
        {

            Terrain parentTerrain = tObj.GetComponent<Terrain>() as Terrain;
            tgo.name = parentTerrain.name + " " + number;

            terrainData.Add(td);
            terrainGo.Add(tgo);

            Terrain genTer = tgo.GetComponent(typeof(Terrain)) as Terrain;
            genTer.terrainData = td;

            AssetDatabase.CreateAsset(td, "Assets/" + genTer.name + ".asset");

            ProcessParentProperties(genTer, parentTerrain);
            TranslatePosition(tgo, x, z, parentTerrain);
            
            genTer.terrainData.SetHeights(0, 0, SplitHeight(td, parentTerrain, x, z));
            genTer.terrainData.SetAlphamaps(0, 0, SplitSplatMap(td,tgo,parentTerrain,x,z));

        }

        public void ProcessParentProperties(Terrain genTer, Terrain parentTerrain)
        {
            // Assign splatmaps
            genTer.terrainData.splatPrototypes = parentTerrain.terrainData.splatPrototypes;

            // Assign detail prototypes
            genTer.terrainData.detailPrototypes = parentTerrain.terrainData.detailPrototypes;

            // Assign tree information
            genTer.terrainData.treePrototypes = parentTerrain.terrainData.treePrototypes;


            genTer.basemapDistance = parentTerrain.basemapDistance;
            genTer.castShadows = parentTerrain.castShadows;
            genTer.detailObjectDensity = parentTerrain.detailObjectDensity;
            genTer.detailObjectDistance = parentTerrain.detailObjectDistance;
            genTer.heightmapMaximumLOD = parentTerrain.heightmapMaximumLOD;
            genTer.heightmapPixelError = parentTerrain.heightmapPixelError;
            genTer.treeBillboardDistance = parentTerrain.treeBillboardDistance;
            genTer.treeCrossFadeLength = parentTerrain.treeCrossFadeLength;
            genTer.treeDistance = parentTerrain.treeDistance;
            genTer.treeMaximumFullLODCount = parentTerrain.treeMaximumFullLODCount;
        }

        public void TranslatePosition(GameObject tgo, int x, int z, Terrain parentTerrain)
        {

            Vector3 parentPosition = parentTerrain.GetPosition();

            float spaceX = parentTerrain.terrainData.size.x / terrainsCountX;
            float spaceZ = parentTerrain.terrainData.size.z / terrainsCountZ;

            float positionX = x * spaceX;
            float positionZ = z * spaceZ;

            tgo.transform.position = new Vector3(tgo.transform.position.x + positionX,
                                                  tgo.transform.position.y,
                                                  tgo.transform.position.z + positionZ);

            // Shift last position
            //tgo.transform.position = new Vector3(tgo.transform.position.x + parentPosition.x,
            //                                      tgo.transform.position.y + parentPosition.y,
            //                                      tgo.transform.position.z + parentPosition.z
            //                                     );
        }


        public float[,] SplitHeight(TerrainData td, Terrain parentTerrain, int numberx, int numberz)
        {
            Debug.Log("Split height");

            //Copy heightmap											
            td.heightmapResolution = parentTerrain.terrainData.heightmapResolution / terrainsCountX;

            //Keep y same
            td.size = new Vector3(parentTerrain.terrainData.size.x / terrainsCountX,
                                   parentTerrain.terrainData.size.y,
                                   parentTerrain.terrainData.size.z / terrainsCountZ
                                  );

            float[,] parentHeight = parentTerrain.terrainData.GetHeights(0, 0, parentTerrain.terrainData.heightmapResolution, parentTerrain.terrainData.heightmapResolution);

            float[,] peaceHeight = new float[parentTerrain.terrainData.heightmapResolution / terrainsCountX + 1,
                                              parentTerrain.terrainData.heightmapResolution / terrainsCountZ + 1
                                            ];

            int heightShift = parentTerrain.terrainData.heightmapResolution / terrainsCountX;

            int startX = 0;
            int startY = 0;

            int endX = parentTerrain.terrainData.heightmapResolution / terrainsCountX + 1; ;
            int endY = parentTerrain.terrainData.heightmapResolution / terrainsCountZ + 1; ;

            for (int x = startX; x < endX; x++)
            {
                EditorUtility.DisplayProgressBar(progressCaption, "Split height", (float)x / (endX - startX));

                for (int y = startY; y < endY; y++)
                {

                    int xShift = heightShift * numberz;
                    int yShift = heightShift * numberx;

                    float ph = parentHeight[x + xShift, y + yShift];

                    peaceHeight[x, y] = ph;
                }
            }

            EditorUtility.ClearProgressBar();
            return peaceHeight;

        }


        public float[,,] SplitSplatMap(TerrainData td, GameObject tgo, Terrain parentTerrain, int numberx, int numberz)
        {
            td.alphamapResolution = parentTerrain.terrainData.alphamapResolution / terrainsCountX;

            float[,,] parentSplat = parentTerrain.terrainData.GetAlphamaps(0, 0, parentTerrain.terrainData.alphamapResolution, parentTerrain.terrainData.alphamapResolution);

            float[,,] peaceSplat = new float[parentTerrain.terrainData.alphamapResolution / terrainsCountX,
                                              parentTerrain.terrainData.alphamapResolution / terrainsCountX,
                                              parentTerrain.terrainData.alphamapLayers
                                            ];

            // Shift calc
            int splatShift = parentTerrain.terrainData.alphamapResolution / terrainsCountX;

            int startX = 0;
            int startY = 0;

            int endX = parentTerrain.terrainData.alphamapResolution / terrainsCountX;
            int endY = parentTerrain.terrainData.alphamapResolution / terrainsCountX;

            // iterate
            for (int s = 0; s < parentTerrain.terrainData.alphamapLayers; s++)
            {
                for (int x = startX; x < endX; x++)
                {
                    EditorUtility.DisplayProgressBar(progressCaption, "Processing splat...", (float)x / (endX - startX));

                    for (int y = startY; y < endY; y++)
                    {

                        int xShift = splatShift * numberz; ;
                        int yShift = splatShift * numberx;

                        float ph = parentSplat[x + xShift, y + yShift, s];
                        peaceSplat[x, y, s] = ph;
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            return peaceSplat;

        }
    }
}
