using UnityEngine;
using UnityEditor;

namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class PathFollower : MonoBehaviour
    {
        public PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed;
        public float distanceTravelled;
        Animator followerAnimator;
        public float offsetPosition;
        public Vector3 prevPathOffset;
        public Quaternion offsetRotation;
        public Quaternion prevPathRotation;
        public bool pathChanged = false;

        public float cycleDuration = 1;
        private Quaternion initRotation;

        void Start()
        {
            if(pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;

                prevPathOffset = transform.position;
                //prevPathOffset = new Vector3(0, 0, 0);

                prevPathRotation = transform.rotation;
                initRotation = transform.rotation;

                transform.position += prevPathOffset;
                //transform.rotation = prevPathRotation;
                offsetRotation = transform.rotation;

                speed = 1;

                // need to define up direction for later calculation to avoid undefined behavior
                //transform.up = new Vector3(0, 1, 0);
            }
        }

        void Update()
        {
            if(pathCreator != null && !PauseScript.paused)
            {
                //get next point in the path
                //distanceTravelled += speed * Time.deltaTime;
                //distanceTravelled += pathCreator.path.length * (Time.deltaTime / cycleDuration);
                /*if(pathChanged)
                {
                    //Debug.Log("path changed.");
                    distanceTravelled = 0;
                    pathChanged = false;
                }*/
                
                if(offsetPosition != 0)
                {
                    //Debug.Log("path progress offset.");
                    distanceTravelled = offsetPosition;
                    offsetPosition = 0;
                    //if(!pathChanged) prevPathRotation = transform.rotation;
                    //else prevPathRotation = Quaternion.Euler(0, 0, 0);

                    if(pathChanged) 
                    {
                        //prevPathRotation = Quaternion.Euler(0, 0, 0);
                        //Debug.Log("path offset reset");
                    }
                    pathChanged = false;
                }

                if(transform.rotation != prevPathRotation) 
                {
                    //Debug.Log("transform changed outside of path");
                    initRotation = transform.rotation;
                }

                distanceTravelled += pathCreator.path.length * (GlobalTimeScript.deltaTime / cycleDuration);
                Vector3 pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                
                //nextPathRotation = GetNextRotation();

                transform.position = pathPosition + prevPathOffset;
                //transform.rotation = pathRotation * initRotation;
                //transform.rotation = nextPathRotation;// * transform.rotation;
                transform.rotation = GetNextRotation();

                prevPathRotation = transform.rotation;

                //Correcting angle to given offset
                //pathRotation *= offsetRotation;
                
                /*since the current transform has the animation transformations 
                applied to it, we can get them by subtracting the total offset 
                added previously(made of both the path and animation offsets)*/
                /*transform.position -= prevPathOffset;
                //Debug.Log(transform.position);
                transform.rotation *= Quaternion.Inverse(prevPathRotation);

                //Vector3 pathOffset = transform.position + pathPosition;
                Vector3 pathOffset = pathPosition;
                //Quaternion rotOffset = transform.rotation * pathRotation;
                Quaternion rotOffset = pathRotation;

                transform.position += pathOffset;
                //Debug.Log(transform.position);
                transform.rotation *= rotOffset;

                prevPathOffset = pathOffset;
                prevPathRotation = rotOffset;*/
            }
        }

        Quaternion GetNextRotation()
        {
            Quaternion nextRotation = Quaternion.identity;
            if(pathCreator.objectsFollowTerrain)
            {
                /*Vector3 terrainSize = Terrain.activeTerrain.terrainData.size;
                float terrainX = pathPosition.x / terrainSize.x;
                float terrainZ = pathPosition.z / terrainSize.z;

                Vector3 normal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(terrainX, terrainZ);
                Debug.Log((normal + transform.position) - transform.position);*/

                RaycastHit hit;
                if(Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
                {
                    // Reference: runevision, May 9 2009
                    // https://forum.unity.com/threads/quaternion-rotation-along-normal.22727/

                    // difference between the obj up and normal(change in rotation on terrain)
                    //Quaternion qterrain = Quaternion.FromToRotation(transform.up, hit.normal); 
                    //nextRotation = Quaternion.Slerp(transform.rotation, qterrain * transform.rotation, 0.01f);
                
                    // difference between world up and normal(overall change in rotation)
                    Quaternion qterrain = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    //nextRotation = Quaternion.Slerp(transform.rotation, qterrain, 0.01f);
                    Vector3 vTerrain = qterrain.eulerAngles;
                    
                    Quaternion getRot = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);// * initRotation;
                    //nextRotation = getRotation;
                    Vector3 vGetRot = getRot.eulerAngles;

                    Vector3 terrainPathedRot = vTerrain + initRotation.eulerAngles;
                    //Debug.Log(vGetRot);
                    terrainPathedRot.y += vGetRot.y;

                    //nextRotation = getRotation * qterrain;
                    //nextRotation = Quaternion.Slerp(transform.rotation, getRotation * qterrain, 0.01f);

                    //nextRotation = Quaternion.Euler(terrainPathedRot);
                    nextRotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(terrainPathedRot), 0.01f);
                    //nextRotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(terrainNormal.x, getRotation.y, terrainNormal.z), 0.25f);
                    
                    //nextRotation = Quaternion.Euler(getRotation) * Quaternion.Euler(terrainNormal);
                }
                else Debug.Log("Ray miss on path follower");
            }
            else nextRotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction) * initRotation;
        
            return nextRotation;
        }

        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
        void OnPathChanged()
        {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }
    }
}