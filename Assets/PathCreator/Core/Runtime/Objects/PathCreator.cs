using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace PathCreation {
    public class PathCreator : MonoBehaviour {

        /// This class stores data for the path editor, and provides accessors to get the current vertex and bezier path.
        /// Attach to a GameObject to create a new path editor.

        public event System.Action pathUpdated;

        [SerializeField, HideInInspector]
        PathCreatorData editorData;
        [SerializeField, HideInInspector]
        bool initialized;
        Vector3 prevPosition;
        private bool visible;

        GlobalDisplaySettings globalEditorDisplaySettings;

        // Vertex path created from the current bezier path
        public VertexPath path {
            get {
                if (!initialized) {
                    InitializeEditorData (false);
                }
                return editorData.GetVertexPath(transform);
            }
        }

        // The bezier path created in the editor
        public BezierPath bezierPath {
            get {
                if (!initialized) {
                    InitializeEditorData (false);
                }
                return editorData.bezierPath;
            }
            set {
                if (!initialized) {
                    InitializeEditorData (false);
                }
                editorData.bezierPath = value;
            }
        }

        public void DrawPath()
        {
            if(path != null) 
            {
                gameObject.TryGetComponent<LineRenderer>(out LineRenderer lineRend);
                if(lineRend == null) lineRend = gameObject.AddComponent<LineRenderer>() as LineRenderer;
                else 
                {
                    //Debug.Log("Already have linerend, resetting...");
                    lineRend.positionCount = 0;
                    lineRend.enabled = true;
                }

                //https://stackoverflow.com/questions/72240485/how-to-add-the-default-line-material-back-to-the-linerenderer-material
                lineRend.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

                lineRend.startColor = Color.green;
                lineRend.endColor = Color.green;
                lineRend.startWidth = 0.2f;
                lineRend.endWidth = 0.2f;
                lineRend.loop = false;
                lineRend.positionCount = path.NumPoints;

                for (int i = 0; i < path.NumPoints; i++)
                {
                    lineRend.SetPosition(i, path.GetPoint(i));
                }
            }
        }

        public void DrawPathEdit()
        {
            if(path != null) 
            {
                //draw through line
                gameObject.TryGetComponent<LineRenderer>(out LineRenderer lineRend);
                if(lineRend == null) lineRend = gameObject.AddComponent<LineRenderer>() as LineRenderer;
                else 
                {
                    //Debug.Log("Already have linerend, resetting...");
                    lineRend.positionCount = 0;
                    lineRend.enabled = true;
                }

                //https://stackoverflow.com/questions/72240485/how-to-add-the-default-line-material-back-to-the-linerenderer-material
                lineRend.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

                lineRend.startColor = Color.magenta;
                lineRend.endColor = Color.magenta;
                lineRend.startWidth = 0.5f;
                lineRend.endWidth = 0.5f;
                lineRend.loop = false;
                lineRend.positionCount = path.NumPoints;

                for (int i = 0; i < path.NumPoints; i++)
                {
                    lineRend.SetPosition(i, path.GetPoint(i));
                }

                //draw ends
                GameObject startPoint = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                startPoint.transform.position = path.GetPoint(0);
                startPoint.tag = "PathEdit";

                GameObject endPoint = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                endPoint.transform.position = path.GetPoint(path.NumPoints - 1);
                endPoint.tag = "PathEdit";

            }
        }

        public void ClearPath()
        {
            if(path != null) 
            {
                gameObject.TryGetComponent<LineRenderer>(out LineRenderer lineRend);
                if(lineRend == null) lineRend = gameObject.AddComponent<LineRenderer>() as LineRenderer;
                else 
                {
                    lineRend.positionCount = 0;
                }
                lineRend.enabled = false;
            }
        }

        bool getPathVisibility()
        {
            string streamingPath = Application.streamingAssetsPath;
            string[] path = streamingPath.Split('/');
            string rootPath = "";
            for(int i = 0; i < path.Length - 2; i++) rootPath += "/" + path[i];
            rootPath = rootPath.Remove(0, 1);

            string[] lines = File.ReadAllLines(rootPath + "/settings.txt");

            //Debug.Log("Visible in pathcreator?: " + bool.Parse(lines[0].Split(':')[1]));
            return bool.Parse(lines[0].Split(':')[1]);
        }

        void Start()
        {
            prevPosition = new Vector3(0, 0, 0);
            visible = getPathVisibility();
        }

        void Update()
        {
            /*if(prevPosition != transform.position && gameObject.tag == "Runtime")
            {
                DrawPath();
                prevPosition = transform.position;
            }*/

            if(!getPathVisibility())
            {
                ClearPath();
            }
            else
            {
                if(gameObject.tag == "Runtime" || gameObject.tag == "ActivePath") DrawPath();
            }
            /*else
            {
                Debug.Log("path is visible...");
                if(visible != getPathVisibility()) 
                {
                    DrawPath();
                    visible = getPathVisibility();
                }
            }*/
        }

        #region Internal methods

        /// Used by the path editor to initialise some data
        public void InitializeEditorData (bool in2DMode) {
            if (editorData == null) {
                editorData = new PathCreatorData ();
            }
            editorData.bezierOrVertexPathModified -= TriggerPathUpdate;
            editorData.bezierOrVertexPathModified += TriggerPathUpdate;

            editorData.Initialize (in2DMode);
            initialized = true;
        }

        public PathCreatorData EditorData {
            get {
                return editorData;
            }

        }

        public void TriggerPathUpdate () {
            if (pathUpdated != null) {
                pathUpdated ();
            }
        }

#if UNITY_EDITOR
        // Draw the path when path objected is not selected (if enabled in settings)
        void OnDrawGizmos () {

            // Only draw path gizmo if the path object is not selected
            // (editor script is resposible for drawing when selected)
            GameObject selectedObj = UnityEditor.Selection.activeGameObject;
            if (selectedObj != gameObject) {

                if (path != null) {
                    path.UpdateTransform (transform);

                    if (globalEditorDisplaySettings == null) {
                        globalEditorDisplaySettings = GlobalDisplaySettings.Load ();
                    }

                    if (globalEditorDisplaySettings.visibleWhenNotSelected) {

                        Gizmos.color = globalEditorDisplaySettings.bezierPath;

                        for (int i = 0; i < path.NumPoints; i++) {
                            int nextI = i + 1;
                            if (nextI >= path.NumPoints) {
                                if (path.isClosedLoop) {
                                    nextI %= path.NumPoints;
                                } else {
                                    break;
                                }
                            }
                            Gizmos.DrawLine (path.GetPoint (i), path.GetPoint (nextI));
                        }
                    }
                }
            }
        }
#endif

        #endregion
    }
}