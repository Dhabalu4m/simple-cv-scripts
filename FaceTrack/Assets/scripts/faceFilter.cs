///Dhawal.
/* Using Affdex Plugin for face features tracking. Below are the points that can be tracked.
 *  0	Right Top Jaw	                        17	Inner Right Eye
    1	Right Jaw Angle	                        18	Inner Left Eye
    2	Tip of Chin	                            19	Outer Left Eye
    3	Left Jaw Angle	                        20	Right Lip Corner
    4	Left Top Jaw	                        21	Right Apex Upper Lip
    5	Outer Right Brow Corner             	22	Upper Lip Center
    6	Right Brow Center                   	23	Left Apex Upper Lip
    7	Inner Right Brow Corner             	24	Left Lip Corner
    8	Inner Left Brow Corner	                25	Left Edge Lower Lip
    9	Left Brow Center	                    26	Lower Lip Center
    10	Outer Left Brow Corner	                27	Right Edge Lower Lip
    11	Nose Root	                            28	Bottom Upper Lip
    12	Nose Tip	                            29	Top Lower Lip
    13	Nose Lower Right Boundary	            30	Upper Corner Right Eye
    14	Nose Bottom Boundary	                31	Lower Corner Right Eye
    15	Nose Lower Left Boundary	            32	Upper Corner Left Eye
    16	Outer Right Eye	                        33	Lower Corner Left Eye
 * */

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Affdex;
using System.Collections;
using UnityEngine.SceneManagement;

public sealed class faceFilter : ImageResultsListener
{
    //Texture related variables
    public Affdex.CameraInput cameraInput;
    private WebCamTexture camTextureUsed;
    private Canvas canvasUsed;
    private bool camTextureCreated = false;
    private float screenScaleX, screenScaleY;

    //Affdex featuresPoints. These are the co-ordinates to the various face tracked features.
    private static FeaturePoint[] featurePointsList;

    //Bool variables to take decisions.
    private static bool faceFound;
    private bool pointsCreated = false;

    //Gameobjects used. 
    private GameObject[] points;                                    //Points are the spheres to display the face tracked points. 
    public GameObject sharingan;                                    //The sharingan filter to display. Should be assigned in the editor. 
    private GameObject _sharingan = null;                           //For internal use.

    //Callbacks called by ImageResultsListener.
    //Called when fae is recognized.
    public override void onFaceFound(float timestamp, int faceId)
    {  
        faceFound = true;
        Debug.Log("Found the face");
    }

    //Called when a recognized face is lost.
    public override void onFaceLost(float timestamp, int faceId)
    {
        faceFound = false;
        Debug.Log("Lost the face");
    }

    //Called to return the face class values.
    public override void onImageResults(Dictionary<int, Face> faces)
    {
        //Create the webcam texture once when Affdex is ready to return the frame.
        if (camTextureCreated == false)
        {
            getTextureDetails();
        }

        //For each face detected, get the details.
        foreach (KeyValuePair<int, Face> pair in faces)
        {
            int FaceId = pair.Key;                          // The Face Unique Id.
            Face face = pair.Value;                         // Instance of the face class containing emotions, and facial expression values.
            featurePointsList = face.FeaturePoints;         //Co-ordinates of the face points tracked.
            scaleFeaturePoints();                           //Scale the feature points according to the screen size.
        }
    }

    public void Awake()
    {
        canvasUsed = GameObject.FindObjectOfType<Canvas>(); //Get the canvas for setting scaling values. Dependent on scren size.
    }

    /*Create the spheres to display the feature points.
     * Then track them.
     * According to user input either Create the sharingan filter OR destroy everything and Exit. */
    void Update()
    {
        if (faceFound)
        {
            if (pointsCreated == false)
            {
                createPoints();
            }
            else if (pointsCreated == true)
            {
                trackPoints();
                #region To show sharingan
#if UNITY_EDITOR || UNITY_STANDALONE
                                if(Input.GetButtonDown("Vertical"))             //Vertical -> UP/DOWN Arrows OR W/S Keys. Check Project Settings->Input. 
                {
                                    if (_sharingan == null)
                                        createSharingan();
                                    else
                                    {
                                        Destroy(_sharingan);
                                        _sharingan = null;
                                    }
                                }
#endif
#if !UNITY_EDITOR && UNITY_ANDROID
                                if (Input.touchCount > 0)                       //Android equivalent to check for > 1 taps/touches.
                                {
                                    if(Input.GetTouch(0).tapCount > 1)
                                    {
                                        if (_sharingan == null)
                                            createSharingan();
                                        else
                                        {
                                            Destroy(_sharingan);
                                            _sharingan = null;
                                        }
                                    }
                                }
#endif
                #endregion

                #region To destroy and exit
#if !UNITY_EDITOR && UNITY_ANDROID
            if (Input.touchCount > 0)                                           //Android equivalent to check for single tap/touch.
            {
                if(Input.GetTouch(0).tapCount < 2)
                {
                  destroyPoints();
                }
            }
#elif UNITY_EDITOR || UNITY_STANDALONE
                if (Input.GetButtonDown("Jump"))                                //Jump -> Spacebar. Check Project Settings->Input. 
                {
                    destroyPoints();
                }
#endif
#endregion
            }
        }
    }

