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
        float distanceTravelled;
        Animator followerAnimator;
        public float offsetPosition;
        public Vector3 prevPathOffset;
        public Quaternion offsetRotation = Quaternion.Euler(0, 0, 0);
        public Quaternion prevPathRotation;
        public bool pathChanged = false;

        public float cycleDuration = 1;

        void Start()
        {
            if(pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;

                prevPathOffset = transform.position;
                //prevPathOffset = new Vector3(0, 0, 0);

                prevPathRotation = transform.rotation;

                transform.position += prevPathOffset;
                //transform.rotation = prevPathRotation;

                speed = 1;
                //Debug.Log(pathCreator.path.length);
                //Debug.Log(cycleDuration);
                //Debug.Log("Path creator not null and initialized");
            }
        }

        void Update()
        {
            if(pathCreator != null && !PauseScript.paused)
            {
                //get next point in the path
                //distanceTravelled += speed * Time.deltaTime;
                //distanceTravelled += pathCreator.path.length * (Time.deltaTime / cycleDuration);
                if(offsetPosition != 0)
                {
                    //Debug.Log("path progress offset.");
                    distanceTravelled += offsetPosition;
                    offsetPosition = 0;
                }

                if(pathChanged)
                {
                    distanceTravelled = 0;
                    pathChanged = false;
                }

                distanceTravelled += pathCreator.path.length * (GlobalTimeScript.deltaTime / cycleDuration);
                Vector3 pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                Quaternion pathRotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);

                //Correcting angle to given offset
                pathRotation *= offsetRotation;
                
                /*since the current transform has the animation transformations 
                applied to it, we can get them by subtracting the total offset 
                added previously(made of both the path and animation offsets)*/
                transform.position -= prevPathOffset;
                //Debug.Log(transform.position);
                transform.rotation *= Quaternion.Inverse(prevPathRotation);

                //Vector3 pathOffset = transform.position + pathPosition;
                Vector3 pathOffset = pathPosition;
                Quaternion rotOffset = transform.rotation * pathRotation;

                transform.position += pathOffset;
                //Debug.Log(transform.position);
                transform.rotation *= rotOffset;

                prevPathOffset = pathOffset;
                prevPathRotation = rotOffset;
            }
        }

        

        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
        void OnPathChanged()
        {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }
    }
}