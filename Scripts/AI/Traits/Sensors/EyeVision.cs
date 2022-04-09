using System.Collections.Generic;
using System.Linq;
using AI.Traits.Shared;
using UnityEngine;

namespace AI.Traits.Sensors
{
    [RequireComponent(typeof(MeshRenderer))]
    public class EyeVision : MonoBehaviour, ISensor
    {
        //color stuff for customization start
        [SerializeField]
        [Range(0,1)]
        public float fovRed = 1f;
        [SerializeField]
        [Range(0,1)]
        public float fovGreen = 0.0f; 
        [SerializeField]
        [Range(0,1)]
        public float fovBlue = 0.0f;
        [SerializeField]
        [Range(0,1)]
        public float fovAlpha = 0.23f;
        //color stuff for customization end
        //Start is called before the first frame update
        private float viewDistance = 10f;
        private float viewAngle = 89f;
        private float height = 1.0f;
        private Color meshColor;
        private int scanFrequency = 30;
        public LayerMask layers;
        public LayerMask occlusionLayers;
        Collider[] collliders = new Collider[50];
        Mesh mesh;
        int count;
        float scanInterval;
        float scanTimer;
        
        //Variables to link this object with the main list (START)
        //shared stuff:
        private SortedList<int, int> amountOfSensorsPickingUpEachObject = new SortedList<int, int>();
        private SortedList<float, int> indexesByDistance = new SortedList<float, int>();
        private SortedList<int, DetectedObject> controlListOfIndexesAndPackedObjects = new SortedList<int, DetectedObject>();
        private List<int> missingIndexesControlList = new List<int>();
        //local stuff:
        private SortedList<float, GameObject> objects = new SortedList<float, GameObject>();
        private SortedList<float, GameObject> objectsCopy = new SortedList<float, GameObject>();
        private List<int> sensorManagedIndexedObjectsMarkedForRemoval = new List<int>();
        private SortedList<int, int> sensorManagedIndexedObjects = new SortedList<int, int>();
        private bool databaseIsOnline;
        private bool databaseIsOnline1;
        private bool databaseIsOnline2;
        private bool databaseIsOnline3;
        private bool databaseIsOnline4;
        //Variables to link this object with the main list (FINISH)
        
        //linking (start)!
        private void CheckIfDataSetIsComplete()
        {
            if (!databaseIsOnline1 || !databaseIsOnline2 || !databaseIsOnline3 || !databaseIsOnline4) return;
            databaseIsOnline = true;
            //Debug.Log("Sensors Online");
        }

        public bool DatabaseIsOnline()
        {
            return databaseIsOnline;
        }

        public void SetControlListOfIndexesAndPackedObjects(SortedList<int, DetectedObject> controlListOfIndexesAndPackedObjectsInput) {
            controlListOfIndexesAndPackedObjects = controlListOfIndexesAndPackedObjectsInput;
            databaseIsOnline1 = true;
            CheckIfDataSetIsComplete();
        }
        public void SetMissingIndexesControlList(List<int> missingIndexesControlListInput) {
            missingIndexesControlList = missingIndexesControlListInput;
            databaseIsOnline2 = true;
            CheckIfDataSetIsComplete();
        }
        public void SetAmountOfSensorsPickingUpEachObject(SortedList<int, int> amountOfSensorsPickingUpEachObjectInput) {
            amountOfSensorsPickingUpEachObject = amountOfSensorsPickingUpEachObjectInput;
            databaseIsOnline3 = true;
            CheckIfDataSetIsComplete();
        }
        public void SetIndexesByDistance(SortedList<float, int> indexesByDistanceInput) {
            indexesByDistance = indexesByDistanceInput;
            databaseIsOnline4 = true;
            CheckIfDataSetIsComplete();
        }
        //Done linking! (FINISH)

        // Start is called before the first frame update
        void Start()
        {
            meshColor = new Color(fovRed, fovGreen, fovBlue, fovAlpha);
            scanInterval = 1.0f / scanFrequency;
            layers = LayerMask.GetMask("Entities");
            occlusionLayers = LayerMask.GetMask("Solid");
        }
        
