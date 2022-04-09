
using System;
using System.Collections.Generic;
using System.Linq;
using AI.Traits.Shared;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Traits.Needs
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NormalHunger : MonoBehaviour, INeeds
    {
        //==========================
        //TO BALANCE STUFF EDIT HERE
        //==========================   
        
        //name of the need 
        private string traitName = "Hunger";
        //Daily intake of the need (can be anything like kcal / ml / mana)
        //the number must be in order with the objects that will be used to satiate the need. 
        private float realDailyIntake = 2500;
        //max time to timeout operation of the need in secs:
        private float takeActionMaxTimeToTimeOut = 2;
        //How it feels as needs get worst
        //Cost of stuff and also the reward for negative numbers:
        private float needsInterval = 1; // the interval the needs are rolled.
        private float dailyIntake = 2500; // amount of resource needed per day.
        private float dayLenght;

        private float needsDelta;
        //example: if it "hurts" 20 it will also be 20 rewarding!
        private int fullState = 20;
        private string fullStateDescription = "Stuffed";
        private string noNeedDescription = "not hungry";
        private int normalState = -10;
        private string normalStateDescription = "Peckish";
        private int lowState = -50;
        private string lowStateDescription = "Hungry";
        private int almostEmptyState = -80;
        private string almostEmptyStateDescription = "Starving";
        private string TypeOfConsumable = "normalFood";

        //=========================
        // End of cost
        //=========================

        // var declaration
        public float KcalLostPerTick;
        public string needState;
        public float actualNeedState;
        public float pleasantness;
        public float needToSatiate;
        private Component gameEngine;
        public float lastTimeNeedsRolled;
        private float takeActionTimeOut;
        private INeedsConsumable typeOfConsumable;
        public NavMeshAgent aiAgent;
        private GameObject targetedObj;
        private INeedsConsumable targetConsumable;
        private bool busy;
        private SortedList<float, DetectedObject> mapOfTargets = new();
        private int idOfThisStance;
        //private bool _once;
        
        
        //new communication ruleset.
        
        //IActionAble DataSet:
        private SortedList<int, float> rewardByIActionableIndex = new(); // this list must be accessed descending
        private SortedList<int, IActionAble> controlListOfIndexesAndIActionable = new();
        
        //INeeds DataSet:
        //private SortedList<float, int> _rewardByIActionableIndex = new SortedList<float, int>(); // this list must be accessed descending
        //private SortedList<int, IActionAble> _controlListOfIndexesAndIActionable = new SortedList<int, IActionAble>();
        
        //Variables to link this object with the main list (START)
        //shared stuff:
        private SortedList<int, int> amountOfSensorsPickingUpEachObject = new(); //no use right now
        private SortedList<float, int> indexesByDistance = new();
        private SortedList<int, DetectedObject> controlListOfIndexesAndPackedObjects = new();
        private List<int> missingIndexesControlList = new(); //no use right now
        //local stuff:
        private bool databaseIsOnline;
        private bool databaseIsOnline1;
        private bool databaseIsOnline2;
        private bool databaseIsOnline3;
        private bool databaseIsOnline4;
        private bool databaseIsOnline5;
        private bool databaseIsOnline6;
        //Variables to link this object with the main list (FINISH)

        //linking (start)!
        public bool DatabaseIsOnline() {
            return databaseIsOnline;
        }
        private void CheckIfDataSetIsComplete()
        {
            if (!databaseIsOnline1 || !databaseIsOnline2 || !databaseIsOnline3 || !databaseIsOnline4 || !databaseIsOnline5 || !databaseIsOnline6) return;
            //_once = true;
            databaseIsOnline = true;
            DatasetRegister();
            //Debug.Log("Hunger Online");
        }
        public void SetControlListOfIndexesAndPackedObjects(SortedList<int, DetectedObject> controlListOfIndexesAndPackedObjectsInput) {
            controlListOfIndexesAndPackedObjects = controlListOfIndexesAndPackedObjectsInput;
            databaseIsOnline1 = true;
            CheckIfDataSetIsComplete();
        }
        public void SetMissingIndexesControlList(List<int> missingIndexesControlListInput) {
            missingIndexesControlList = missingIndexesControlListInput;
            databaseIsOnline4 = true;
            CheckIfDataSetIsComplete();
        }

        public void SetRewardByIActionableIndex(SortedList<int, float> rewardByIActionableIndexInput) {
            rewardByIActionableIndex = rewardByIActionableIndexInput;
            databaseIsOnline5 = true;
            CheckIfDataSetIsComplete();
        }
        public void SetControlListOfIndexesAndIActionable(SortedList<int, IActionAble> controlListOfIndexesAndIActionableInput) {
            controlListOfIndexesAndIActionable = controlListOfIndexesAndIActionableInput;
            databaseIsOnline6 = true;
            CheckIfDataSetIsComplete();
        }

        public void SetAmountOfSensorsPickingUpEachObject(SortedList<int, int> amountOfSensorsPickingUpEachObjectInput) {
            amountOfSensorsPickingUpEachObject = amountOfSensorsPickingUpEachObjectInput;
            databaseIsOnline2 = true;
            CheckIfDataSetIsComplete();
        }
        public void SetIndexesByDistance(SortedList<float, int> indexesByDistanceInput) {
            indexesByDistance = indexesByDistanceInput;
            databaseIsOnline3 = true;
            CheckIfDataSetIsComplete();
        }
        //Done linking! (FINISH)

        private void DatasetRegister()
        {
            //this check exists only to make index match.
            if (controlListOfIndexesAndIActionable.Count == 0)
            {
                idOfThisStance = 0;
                controlListOfIndexesAndIActionable.Add(idOfThisStance,this);
                rewardByIActionableIndex.Add(idOfThisStance,0);

            }
            else
            {
                idOfThisStance = controlListOfIndexesAndIActionable.Count + 1;
                controlListOfIndexesAndIActionable.Add(idOfThisStance,this);
                rewardByIActionableIndex.Add(idOfThisStance,0);
            }
        }
        
        // end of new communication set.
        //ruleset of how it mainly works works
        private void UpdateInstantHungerPleasantness(){
            pleasantness = 0;
            needToSatiate = 0;
            if(actualNeedState < 10){
                pleasantness = fullState; //fully satiated
                needState = fullStateDescription;
            } else { if(actualNeedState > 10 && actualNeedState < 35)
                {
                    needToSatiate = 0;
                    pleasantness = 0;
                    needState = noNeedDescription; // no need (just fine)
                } else { if(actualNeedState >= 35 && actualNeedState <= 50){
                        pleasantness = normalState; // kind of in need.
                        needToSatiate = Mathf.Abs(normalState);
                        needState = normalStateDescription;
                    } else {if(actualNeedState > 50 && actualNeedState <= 75){
                            pleasantness = lowState; // in need
                            needToSatiate = Mathf.Abs(lowState);
                            needState = lowStateDescription; //very much in need
                        } else {if(actualNeedState > 75){
                                pleasantness = almostEmptyState; // extremely in need.
                                needToSatiate = Mathf.Abs(almostEmptyState);
                                needState = almostEmptyStateDescription;
                            }    
                        }
                    }
                }
            }
            rewardByIActionableIndex[idOfThisStance] = needToSatiate;
        }
        //filter for correct consumables
        private void FindConsumables()
        {
            mapOfTargets.Clear();
            foreach (KeyValuePair<float, int> objToBeChecked in indexesByDistance)
            {
                //this is fucked, please avoid checking if stuff still exists.
                if (controlListOfIndexesAndPackedObjects[objToBeChecked.Value].actualGameObject != null)
                {
                    if (controlListOfIndexesAndPackedObjects[objToBeChecked.Value].actualGameObject
                            .GetComponent<INeedsConsumable>() == null ||
                       controlListOfIndexesAndPackedObjects[objToBeChecked.Value].actualGameObject
                            .GetComponent<INeedsConsumable>().GetName() != TypeOfConsumable) continue;
                    {
                        mapOfTargets.Add(objToBeChecked.Key,controlListOfIndexesAndPackedObjects[objToBeChecked.Value]);
                    }
                }
            }
        }
        //end of var
        //awake should be empty, all this crap is for testing only.
        private void Awake()
        {
            //Init Needs Timers:
            dayLenght = GameObject.FindGameObjectWithTag("WorldEngineObject").GetComponent<IWorldEngine>()
                .GetDayLenght();
            needsDelta = dailyIntake / (dayLenght / needsInterval);
            //done!
            //Init PathFinder!
            aiAgent = gameObject.GetComponent<NavMeshAgent>();
            //done!
            KcalLostPerTick = 25;
            //down here is questionable.
            actualNeedState = 99;
            lastTimeNeedsRolled = 58;
            UpdateInstantHungerPleasantness();
            needState = "not hungry";
            busy = false;
            //end of questionable stuff.
        }
        //start of mandatory stuff
        public string GetNeedName()
        {
            return traitName;
        }

        public string GetState()
        {
            return needState;
        }

        public void Intake(float intakeExtended) //intake in kcal from eaten stuff
        {
            actualNeedState = Math.Clamp(actualNeedState + (float) - Math.Round(((intakeExtended*100)/dailyIntake)),0.0f,100.0f);
            UpdateInstantHungerPleasantness();
        }
        
        public float GetFullState()
        {
            return realDailyIntake;
        }
        
        public float GetNeedExtended() // return hunger in kcal
        {
            return (realDailyIntake/100) * actualNeedState;
            //actual state of how much it needs.
        }
        
        public float GetNeed()
        {
            return actualNeedState;
            //how much it needs it is from 0 to 100
        }

        public float GetExpectedPleasantness()
        {
            return needToSatiate;
            //expected reward
        }

        public float GetActivePleasantness()
        {
            return pleasantness;
            //actual reward
        }
        //end of mandatory stuff
        private void Update()
        {
            if (DatabaseIsOnline())
            {
                lastTimeNeedsRolled += Time.unscaledDeltaTime;
                if (lastTimeNeedsRolled >= needsInterval)
                {
                    lastTimeNeedsRolled = 0;
                    Tick();
                }
            }
        }

        //Class functions

        public void Tick()
        {
            Intake(-KcalLostPerTick);
        }
        

        public int TakeAction()
        {
            //check if database link is fucked
            if (!databaseIsOnline)
            {
                return 3;
            }
            takeActionTimeOut += Time.unscaledDeltaTime;
            if (!indexesByDistance.Any())
            {
                //no targets in range
                return 0;
            }
            if (takeActionTimeOut >= takeActionMaxTimeToTimeOut)
            {
                takeActionTimeOut = 0;
                busy = false;
            }
            //searching for food state
            //filter targets()
            if (!busy)
            {
                FindConsumables();
                if (!mapOfTargets.Any())
                {
                    //no food found state
                    return 0;
                }
                //food found state
                //findClosest();
                // fist entry is the target
                targetedObj = mapOfTargets.Values[0].actualGameObject;
                targetConsumable = targetedObj.GetComponent<INeedsConsumable>();
                
                //move()
                aiAgent.SetDestination(mapOfTargets.First().Value.actualGameObject.transform.position);
                busy = true;
            }
            if (Vector3.Distance(transform.position, targetedObj.transform.position) <
                targetConsumable.GetMinimalUseDistance())
            {
                //ConsumeIfInRange()
                //eating food state
                targetConsumable.ConsumeItem(this);
                busy = false;
                takeActionTimeOut = 0;
                mapOfTargets.Clear();
                UpdateInstantHungerPleasantness();
                return 2;
            }
            return 1;
        }

        public string GetActionName()
        {
            return traitName;
        }
    }
}