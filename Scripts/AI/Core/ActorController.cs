using System;
using System.Collections.Generic;
using System.Linq;
using AI.Traits.Shared;
using UnityEngine;

namespace AI.Core
{
    public class ActorController : MonoBehaviour {

        
        //refactoring start:
        //=====================
        //Sensor Information:
        //=====================
        //Declarations and activations:
        private SortedList<Component, int> offWorldSensorComponent = new(); //pop out eyes!
        //datasets:
        private SortedList<int, int> amountOfSensorsPickingUpEachObject = new();
        private SortedList<float, int> indexesByDistance = new();
        private SortedList<int, DetectedObject> controlListOfIndexesAndPackedObjects = new();
        private List<int> missingIndexesControlList = new();
        //=====================
        
        //=====================
        //IActionable Information:
        //=====================
        private SortedList<int, float> rewardByIActionableIndex = new(); // this list must be accessed descending
        private SortedList<int, IActionAble> controlListOfIndexesAndIActionable = new();
        //=====================

        //refactoring end.
        private List<Component> needsComponentsList = new();
        private List<INeeds> NeedList = new();
        public bool isIa = true;
        public bool busy;
        private GameObject eyesDirector;
        private IActionAble targetIActionAble;

        //Control variables

        public float takeActionThreshold = 10;
        
        void Awake() {
            // TODO : make a constructor for this (external class)
            AddScript("AI.Traits.Sensors.EyeVision", "Sensors");
            AddScript("AI.Traits.Needs.NormalHunger", "Needs");
            busy = false;
        }
        void Update() {
            // called once per frame
            if (isIa && !busy && rewardByIActionableIndex.Count > 0)
            {
                if (rewardByIActionableIndex.Values.OrderByDescending(num => num).First() < takeActionThreshold)
                {
                    //idle! 
                }
                else
                {
                    targetIActionAble = controlListOfIndexesAndIActionable[
                        rewardByIActionableIndex.IndexOfValue(rewardByIActionableIndex.Values
                            .OrderByDescending(num => num).First())];
                    busy = true;
                }
            }
            if (busy)
            {
                int statement = targetIActionAble.TakeAction();
                if (statement == 2)
                {
                    busy = false;
                }
            }
        }
        //======================
        //decision making functions start
        //======================
        //update sensor with the database information.
        private void UpdateSensorDatabase(ISensor sensorInput) {
            sensorInput.SetIndexesByDistance(indexesByDistance);
            sensorInput.SetControlListOfIndexesAndPackedObjects(controlListOfIndexesAndPackedObjects);
            sensorInput.SetAmountOfSensorsPickingUpEachObject(amountOfSensorsPickingUpEachObject);
            sensorInput.SetMissingIndexesControlList(missingIndexesControlList);
        }

        //updates sensor list
        //!!!might be overkill!!!
        //!!!might be fucked!!!
        private void UpdateSensorsDatabase() {
            foreach (ISensor sensorComponent in gameObject.GetComponents<ISensor>()) {
                if (sensorComponent.DatabaseIsOnline() == false) {
                    sensorComponent.SetIndexesByDistance(indexesByDistance);
                    sensorComponent.SetControlListOfIndexesAndPackedObjects(controlListOfIndexesAndPackedObjects);
                    sensorComponent.SetAmountOfSensorsPickingUpEachObject(amountOfSensorsPickingUpEachObject);
                    sensorComponent.SetMissingIndexesControlList(missingIndexesControlList);
                }
            }
            foreach (KeyValuePair<Component, int> offWorldSensorComponentPair in offWorldSensorComponent) {
                if (offWorldSensorComponentPair.Value == 0) {
                    offWorldSensorComponentPair.Key.GetComponent<ISensor>().SetIndexesByDistance(indexesByDistance);
                    offWorldSensorComponentPair.Key.GetComponent<ISensor>().SetControlListOfIndexesAndPackedObjects(controlListOfIndexesAndPackedObjects);
                    offWorldSensorComponentPair.Key.GetComponent<ISensor>().SetAmountOfSensorsPickingUpEachObject(amountOfSensorsPickingUpEachObject);
                    offWorldSensorComponentPair.Key.GetComponent<ISensor>().SetMissingIndexesControlList(missingIndexesControlList);
                }
            }
        }
        //as fucked as the other one.
        private void UpdateNeedsList() {
            NeedList.Clear();
            if (needsComponentsList.Any()) {
                foreach (Component needing in needsComponentsList) {
                    NeedList.Add(needing.gameObject.GetComponent<INeeds>()); //fuckery might be here.
                }
            }
            UpdateNeedsDatabase();
        }

        private void UpdateNeedsDatabase() {
            foreach (INeeds needsComponent in NeedList) {
                if (needsComponent.DatabaseIsOnline() == false) {
                    //Sensorial information
                    needsComponent.SetIndexesByDistance(indexesByDistance);
                    needsComponent.SetControlListOfIndexesAndPackedObjects(controlListOfIndexesAndPackedObjects);
                    needsComponent.SetAmountOfSensorsPickingUpEachObject(amountOfSensorsPickingUpEachObject);
                    needsComponent.SetMissingIndexesControlList(missingIndexesControlList);
                    //IActionable information cos not all IActionable are needs but all needs are IActionable!
                    needsComponent.SetControlListOfIndexesAndIActionable(controlListOfIndexesAndIActionable);
                    needsComponent.SetRewardByIActionableIndex(rewardByIActionableIndex);
                }
            }
        }
        //used to add scripts
        private void AddScript(String scriptName, String assembly) {
            Type newScriptType = Type.GetType(scriptName + "," + assembly, false, false);
            //check if exist
            if (newScriptType == null)
            {
                Debug.Log(scriptName + " is fucked up.");
            }
            if (newScriptType != null && gameObject.GetComponent(newScriptType.Name) == null) {
                //Eyes rule exception
                //this will pop out the eyes, lol.
                if (scriptName == "AI.Traits.Sensors.EyeVision") {
                    eyesDirector = new GameObject();
                    eyesDirector.name = "eyesDirector";
                    eyesDirector.AddComponent(newScriptType);
                    eyesDirector.transform.parent = gameObject.transform;
                    eyesDirector.transform.localPosition = new Vector3(0, -0.5f, 0.6f);
                } else {
                    gameObject.AddComponent(Type.GetType(scriptName + "," + assembly, false, false));
                }
            } else {
                Debug.Log(scriptName + " Already exist and will not be added.");
            }

            // ReSharper disable once PossibleNullReferenceException
            if (newScriptType.GetInterface("ISensor", true) == typeof(ISensor)) {
                if (scriptName == "AI.Traits.Sensors.EyeVision")
                {
                    offWorldSensorComponent.Add(eyesDirector.GetComponent(newScriptType), 1);
                    UpdateSensorDatabase(eyesDirector.GetComponent<ISensor>());
                } else {
                    UpdateSensorsDatabase();
                }
            } else if (newScriptType.GetInterface("INeeds", true) == typeof(INeeds)) {
                if (!needsComponentsList.Contains(gameObject.GetComponent(newScriptType))) {
                    needsComponentsList.Add(gameObject.GetComponent(newScriptType));
                    UpdateNeedsList();
                }
            }
        }
    }
}