        void Update()
        {
            //meshColor = new Color(FOVRed, FOVGreen, FOVBlue, FOVAlpha); //this will allow unity to change the color in real time
            if (databaseIsOnline)
            {
                
                scanTimer -= Time.deltaTime;
                if (scanTimer < 0)
                {
                    scanTimer += scanInterval;
                    Scan();
                }
                objectsCopy.Clear();
                // start of object sorting
                foreach (KeyValuePair<float, GameObject> detected in objects)
                {
                    //check if its sensor handled
                    if (!sensorManagedIndexedObjects.ContainsValue(detected.Value.GetInstanceID()))
                    {
                        //this sensor does not know this object but does anybody else do?
                        //check if its already handled globally
                        if (amountOfSensorsPickingUpEachObject.ContainsValue(detected.Value.GetInstanceID()))
                        {
                            //update distance start
                            //only update distances. //this is ugly as hell but should work.
                            controlListOfIndexesAndPackedObjects
                                .Values[
                                    amountOfSensorsPickingUpEachObject[
                                        amountOfSensorsPickingUpEachObject.IndexOfValue(
                                            detected.Value.GetInstanceID())]].Distance = detected.Key;
                            //packed object distance updated. //this is ugly as hell but should work.
                            indexesByDistance.Keys[
                                indexesByDistance.IndexOfValue(amountOfSensorsPickingUpEachObject.Keys[
                                    amountOfSensorsPickingUpEachObject.IndexOfValue(
                                        detected.Value.GetInstanceID())])] = detected.Key;
                            //update distance end.
                            //make it ALSO handled by this sensor.
                            sensorManagedIndexedObjects.Add(sensorManagedIndexedObjects.Keys[sensorManagedIndexedObjects.IndexOfValue(detected.Value.GetInstanceID())],detected.Value.GetInstanceID());
                            //add amount of sensors picking up this prick.
                            amountOfSensorsPickingUpEachObject[sensorManagedIndexedObjects.IndexOfValue(detected.Value.GetInstanceID())]++;
                        }
                        else //its a new object.
                        {
                            //any holes on the main control list?
                            if (missingIndexesControlList.Any())
                            {
                                //add to local managed
                                sensorManagedIndexedObjects.Add(missingIndexesControlList[0],detected.Value.GetInstanceID());
                                //add to sensor lists id and 1 since its really new.
                                amountOfSensorsPickingUpEachObject.Add(missingIndexesControlList[0],1);
                                //add to actual objects
                                DetectedObject detectedObject = new DetectedObject(1, detected.Key, detected.Value,
                                    missingIndexesControlList[0]);
                                controlListOfIndexesAndPackedObjects.Add(missingIndexesControlList[0], detectedObject); 
                                //create new index by distance
                                indexesByDistance.Add(detected.Key,missingIndexesControlList[0]);
                                //deleting entry from missing indexes!
                                missingIndexesControlList.RemoveAt(0);
                            }
                            else // its a new object but there are no missing indexes.
                            {
                                //case no new holes on list!
                                //create new object with next available index (index + 1)
                                DetectedObject detectedObject = new DetectedObject(1, detected.Key, detected.Value,
                                    controlListOfIndexesAndPackedObjects.Count + 1);
                                //add it to the list that actually keep the indexes:
                                controlListOfIndexesAndPackedObjects.Add(controlListOfIndexesAndPackedObjects.Count + 1,detectedObject);
                                //add to local managed !!!!(beware the count here is not +1 since the new entry changed the count!)!!!!
                                sensorManagedIndexedObjects.Add(controlListOfIndexesAndPackedObjects.Count,detected.Value.GetInstanceID());
                                //add to sensor lists id and 1 since its really new.
                                amountOfSensorsPickingUpEachObject.Add(controlListOfIndexesAndPackedObjects.Count,1);
                                //create new index by distance
                                indexesByDistance.Add(detected.Key,controlListOfIndexesAndPackedObjects.Count);
                            }
                        }
                    }
                    else // its an already know and managed object, just update the distances.
                    {
                        //update distance start
                        //only update distances. //this is ugly as hell but should work.
                        controlListOfIndexesAndPackedObjects.Values
                            [controlListOfIndexesAndPackedObjects.IndexOfKey(sensorManagedIndexedObjects.Keys[sensorManagedIndexedObjects.IndexOfValue(detected.Value.GetInstanceID())])].Distance = detected.Key;
                        //packed object distance updated. //this is ugly as hell but should work.
                        indexesByDistance.RemoveAt(indexesByDistance.IndexOfValue(sensorManagedIndexedObjects.Keys[sensorManagedIndexedObjects.IndexOfValue(detected.Value.GetInstanceID())]));
                        indexesByDistance.Add(detected.Key,sensorManagedIndexedObjects.Keys[sensorManagedIndexedObjects.IndexOfValue(detected.Value.GetInstanceID())]);
                        
                        //update distance end.
                    }
                    //copy object to cleanup list!
                    objectsCopy.Add(detected.Key,detected.Value);
                } // end of object sorting
                //start of object cleanup
                //Start by loading up managedObjectsList.
                foreach (KeyValuePair<int, int> managedObjectToBeValidated in sensorManagedIndexedObjects)
                {
                    //check if the object its in this round's object list.
                    bool found = false;
                    float keyOfFound = 0;
                    foreach (KeyValuePair<float,GameObject> copiedListData in objectsCopy)
                    {
                        if (managedObjectToBeValidated.Value != copiedListData.Value.GetInstanceID()) continue;
                        found = true;
                        keyOfFound = copiedListData.Key;
                        break;
                    }
                    if (found)
                    {
                        //object is validated and present.
                        objectsCopy.Remove(keyOfFound);
                    }
                    else
                    {
                      //object failed to validated.
                      //check if there are any other sensors validating this object
                      if (amountOfSensorsPickingUpEachObject.Values[amountOfSensorsPickingUpEachObject.IndexOfKey(managedObjectToBeValidated.Key)] == 1)
                      {
                          //this object is only kept by this sensor, complete delete it.
                          //delete from index - distance list
                          indexesByDistance.RemoveAt(indexesByDistance.IndexOfValue(managedObjectToBeValidated.Key));
                          //delete from packed object list
                          controlListOfIndexesAndPackedObjects.Remove(managedObjectToBeValidated.Key);
                          //delete from amount of sensors picking it up
                          amountOfSensorsPickingUpEachObject.Remove(managedObjectToBeValidated.Key);
                          //add to missing index values
                          missingIndexesControlList.Add(managedObjectToBeValidated.Key);
                          //mark key for removal from sensor managed index, key are being marked only cos this is inside a loop of the same list!
                          sensorManagedIndexedObjectsMarkedForRemoval.Add(managedObjectToBeValidated.Key);
                      }
                      else
                      {
                          //this object is also being kept by another sensor, delete this sensor imprint.
                          amountOfSensorsPickingUpEachObject.Values[managedObjectToBeValidated.Key]--;
                          //mark key for removal from sensor managed index, key are being marked only cos this is inside a loop of the same list!
                          sensorManagedIndexedObjectsMarkedForRemoval.Add(managedObjectToBeValidated.Key);
                      }
                    }
                }
                objectsCopy.Clear();
                foreach (int deleteMe in sensorManagedIndexedObjectsMarkedForRemoval)
                {
                    sensorManagedIndexedObjects.Remove(deleteMe);
                }
                sensorManagedIndexedObjectsMarkedForRemoval.Clear();
                //end of object cleanup
            }
        }

