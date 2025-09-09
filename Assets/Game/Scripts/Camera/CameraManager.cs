using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private InputManager input;

    [SerializeField] public CameraState cameraState;
    [SerializeField] private CinemachineFreeLook tppCamera;
    [SerializeField] private CinemachineVirtualCamera fppCamera;

    private void Start()
    {
        input.OnChangePOV += SwitchCamera;
    }

    private void OnDestroy()
    {
        input.OnChangePOV -= SwitchCamera;
    }

    public void SetTPPFieldOfView(float fieldOfView)
    {
        tppCamera.m_Lens.FieldOfView = fieldOfView;
    }

    public void SetFPPClampedCamera(bool isClamped, Vector3 playerRotation)
    {
        CinemachinePOV pov = fppCamera.GetCinemachineComponent<CinemachinePOV>();
        if (isClamped)
        {
            pov.m_HorizontalAxis.m_Wrap = false;
            pov.m_HorizontalAxis.m_MinValue = playerRotation.y - 45;
            pov.m_HorizontalAxis.m_MaxValue = playerRotation.y + 45;
        }
        else
        {
            pov.m_HorizontalAxis.m_MinValue = -180;
            pov.m_HorizontalAxis.m_MaxValue = 180;
            pov.m_HorizontalAxis.m_Wrap = true;
        }
    }

    private void SwitchCamera()
    {
        if (cameraState == CameraState.ThirdPerson)
        {
            cameraState = CameraState.FirstPerson;
            fppCamera.gameObject.SetActive(true);
            tppCamera.gameObject.SetActive(false);
        }
        else
        {
            cameraState = CameraState.ThirdPerson;
            tppCamera.gameObject.SetActive(true);
            fppCamera.gameObject.SetActive(false);
        }
    }
}
