using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    public int spawnInterval = 5;
    public GameObject[] towerComponents;
    public Rigidbody baseTowerRigidBody;
    public GameObject towerCentre;
    public CinemachineVirtualCamera virtualCamera;
    private CinemachineFramingTransposer virtualCameraFramingTransposer;
    private float calibrationOffset;
    private List<GameObject> activeTowerComponents = new List<GameObject>();
    private int activeTowerIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = true;
        calibrationOffset = Input.gyro.attitude.eulerAngles.z;
        InvokeRepeating("SpawnTowerComponent", spawnInterval, spawnInterval);
        virtualCameraFramingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        virtualCameraFramingTransposer.m_CameraDistance = 5;
        towerCentre.transform.position = new Vector3(transform.position.x, transform.position.y + activeTowerComponents.Count, transform.position.z);
        towerCentre.transform.rotation = Quaternion.Euler(5, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, Input.gyro.attitude.eulerAngles.z - calibrationOffset);
        // set dutch angle to follow gyro
        virtualCamera.m_Lens.Dutch = Input.gyro.attitude.eulerAngles.z - calibrationOffset;
    }

    public void ResetRotation()
    {
        calibrationOffset = Input.gyro.attitude.eulerAngles.z;
    }

    public void SpawnTowerComponent()
    {
        if (activeTowerIndex == 0)
        {
            Vector3 spawnLocation = transform.position + transform.up;
            GameObject newTowerComponent = Instantiate(towerComponents[Random.Range(0, towerComponents.Length)], spawnLocation, transform.rotation);
            activeTowerComponents.Add(newTowerComponent);
            newTowerComponent.GetComponent<HingeJoint>().connectedBody = baseTowerRigidBody;
        }
        else
        {
            Vector3 spawnLocation = activeTowerComponents[activeTowerIndex - 1].transform.position + activeTowerComponents[activeTowerIndex - 1].transform.up;
            GameObject newTowerComponent = Instantiate(towerComponents[Random.Range(0, towerComponents.Length)], spawnLocation, activeTowerComponents[activeTowerIndex - 1].transform.rotation);
            activeTowerComponents.Add(newTowerComponent);
            newTowerComponent.GetComponent<HingeJoint>().connectedBody = activeTowerComponents[activeTowerIndex - 1].GetComponent<Rigidbody>();
        }
        activeTowerIndex++;
        towerCentre.transform.position = new Vector3(transform.position.x, transform.position.y + activeTowerComponents.Count, transform.position.z);
        if (activeTowerComponents.Count <= 14 )
        {
            virtualCameraFramingTransposer.m_CameraDistance = 5 + 1.25f * activeTowerComponents.Count;
            towerCentre.transform.rotation = Quaternion.Euler(5 + 1.35f * activeTowerComponents.Count, 0, 0);
        }
    }
}
