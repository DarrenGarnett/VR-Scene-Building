using UnityEngine;

namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class PathFollower : MonoBehaviour
    {
        public PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 100;
        float distanceTravelled;
        Animator followerAnimator;
        Vector3 prevPosition;
        Quaternion prevRotation;
        Vector3 prevPathOffset;

        public float cycleDuration = 1;

        void Start()
        {
            if(pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;
            
                followerAnimator = gameObject.GetComponent<Animator>();
                //if(followerAnimator) followerAnimator.applyRootMotion = false;

                prevPathOffset = new Vector3(0, 0, 0);

                transform.position = prevPathOffset;

                //Debug.Log(pathCreator.path.length);
                //Debug.Log(cycleDuration);
            }
        }

        void Update()
        {
            if(pathCreator != null)
            {
                //get next point in the path
                //distanceTravelled += speed * Time.deltaTime;
                distanceTravelled += pathCreator.path.length * (Time.deltaTime / cycleDuration);
                Vector3 pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                
                /*since the current transform has the animation transformations 
                applied to it, we can get them by subtracting the total offset 
                added previously(made of both the path and animation offsets)*/
                transform.position -= prevPathOffset;

                Vector3 pathOffset = transform.position + pathPosition;

                transform.position += pathOffset;

                prevPathOffset = pathOffset;
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