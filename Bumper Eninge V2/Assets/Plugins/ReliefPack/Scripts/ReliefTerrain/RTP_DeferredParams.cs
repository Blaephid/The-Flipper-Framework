using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("Relief Terrain/Helpers/Deferred Params")]
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
public class RTP_DeferredParams : MonoBehaviour {
    private Camera mycam;
	private CommandBuffer combufPreLight;
	private CommandBuffer combufPostLight;

    public Material CopyPropsMat; // to make sure we have shader exported for build

    private HashSet<Camera> sceneCamsWithBuffer = new HashSet<Camera>();

    public void OnEnable()
    {
        if (NotifyDecals()) return; // decals installed and used - don't handle command buffers

        if (mycam == null)
        {
            mycam = GetComponent<Camera>();
            if (mycam == null) return;
        }
        Initialize();
        Camera.onPreRender += SetupCam;
    }

    public void OnDisable()
    {
        NotifyDecals();
        Cleanup();
    }
    public void OnDestroy()
    {
        NotifyDecals();
        Cleanup();
    }

    private bool NotifyDecals()
    {
        System.Type decalsType = System.Type.GetType("UBERDecalSystem.DecalManager");
        if (decalsType != null)
        {
            bool DecalsEnabled = false;
            DecalsEnabled = GameObject.FindObjectOfType(decalsType) != null && (GameObject.FindObjectOfType(decalsType) is MonoBehaviour) && (GameObject.FindObjectOfType(decalsType) as MonoBehaviour).enabled;
            if (DecalsEnabled)
            {
                (GameObject.FindObjectOfType(decalsType) as MonoBehaviour).Invoke("OnDisable", 0);
                (GameObject.FindObjectOfType(decalsType) as MonoBehaviour).Invoke("OnEnable", 0);
                return true;
            }
        }
        return false;
    }

    private void Cleanup()
    {
        if (combufPreLight != null)
        {
            if (mycam)
            {
                mycam.RemoveCommandBuffer(CameraEvent.BeforeReflections, combufPreLight);
                mycam.RemoveCommandBuffer(CameraEvent.AfterLighting, combufPostLight);
            }
            foreach (Camera cam in sceneCamsWithBuffer)
            {
                if (cam)
                {
                    cam.RemoveCommandBuffer(CameraEvent.BeforeReflections, combufPreLight);
                    cam.RemoveCommandBuffer(CameraEvent.AfterLighting, combufPostLight);
                }
            }
        }
        sceneCamsWithBuffer.Clear();
        Camera.onPreRender -= SetupCam;
    }

    private void SetupCam(Camera cam)
    {
        bool isSceneCam = false;
#if UNITY_EDITOR
        if (Camera.main && Camera.main == mycam)
        {
            // scene cameras handled by component attached to Camera.main only (to not double it)
            isSceneCam = (SceneView.lastActiveSceneView && cam == SceneView.lastActiveSceneView.camera);
        }
#endif
        if (cam==mycam || isSceneCam)
        {
            RefreshComBufs(cam, isSceneCam);
        }
    }

	public void RefreshComBufs(Camera cam, bool isSceneCam) {
		if (cam && combufPreLight!=null && combufPostLight!=null) {
            CommandBuffer[] combufsPreLight = cam.GetCommandBuffers(CameraEvent.BeforeReflections);
            bool found = false;
            foreach (CommandBuffer cbuf in combufsPreLight)
            {
                // instance comparison below DOESN'T work !!! Well, weird isn't it ???
                //if (cbuf == combufPreLight)
                if (cbuf.name == combufPreLight.name)
                {
                    // got it already in command buffers
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                cam.AddCommandBuffer(CameraEvent.BeforeReflections, combufPreLight);
                cam.AddCommandBuffer(CameraEvent.AfterLighting, combufPostLight);
                if (isSceneCam)
                {
                    sceneCamsWithBuffer.Add(cam);
                }
            }
		}
	}

	public void Initialize() {
		if (combufPreLight == null) {
			int propsBufferID = Shader.PropertyToID("_UBERPropsBuffer");

            // prepare material
            if (CopyPropsMat == null)
            {
                if (CopyPropsMat != null)
                {
                    DestroyImmediate(CopyPropsMat);
                }
                CopyPropsMat = new Material(Shader.Find("Hidden/UBER_CopyPropsTexture"));
                CopyPropsMat.hideFlags = HideFlags.DontSave;
            }

            // take a copy of emission buffer.a where UBER stores its props (translucency, self-shadowing, wetness)
            combufPreLight = new CommandBuffer();
			combufPreLight.name="UBERPropsPrelight";
            combufPreLight.GetTemporaryRT(propsBufferID, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RHalf);
            combufPreLight.Blit(BuiltinRenderTextureType.CameraTarget, propsBufferID, CopyPropsMat);
            
			// release temp buffer
			combufPostLight = new CommandBuffer();
			combufPostLight.name="UBERPropsPostlight";
            combufPostLight.ReleaseTemporaryRT (propsBufferID);
		}
    }
}