    //Create the sphere primitive for each feature point. Transform and scale it accordingly. Color it blue. 
    private void createPoints()
    {
        int i;
        points = new GameObject[featurePointsList.Length];
        for (i = 0; i < featurePointsList.Length; i++)
        {
            points[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            points[i].transform.position = new Vector3(featurePointsList[i].x, featurePointsList[i].y, 100);
            points[i].transform.localScale = new Vector3(1f, 1f, 1f);
            points[i].GetComponent<Renderer>().material.color = Color.blue;
        }
        pointsCreated = true;
    }

    //nstantiate the sharigan prefab as assigned in the editor. Transform and scale it according to right eye position and size. Adjust by trial & error.
    private void createSharingan()
    {
        _sharingan = Instantiate(sharingan);

        float verticalEyeSize = Mathf.Abs(featurePointsList[30].y - featurePointsList[31].y);   //30->Right eye upper corner. 31-> Right eye lower corner.

        _sharingan.transform.localPosition = new Vector3(featurePointsList[30].x, featurePointsList[30].y - verticalEyeSize / 2f, 90);
        _sharingan.transform.localScale = new Vector3(verticalEyeSize/25f, verticalEyeSize/25f, 1);
    }

    //Change the sphere positions as returned by feature points. Also track the sharingan if created.
    private void trackPoints()
    {
        int i;
        for (i = 0; i < featurePointsList.Length; i++)
        {
            points[i].transform.position = new Vector3(featurePointsList[i].x, featurePointsList[i].y, 100);
        }
        if (_sharingan != null)
        {
            float verticalEyeSize = Mathf.Abs(featurePointsList[30].y - featurePointsList[31].y);

            _sharingan.transform.localPosition = new Vector3(featurePointsList[30].x, featurePointsList[30].y - verticalEyeSize / 2f, 90);
            _sharingan.transform.localScale = new Vector3(verticalEyeSize / 25f, verticalEyeSize / 25f, 1);
        }
    }

    //Destroy spheres and sharingan if created and then Quit. 
    private void destroyPoints()
    {
        int i;
        if (pointsCreated)
        {
            for (i = 0; i < points.Length; i++)
                Destroy(points[i]);
        }
        if (_sharingan != null)
            Destroy(_sharingan);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;                    //If running in editor, then exit play mode. 
#elif !UNITY_EDITOR
        Application.Quit();
#endif
    }

    private void scaleFeaturePoints()
    {
        int i;
        Vector2 temp;
        for (i = 0; i < featurePointsList.Length; i++)
        {
            temp = scale(new Vector2(featurePointsList[i].x, featurePointsList[i].y));
            featurePointsList[i].x = temp.x;
            featurePointsList[i].y = temp.y;
        }
    }

    //Map the co-ordinates of the feature points from CameraInput texture scale to screen(canvas) space.
    private Vector2 scale(Vector2 OldValue)
    {
        float OldRangeX = (camTextureUsed.width);
        float NewRangeX = screenScaleX;
        float NewValueX = ((NewRangeX / OldRangeX) * OldValue.x) - (NewRangeX / 2);

        float OldRangeY = (camTextureUsed.height);
        float NewRangeY = screenScaleY;
        float NewValueY = ((NewRangeY / OldRangeY) * OldValue.y) - (NewRangeY / 2);

        return (new Vector2(-NewValueX, -NewValueY));
    }

    private void getTextureDetails()
    {
        camTextureUsed = cameraInput.camTexture;

        screenScaleX = (GameObject.FindObjectOfType<RawImage>().rectTransform.rect.width) * (float)canvasUsed.transform.localScale.x;
        screenScaleY = (Screen.height) * (float)canvasUsed.transform.localScale.y;

        camTextureCreated = true;
    }

}//faceFilter. Dhawal.
    