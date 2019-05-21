using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class KinectOverlayer : MonoBehaviour 
{
	public RawImage backgroundImage;
	public GameObject OverlayObject;
    [HideInInspector]
    public GameObject[] Objects;
    public Camera Main;


	void Start()
	{
        Objects = new GameObject[(int)KinectWrapper.NuiSkeletonPositionIndex.Count];
        for(int i = 0; i < Objects.Length; i++)
        {
            Objects[i] = Instantiate(OverlayObject);
        }
        Main = Camera.main;
	}
	
	void Update() 
	{
		KinectManager manager = KinectManager.Instance;
		
		if(manager && manager.IsInitialized())
		{
			if(backgroundImage && (backgroundImage.texture == null))
			{
				backgroundImage.texture = manager.GetUsersClrTex();
			}
			
			if(manager.IsUserDetected())
			{
				uint userId = manager.GetPlayer1ID();
				
                for(int i = 0; i < Objects.Length; i++)
                {
                    if (manager.IsJointTracked(userId, i))
                    {
                        Objects[i].GetComponent<MeshRenderer>().enabled = true;
                        Vector3 posJoint = manager.GetRawSkeletonJointPos(userId, i);

                        if (posJoint != Vector3.zero)
                        {
                            Vector2 posDepth = manager.GetDepthMapPosForJointPos(posJoint);
                            Vector2 posColor = manager.GetColorMapPosForDepthPos(posDepth);

                            float scaleX = posColor.x / KinectWrapper.Constants.ColorImageWidth;
                            float scaleY = posColor.y / KinectWrapper.Constants.ColorImageHeight;

                            //Debug.Log(posColor.x + ", " + posColor.y + " / " + KinectWrapper.Constants.ColorImageWidth + ", " + KinectWrapper.Constants.ColorImageHeight);
                            //Objects[i].transform.position = Main.ViewportToWorldPoint(new Vector3(scaleX, 1- scaleY, 5f));
                        }
                    }
                    else
                    {
                        Objects[i].GetComponent<MeshRenderer>().enabled = false;
                    }
                }
			}
		}
	}
}
