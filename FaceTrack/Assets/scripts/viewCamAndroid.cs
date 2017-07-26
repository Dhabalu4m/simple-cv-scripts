///Dhawal. To get the camera frame from Affdex and adjust to fit screen size and look more natural. 
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using Affdex;


public class viewCamAndroid : MonoBehaviour
{
    public Affdex.CameraInput cameraInput;

    public RawImage image;
    public RectTransform imageParent;

    //Image fitter attached to Leia makes sure the image always follows a specific ratio(as determied by screen size) adjusting the width according
    //to the screen height. Configured in the editor. 
    public AspectRatioFitter imageFitter;

    WebCamTexture activeCameraTexture = null;

    // Image rotation
    Vector3 rotationVector = new Vector3(0f, 0f, 0f);

    // Image uvRect
    Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
    Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

    // Image Parent's scale
    Vector3 defaultScale = new Vector3(1f, 1f, 1f);
    Vector3 fixedScale = new Vector3(-1f, 1f, 1f);

    private bool setTexture = false;

    void Start()
    {
        //Make sure Affdex is properly loaded.
        if (!AffdexUnityUtils.ValidPlatform())
            return;
    }

    void Update()
    {
        if (activeCameraTexture != null)
        {
            if (setTexture == false)
            {
                setTextures();
            }
            else
            {
                // Rotate image to show correct orientation 
                rotationVector.z = -activeCameraTexture.videoRotationAngle;
                image.rectTransform.localEulerAngles = rotationVector;

                // Set AspectRatioFitter's ratio
                float videoRatio = (float)activeCameraTexture.width / (float)activeCameraTexture.height;
                imageFitter.aspectRatio = videoRatio;

                // Unflip if vertically flipped
                image.uvRect =
                    activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;

                // Mirror front-facing camera's image horizontally to look more natural
                imageParent.localScale = fixedScale;
            }
        }
        else
        {
            activeCameraTexture = cameraInput.camTexture;
        }
    }

    private void setTextures()
    {
        activeCameraTexture.filterMode = FilterMode.Trilinear;

        image.texture = activeCameraTexture;
        image.material.mainTexture = activeCameraTexture;
        setTexture = true;
    }
}//viewCamAndroid. Dhawal.