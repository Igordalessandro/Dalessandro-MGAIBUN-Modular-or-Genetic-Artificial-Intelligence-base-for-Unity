using AI.Traits.Shared;
using UnityEngine;

namespace AI.Core
{
    public class WorldEngine : MonoBehaviour, IWorldEngine {
        [Range(0.0f, 86400.0f)]
        public float dayLenght = 1800; // in secs
        // Start is called before the first frame update
        [Range(0.0f, 6000.0f)]
        public float needsInterval = 60; //in secs
        public float lastTimeNeedsRolled;
        public float lastTimeDayChanged;
        public float kcalIntakeDaily = 2500; //expanded needs
        public float needsDelta;

        private void Awake()
        {
            needsDelta = (kcalIntakeDaily / (dayLenght / needsInterval));
        }

        void Start() {
            lastTimeNeedsRolled = Time.unscaledTime;
            lastTimeDayChanged = Time.unscaledTime;
        }
        
        // Update is called once per frame
        void Update() {
            //update needs
            lastTimeNeedsRolled += Time.unscaledDeltaTime;
            if (lastTimeNeedsRolled >= needsInterval) {
                lastTimeNeedsRolled = 0;
            }

            //update day
            if (Time.unscaledTime - lastTimeDayChanged > dayLenght) {
                lastTimeDayChanged = Time.unscaledTime;
            }
        }

        public float GetDayLenght()
        {
            return dayLenght;
        }
    }
}
