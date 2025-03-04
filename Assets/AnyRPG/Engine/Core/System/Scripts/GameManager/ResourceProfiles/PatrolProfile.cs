using AnyRPG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Patrol Profile", menuName = "AnyRPG/PatrolProfile")]
    public class PatrolProfile : DescribableResource {

        [SerializeField]
        private PatrolProps patrolProperties = new PatrolProps();

        // the current count of destinations reached
        private int destinationRetrievedCount = 0;

        // the current count of destinations reached
        private int destinationReachedCount = 0;

        // track the current position in the list of destinations to travel to on the patrol path
        private int destinationIndex = 0;

        // keep track of the current destination
        private Vector3 currentDestination = Vector3.zero;

        private UnitController unitController;

        public UnitController CurrentUnitController { get => unitController; set => unitController = value; }
        public int DestinationCount {
            get {
                if (patrolProperties.UseTags == true) {
                    return patrolProperties.DestinationTagList.Count;
                }
                return patrolProperties.DestinationList.Count;
            }
        }

        public PatrolProps PatrolProperties { get => patrolProperties; set => patrolProperties = value; }

        public Vector3 GetDestination(bool destinationReached) {
            //Debug.Log("PatrolProfile.GetDestination(" + destinationReached + ")");
            Vector3 returnValue = Vector3.zero;

            if (destinationReached || destinationRetrievedCount == 0) {
                // choose next correct destination from list
                if (patrolProperties.RandomDestinations) {
                    returnValue = GetRandomDestination();
                } else {
                    returnValue = GetLinearDestination();
                }
            } else {
                // return current destination since it has not yet been reached and this is not the first retrieval
                returnValue = currentDestination;
            }

            if (destinationRetrievedCount == 0) {
                // not allowed to reach destination on first retrieve
                destinationRetrievedCount++;
            } else {
                if (destinationReached) {
                    // if destination was not reached, we do not increment the retrieval because it is the current destination
                    destinationRetrievedCount++;
                    destinationReachedCount++;
                }
            }

            // check if patrol is complete
            if (PatrolComplete()) {
                returnValue = Vector3.zero;
            }

            currentDestination = returnValue;
            return returnValue;
        }

        public bool PatrolComplete() {
            //Debug.Log("PatrolProfile.PatrolComplete(): loopDestination: " + patrolProperties.LoopDestinations + "; destinationReachedCount: " + destinationReachedCount + "; maxDestinations: " + patrolProperties.MaxDestinations + "; destinationCount: " + DestinationCount);

            if (patrolProperties.RandomDestinations && (patrolProperties.MaxDestinations == 0 || destinationReachedCount < patrolProperties.MaxDestinations)) {
                //Debug.Log("AIPatrol.PatrolComplete() randomDestinations && (maxDestinations == 0 || destinationReachedCount < maxDestinations); return false");
                return false;
            }

            if (!patrolProperties.LoopDestinations && destinationReachedCount >= DestinationCount) {
                return true;
            }

            // apply destination amount cap
            if (patrolProperties.MaxDestinations > 0 && destinationReachedCount >= patrolProperties.MaxDestinations) {
                return true;
            }
            //Debug.Log("AIPatrol.PatrolComplete(): returning false");
            return false;
        }

        /// <summary>
        /// get a random destination from the list, or a random destination near the spawn point if no list exists
        /// </summary>
        /// <returns></returns>
        public Vector3 GetRandomDestination() {
            //Debug.Log(MyName + ".AIPatrol.GetRandomDestination()");
            if (DestinationCount > 0) {
                // get destination from list
                int randomNumber = Random.Range(0, DestinationCount);
                return GetDestinationByIndex(randomNumber);
            } else {
                // choose nearby random destination
                float randomXNumber = Random.Range(0, patrolProperties.MaxDistanceFromSpawnPoint * 2) - patrolProperties.MaxDistanceFromSpawnPoint;
                float randomZNumber = Random.Range(0, patrolProperties.MaxDistanceFromSpawnPoint * 2) - patrolProperties.MaxDistanceFromSpawnPoint;
                if (unitController == null) {
                    //Debug.Log("AIPatrol.GetRandomDestination(): CharacterUnit is null!");
                    return Vector3.zero;
                }

                // get a random point that's on the navmesh
                Vector3 randomPoint = unitController.MyStartPosition + new Vector3(randomXNumber, 0, randomZNumber);
                randomPoint = unitController.UnitMotor.CorrectedNavmeshPosition(randomPoint);
                /*
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas)) {
                    //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): destinationPosition " + destinationPosition + " on NavMesh found closest point: " + hit.position + ")");
                    randomPoint = hit.position;
                } else {
                    //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): destinationPosition " + randomPoint + " was not on NavMesh! return start position instead");
                    return (characterUnit.MyCharacter.MyCharacterController as AIController).MyStartPosition;
                }
                */
                return randomPoint;
            }
        }

        /// <summary>
        /// return a vector3 location for a destination in the current destination list
        /// </summary>
        /// <param name="listIndex"></param>
        /// <returns></returns>
        public Vector3 GetDestinationByIndex(int listIndex) {
            //Debug.Log("PatrolProfile.GetLinearDestination(): destinationIndex: " + destinationIndex);
            Vector3 returnValue = Vector3.zero;
            if (patrolProperties.UseTags == false) {
                returnValue = patrolProperties.DestinationList[listIndex];
            } else {
                GameObject tagObject = GameObject.FindGameObjectWithTag(patrolProperties.DestinationTagList[listIndex]);
                if (tagObject != null) {
                    //Debug.Log("PatrolProfile.GetLinearDestination(): destinationIndex: " + destinationIndex + "; tag object " + destinationTagList[listIndex] + " found at " + tagObject.transform.position);
                    returnValue = tagObject.transform.position;
                } else {
                    //Debug.Log("PatrolProfile.GetLinearDestination(): destinationIndex: " + destinationIndex + "; tag object " + destinationTagList[listIndex] + " not found!");
                }
            }
            return returnValue;
        }

        /// <summary>
        /// get the next destination based on the current index
        /// </summary>
        /// <returns></returns>
        public Vector3 GetLinearDestination() {
            //Debug.Log("AIPatrol.GetLinearDestination(): destinationIndex: " + destinationIndex);
            Vector3 returnValue = GetDestinationByIndex(destinationIndex);
            destinationIndex++;
            if (destinationIndex >= DestinationCount) {
                destinationIndex = 0;
            }
            return returnValue;
        }

    }

}