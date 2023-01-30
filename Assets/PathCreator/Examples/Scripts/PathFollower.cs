using UnityEngine;

namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class PathFollower : MonoBehaviour
    {
        public PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 5;
        float distanceTravelled;
        Animator followerAnimator;
        Vector3 prevPosition;
        Quaternion prevRotation;
        Vector3 prevPathOffset;

        void Start()
        {
            if(pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;
            
                followerAnimator = gameObject.GetComponent<Animator>();
                //if(followerAnimator) followerAnimator.applyRootMotion = false;

                prevPathOffset = new Vector3(0, 0, 0);
            }
        }

        void Update()
        {
            if(pathCreator != null)
            {
                //get next point in the path
                distanceTravelled += speed * Time.deltaTime;
                Vector3 pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                
                /*since the current transform has the animation transformations 
                applied to it, we can get them by subtracting the total offset 
                added previously(made of both the path and animation offsets)*/
                transform.position -= prevPathOffset;

                //add the current offset of the animation to the next point in the path
                Vector3 pathOffset = transform.position + pathPosition;
                //Debug.Log(pathOffset);
                //Debug.Log(prevPathOffset);

                //subtract the current animation offset? is it redundant?

                //I don't know ~_~
                transform.position += pathOffset;

                prevPathOffset = pathOffset;

                /*
                Vector3 animationPosition = transform.position;
                Quaternion animationRotation = transform.rotation;
                
                //get animation transformations
                
                Quaternion pathRotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);

                prevPosition = transform.position - pathPosition;
                //Debug.Log(prevPosition);
                prevRotation = transform.rotation * Quaternion.Inverse(pathRotation);
                //Debug.Log(prevRotation);

                distanceTravelled += speed * Time.deltaTime;
                pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                pathRotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
                
                //transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                //transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
                
                //transform.position = pathPosition + prevPosition;
                //transform.rotation = pathRotation * prevRotation;

                Vector3 positionDisplacement = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) - pathCreator.transform.position;

                //transform.position = positionDisplacement;
                //Debug.Log("Update time = " + Time.deltaTime);
                */
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