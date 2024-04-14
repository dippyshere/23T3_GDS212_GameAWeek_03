using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using TMPro;
using System.Globalization;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 5;
    [SerializeField] private GameObject[] towerComponents;
    [SerializeField] private Rigidbody baseTowerRigidBody;
    [SerializeField] private GameObject towerCentre;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject restartButton;
    private int maxScore;
    private CinemachineFramingTransposer virtualCameraFramingTransposer;
    private float calibrationOffset;
    private List<GameObject> activeTowerComponents = new List<GameObject>();
    private int activeTowerIndex = 0;
    private bool burstActive = false;
    private bool isPlaying = false;

    public bool gyroRotate = false;

    // Start is called before the first frame update
    void Start()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetSceneByName("SimplePoly City - Low Poly").isLoaded == false)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("SimplePoly City - Low Poly", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
        Input.gyro.enabled = true;
        Quaternion gyro = Input.gyro.attitude;
        calibrationOffset = (Quaternion.Euler(90, 0, 0) * new Quaternion(-gyro.x, -gyro.y, gyro.z, gyro.w)).eulerAngles.z;
        virtualCameraFramingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        virtualCameraFramingTransposer.m_CameraDistance = 5;
        towerCentre.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        towerCentre.transform.rotation = Quaternion.Euler(5, 0, 0);
        InvokeRepeating("IdleAnimations", 2, 10);
        scoreText.text = maxScore.ToString("N0", CultureInfo.InvariantCulture);
        startButton.SetActive(true);
        restartButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (gyroRotate)
        {
            Quaternion gyro = Input.gyro.attitude;
            gyro = Quaternion.Euler(90, 0, 0) * new Quaternion(-gyro.x, -gyro.y, gyro.z, gyro.w);
            gyro = Quaternion.Euler(0, 0, gyro.eulerAngles.z - calibrationOffset);
            transform.rotation = Quaternion.Euler(0, 0, gyro.eulerAngles.z);
            // set dutch angle to follow gyro
            virtualCamera.m_Lens.Dutch = gyro.eulerAngles.z;
        }
    }

    private void FixedUpdate()
    {
        if (activeTowerComponents.Count > 0)
        {
            towerCentre.transform.position = new Vector3(transform.position.x, activeTowerComponents[activeTowerComponents.Count - 1].transform.position.y, transform.position.z);
        }
    }

    public void ResetRotation()
    {
        Quaternion gyro = Input.gyro.attitude;
        calibrationOffset = (Quaternion.Euler(90, 0, 0) * new Quaternion(-gyro.x, -gyro.y, gyro.z, gyro.w)).eulerAngles.z;
        Debug.Log(calibrationOffset);
    }

    public void BeginSpawning()
    {
        gyroRotate = true;
        startButton.SetActive(false);
        restartButton.SetActive(true);
        InvokeRepeating("SpawnTowerPieces", 0.9f, spawnInterval);
        InvokeRepeating("CheckTowerBalance", 0.3f, 0.3f);
        Invoke("SpawnTowerBurst", 10f);
        CancelInvoke("IdleAnimations");
        ResetIdleAnimations();
        playerAnimator.SetTrigger("Spell");
    }

    private void IdleAnimations()
    {
        if (Random.Range(0, 2) == 0)
        {
            playerAnimator.SetTrigger("IdleAction");
        }
        else
        {
            playerAnimator.SetTrigger("IdleAction2");
        }
        Invoke("ResetIdleAnimations", 2);
    }

    private void ResetIdleAnimations()
    {
        playerAnimator.ResetTrigger("IdleAction");
        playerAnimator.ResetTrigger("IdleAction2");
    }

    private void SpawnTowerPieces()
    {
        if (burstActive)
        {
            return;
        }
        SpawnTowerComponent();
    }

    private void SpawnTowerBurst()
    {
        burstActive = true;
        InvokeRepeating("SpawnTowerComponent", 1.5f * spawnInterval, 0.2f * spawnInterval);
        Invoke("StopTowerBurst", 2.5f * spawnInterval);
        Invoke("SpawnTowerBurst", 10 + 2.5f * spawnInterval);
    }

    private void StopTowerBurst()
    {
        CancelInvoke("SpawnTowerComponent");
        burstActive = false;
    }

    private void SpawnTowerComponent()
    {
        if (activeTowerIndex == 0)
        {
            Vector3 spawnLocation = transform.position + transform.up;
            // Quaternion randomRotation = Quaternion.Euler(Random.Range(-1f, 1f) + transform.rotation.x, Random.Range(-1f, 1f) + transform.rotation.y, Random.Range(-4f, 4f) + transform.rotation.z);
            GameObject newTowerComponent = Instantiate(towerComponents[Random.Range(0, towerComponents.Length)], spawnLocation, transform.rotation);
            activeTowerComponents.Add(newTowerComponent);
            newTowerComponent.GetComponent<HingeJoint>().connectedBody = baseTowerRigidBody;
            newTowerComponent.GetComponent<DestroyOutOfBounds>().playerControllerScript = this;
        }
        else
        {
            Vector3 spawnLocation = activeTowerComponents[activeTowerIndex - 1].transform.position + activeTowerComponents[activeTowerIndex - 1].transform.up;
            // Quaternion randomRotation = Quaternion.Euler(Random.Range(-1f, 1f) + activeTowerComponents[activeTowerIndex - 1].transform.rotation.x, Random.Range(-1f, 1f) + activeTowerComponents[activeTowerIndex - 1].transform.rotation.y, Random.Range(-4f, 4f) + activeTowerComponents[activeTowerIndex - 1].transform.rotation.z);
            GameObject newTowerComponent = Instantiate(towerComponents[Random.Range(0, towerComponents.Length)], spawnLocation, activeTowerComponents[activeTowerIndex - 1].transform.rotation);
            activeTowerComponents.Add(newTowerComponent);
            newTowerComponent.GetComponent<HingeJoint>().connectedBody = activeTowerComponents[activeTowerIndex - 1].GetComponent<Rigidbody>();
            newTowerComponent.GetComponent<DestroyOutOfBounds>().playerControllerScript = this;
        }
        activeTowerIndex++;
        if (activeTowerComponents.Count <= 14 )
        {
            virtualCameraFramingTransposer.m_CameraDistance = 5 + 1.25f * activeTowerComponents.Count;
            towerCentre.transform.rotation = Quaternion.Euler(5 + 1.35f * activeTowerComponents.Count, 0, 0);
        }
        maxScore++;
        scoreText.text = maxScore.ToString("N0", CultureInfo.InvariantCulture);
    }

    private void CheckTowerBalance()
    {
        if (activeTowerComponents.Count < 2)
        {
            return;
        }
        //for (int i = 0; i < activeTowerComponents.Count - 1; i++)
        //{
        //    if (Vector3.Angle(activeTowerComponents[i].transform.up, activeTowerComponents[i + 1].transform.up) > 8)
        //    {
        //        CancelInvoke("SpawnTowerPieces");
        //        CancelInvoke("CheckTowerBalance");
        //        CancelInvoke("SpawnTowerComponent");
        //        CancelInvoke("SpawnTowerBurst");
        //        playerAnimator.ResetTrigger("Spell");
        //        playerAnimator.SetTrigger("Die");
        //        for (int j = 0; j < activeTowerComponents.Count; j++)
        //        {
        //            Destroy(activeTowerComponents[j].GetComponent<HingeJoint>());
        //        }
        //        break;
        //    }
        //}
        Vector3 centreOfGravity = Vector3.zero;
        foreach (GameObject towerComponent in activeTowerComponents)
        {
            centreOfGravity += towerComponent.transform.position;
        }
        centreOfGravity /= activeTowerComponents.Count;
        if (Mathf.Abs(centreOfGravity.x) > 2f)
        {
            CancelInvoke("SpawnTowerPieces");
            CancelInvoke("CheckTowerBalance");
            CancelInvoke("SpawnTowerComponent");
            CancelInvoke("SpawnTowerBurst");
            playerAnimator.ResetTrigger("Spell");
            playerAnimator.SetTrigger("Die");
            for (int j = 0; j < activeTowerComponents.Count; j++)
            {
                Destroy(activeTowerComponents[j].GetComponent<HingeJoint>());
            }
            Debug.Log("Destroying tower due to centre of gravity being " + centreOfGravity.x + " units away from centre");
        }
        else
        {
            for (int i = 1; i < activeTowerComponents.Count - 1; i++)
            {
                if (Vector3.Angle(activeTowerComponents[i].transform.up, activeTowerComponents[i + 1].transform.up) > 15)
                {
                    CancelInvoke("SpawnTowerPieces");
                    CancelInvoke("CheckTowerBalance");
                    CancelInvoke("SpawnTowerComponent");
                    CancelInvoke("SpawnTowerBurst");
                    playerAnimator.ResetTrigger("Spell");
                    playerAnimator.SetTrigger("Die");
                    for (int j = 0; j < activeTowerComponents.Count; j++)
                    {
                        Destroy(activeTowerComponents[j].GetComponent<HingeJoint>());
                    }
                    Debug.Log("Destroying tower due to angle between tower components being " + Vector3.Angle(activeTowerComponents[i].transform.up, activeTowerComponents[i + 1].transform.up) + " degrees at location " + i);
                    break;
                }
            }
        }
    }

    public void RemoveTowerComponent(GameObject towerPiece)
    {
        activeTowerComponents.Remove(towerPiece);
        activeTowerIndex--;
    }

    private void OnDrawGizmos()
    {
        Vector3 centreOfGravity = Vector3.zero;
        foreach (GameObject towerComponent in activeTowerComponents)
        {
            centreOfGravity += towerComponent.transform.position;
        }
        centreOfGravity /= activeTowerComponents.Count;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(centreOfGravity, 1f);
    }

    public void ResetPlayer()
    {
        foreach (GameObject towerComponent in activeTowerComponents)
        {
            Destroy(towerComponent);
        }
        activeTowerComponents.Clear();
        activeTowerIndex = 0;
        CancelInvoke("SpawnTowerPieces");
        CancelInvoke("CheckTowerBalance");
        CancelInvoke("SpawnTowerComponent");
        CancelInvoke("SpawnTowerBurst");
        CancelInvoke("IdleAnimations");
        playerAnimator.ResetTrigger("Spell");
        playerAnimator.ResetTrigger("Die");
        virtualCameraFramingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        virtualCameraFramingTransposer.m_CameraDistance = 5;
        towerCentre.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        towerCentre.transform.rotation = Quaternion.Euler(5, 0, 0);
        InvokeRepeating("IdleAnimations", 0, 10);
        maxScore = 0;
        gyroRotate = false;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        virtualCamera.m_Lens.Dutch = 0;
        scoreText.text = maxScore.ToString("N0", CultureInfo.InvariantCulture);
        startButton.SetActive(true);
        restartButton.SetActive(false);
    }
}