        private void Scan() {
            objects.Clear();
            count = Physics.OverlapSphereNonAlloc(transform.position, viewDistance, collliders, layers, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++) {
                GameObject obj = collliders[i].gameObject;
                float objDistance = IsInSight(obj);
                if (objDistance >= 0) {
                    objects.Add(objDistance,obj);
                }
            }
        }
        private float IsInSight(GameObject obj) {
            Vector3 origin = transform.position;
            Vector3 dest = obj.transform.position;
            Vector3 direction = dest - origin;
            if (direction.y < 0 || direction.y > height) {
                return -1;
            }

            direction.y = 0;
            float deltaAngle = Vector3.Angle(direction, transform.forward);
            if (deltaAngle > viewAngle) {
                return -1;
            }

            origin.y += height / 2;
            dest.y = origin.y;
            // TODO : x-ray vision bad.
            if (Physics.Linecast(origin, dest, occlusionLayers)) {
                return -1;
            }
            
            return Vector3.Distance(origin, dest);
        }

        /// <summary>
        /// This function will returns a mesh that is supposed to be the AI View Area set using the private attributes above. It's good to note that this function uses meshes and will generate geometry inside unity.
        /// </summary>
        /// <returns></returns>
        Mesh CreateWedgeMesh() {
            // this function will

            Mesh wedgeMesh = new Mesh();

            int segments = 10;
            int numTriangles = (segments * 4) + 2 + 2;
            int numVertices = numTriangles * 3;

            Vector3[] vertices = new Vector3[numVertices];
            int[] triangles = new int[numVertices];

            Vector3 bottomCenter = Vector3.zero;
            Vector3 bottomLeft = Quaternion.Euler(0, -viewAngle, 0) * Vector3.forward * viewDistance;
            Vector3 bottomRight = Quaternion.Euler(0, viewAngle, 0) * Vector3.forward * viewDistance;

            Vector3 topCenter = bottomCenter + Vector3.up * height;
            Vector3 topRight = bottomRight + Vector3.up * height;
            Vector3 topLeft = bottomLeft + Vector3.up * height;


            int vert = 0;

            //left side
            vertices[vert++] = bottomCenter;
            vertices[vert++] = bottomLeft;
            vertices[vert++] = topLeft;

            vertices[vert++] = topLeft;
            vertices[vert++] = topCenter;
            vertices[vert++] = bottomCenter;

            // right side
            vertices[vert++] = bottomCenter;
            vertices[vert++] = topCenter;
            vertices[vert++] = topRight;

            vertices[vert++] = topRight;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomCenter;

            float currentAngle = -viewAngle;
            float deltaAngle = (viewAngle * 2) / segments;
            for (int i = 0; i < segments; i++) {
                bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * viewDistance;
                bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * viewDistance;

                topRight = bottomRight + Vector3.up * height;
                topLeft = bottomLeft + Vector3.up * height;

                // far side
                vertices[vert++] = bottomLeft;
                vertices[vert++] = bottomRight;
                vertices[vert++] = topRight;

                vertices[vert++] = topRight;
                vertices[vert++] = topLeft;
                vertices[vert++] = bottomLeft;

                //top
                vertices[vert++] = topCenter;
                vertices[vert++] = topLeft;
                vertices[vert++] = topRight;

                //bottom
                vertices[vert++] = bottomCenter;
                vertices[vert++] = bottomRight;
                vertices[vert++] = bottomLeft;

                currentAngle += deltaAngle;
            }

            for (int i = 0; i < numVertices; i++) {
                triangles[i] = i;
            }

            wedgeMesh.vertices = vertices;
            wedgeMesh.triangles = triangles;
            wedgeMesh.RecalculateNormals();

            return wedgeMesh;
        }
        
        private void OnValidate() {
            mesh = CreateWedgeMesh();
            scanInterval = 1.0f / scanFrequency;
        }

        private void OnDrawGizmos() {
            if (mesh) {
                Gizmos.color = meshColor;
                Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
            }
            
            Gizmos.DrawWireSphere(transform.position, viewDistance);
            for (int i = 0; i < count; i++) {
                Gizmos.DrawSphere(collliders[i].transform.position, 0.5f);
            }

            Gizmos.color = new Color(fovGreen,fovRed,fovBlue,fovAlpha);
            foreach (var obj in objects) {
                Gizmos.DrawSphere(obj.Value.transform.position, 0.9f);
            }
        }
        
    }
}