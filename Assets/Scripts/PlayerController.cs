using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using TMPro;
using System.Globalization;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    static readonly int Spell = Animator.StringToHash("Spell");
    static readonly int IdleAction = Animator.StringToHash("IdleAction");
    static readonly int IdleAction2 = Animator.StringToHash("IdleAction2");
    static readonly int Die = Animator.StringToHash("Die");
    [SerializeField] private float spawnInterval = 5;
    [SerializeField] private GameObject[] towerComponents;
    [SerializeField] private Rigidbody baseTowerRigidBody;
    [SerializeField] private GameObject towerCentre;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachinePositionComposer virtualCameraFramingTransposer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject restartButton;
    private int maxScore;
    private float calibrationOffset;
    private List<GameObject> activeTowerComponents = new();
    private int activeTowerIndex = 0;
    private bool burstActive = false;
    float dampingRef;

    public bool gyroRotate = false;

    InputAction rotationAction;

    // Start is called before the first frame update
    void Start()
    {
        if (!UnityEngine.SceneManagement.SceneManager.GetSceneByName("SimplePoly City - Low Poly").isLoaded)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("SimplePoly City - Low Poly", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
        rotationAction = InputSystem.actions.FindAction("Rotation");
        try
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
            Quaternion gyro = rotationAction.ReadValue<Quaternion>();
            calibrationOffset = (Quaternion.Euler(90, 0, 0) * gyro).eulerAngles.z;
        }
        catch (Exception)
        {
            // gyro not available
        }
        virtualCameraFramingTransposer.CameraDistance = 5;
        towerCentre.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        towerCentre.transform.rotation = Quaternion.Euler(5, 0, 0);
        InvokeRepeating(nameof(IdleAnimations), 2, 10);
        scoreText.text = maxScore.ToString("N0", CultureInfo.InvariantCulture);
        startButton.SetActive(true);
        restartButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        virtualCameraFramingTransposer.CameraDistance = Mathf.SmoothDamp(virtualCameraFramingTransposer.CameraDistance, 5 + 1.2f * Math.Clamp(activeTowerComponents.Count, 0, 18), ref dampingRef, 0.5f, 10f, Time.smoothDeltaTime);

        if (!gyroRotate)
        {
            return;
        }

        float gyro = Quaternion.Euler(0, 0, (Quaternion.Euler(90, 0, 0) * rotationAction.ReadValue<Quaternion>()).eulerAngles.z - calibrationOffset).eulerAngles.z;
        transform.rotation = Quaternion.Euler(0, 0, gyro);
        // set dutch angle to follow gyro
        // virtualCamera.Lens.Dutch = gyro;
        // set dutch angle with smoothing, accounting for 180 degree flips
        float targetDutch = gyro;
        if (targetDutch > 180)
        {
            targetDutch -= 360;
        }
        virtualCamera.Lens.Dutch = Mathf.Lerp(virtualCamera.Lens.Dutch, targetDutch, 0.1f);
    }

    void FixedUpdate()
    {
        if (activeTowerComponents.Count > 0)
        {
            towerCentre.transform.position = new Vector3(transform.position.x, activeTowerComponents[^1].transform.position.y, transform.position.z);
        }
    }

    public void ResetRotation()
    {
        try
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
            Quaternion gyro = rotationAction.ReadValue<Quaternion>();
            calibrationOffset = (Quaternion.Euler(90, 0, 0) * gyro).eulerAngles.z;
        }
        catch (Exception)
        {
            // gyro not available
        }
        Debug.Log(calibrationOffset);
    }

    public void BeginSpawning()
    {
        ResetRotation();
        gyroRotate = true;
        startButton.SetActive(false);
        restartButton.SetActive(true);
        InvokeRepeating(nameof(SpawnTowerPieces), 0.9f, spawnInterval);
        InvokeRepeating(nameof(CheckTowerBalance), 0.3f, 0.3f);
        Invoke(nameof(SpawnTowerBurst), 10f);
        CancelInvoke(nameof(IdleAnimations));
        ResetIdleAnimations();
        playerAnimator.SetTrigger(Spell);
        try
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
        }
        catch (Exception)
        {
            // gyro not available
        }
    }

    private void IdleAnimations()
    {
        playerAnimator.SetTrigger(Random.Range(0, 2) == 0 ? IdleAction : IdleAction2);
        Invoke(nameof(ResetIdleAnimations), 2);
    }

    private void ResetIdleAnimations()
    {
        playerAnimator.ResetTrigger(IdleAction);
        playerAnimator.ResetTrigger(IdleAction2);
    }

    private void SpawnTowerPieces()
    {
        if (burstActive)
        {
            return;
        }
        StartCoroutine(SpawnTowerComponent());
    }

    private void SpawnTowerBurst()
    {
        burstActive = true;
        InvokeRepeating(nameof(SpawnComponents), 1.5f * spawnInterval, 0.2f * spawnInterval);
        Invoke(nameof(StopTowerBurst), 2.5f * spawnInterval);
        Invoke(nameof(SpawnTowerBurst), 10 + 2.5f * spawnInterval);
    }

    void SpawnComponents()
    {
        StartCoroutine(SpawnTowerComponent());
    }

    private void StopTowerBurst()
    {
        CancelInvoke(nameof(SpawnTowerComponent));
        CancelInvoke(nameof(SpawnComponents));
        burstActive = false;
    }

    private IEnumerator SpawnTowerComponent()
    {
        if (activeTowerIndex == 0)
        {
            Vector3 spawnLocation = transform.position + transform.up;
            // Quaternion randomRotation = Quaternion.Euler(Random.Range(-1f, 1f) + transform.rotation.x, Random.Range(-1f, 1f) + transform.rotation.y, Random.Range(-4f, 4f) + transform.rotation.z);
            AsyncInstantiateOperation<GameObject> newTowerComponentOperation = InstantiateAsync(towerComponents[Random.Range(0, towerComponents.Length)], spawnLocation, transform.rotation);
            yield return newTowerComponentOperation;
                activeTowerComponents.Add(newTowerComponentOperation.Result[0]);
                newTowerComponentOperation.Result[0].GetComponent<HingeJoint>().connectedBody = baseTowerRigidBody;
                newTowerComponentOperation.Result[0].GetComponent<DestroyOutOfBounds>().playerControllerScript = this;
        }
        else
        {
            Vector3 spawnLocation = activeTowerComponents[activeTowerIndex - 1].transform.position + activeTowerComponents[activeTowerIndex - 1].transform.up;
            // Quaternion randomRotation = Quaternion.Euler(Random.Range(-1f, 1f) + activeTowerComponents[activeTowerIndex - 1].transform.rotation.x, Random.Range(-1f, 1f) + activeTowerComponents[activeTowerIndex - 1].transform.rotation.y, Random.Range(-4f, 4f) + activeTowerComponents[activeTowerIndex - 1].transform.rotation.z);
            AsyncInstantiateOperation<GameObject> newTowerComponentOperation = InstantiateAsync(towerComponents[Random.Range(0, towerComponents.Length)], spawnLocation, activeTowerComponents[activeTowerIndex - 1].transform.rotation);
            yield return newTowerComponentOperation;
                activeTowerComponents.Add(newTowerComponentOperation.Result[0]);
                newTowerComponentOperation.Result[0].GetComponent<HingeJoint>().connectedBody =
                    activeTowerComponents[activeTowerIndex - 1].GetComponent<Rigidbody>();
                newTowerComponentOperation.Result[0].GetComponent<DestroyOutOfBounds>().playerControllerScript = this;
        }
        activeTowerIndex++;
        if (activeTowerComponents.Count <= 17 )
        {
            towerCentre.transform.SetPositionAndRotation(new Vector3(transform.position.x, activeTowerComponents[^1].transform.position.y, transform.position.z), Quaternion.Euler(5 + 1.35f * activeTowerComponents.Count, 0, 0));
        }
        else
        {
            towerCentre.transform.position = new Vector3(transform.position.x, activeTowerComponents[^1].transform.position.y, transform.position.z);
        }
        maxScore++;
        scoreText.text = maxScore.ToString("N0", CultureInfo.InvariantCulture);
    }

    private void CheckTowerBalance()
    {
        if (activeTowerComponents.Count < 1)
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
        if (Mathf.Abs(centreOfGravity.x) > 2.75f)
        {
            CancelInvoke(nameof(SpawnTowerPieces));
            CancelInvoke(nameof(CheckTowerBalance));
            CancelInvoke(nameof(SpawnTowerComponent));
            CancelInvoke(nameof(SpawnTowerBurst));
            CancelInvoke(nameof(SpawnComponents));
            playerAnimator.ResetTrigger(Spell);
            playerAnimator.SetTrigger(Die);
            foreach (GameObject component in activeTowerComponents)
            {
                Destroy(component.GetComponent<HingeJoint>());
            }
            Debug.Log("Destroying tower due to centre of gravity being " + centreOfGravity.x + " units away from centre");
        }
        else
        {
            for (int i = 1; i < activeTowerComponents.Count - 1; i++)
            {
                if (!(Vector3.Angle(activeTowerComponents[i].transform.up, activeTowerComponents[i + 1].transform.up) >
                      17))
                {
                    continue;
                }

                CancelInvoke(nameof(SpawnTowerPieces));
                CancelInvoke(nameof(CheckTowerBalance));
                CancelInvoke(nameof(SpawnTowerComponent));
                CancelInvoke(nameof(SpawnTowerBurst));
                CancelInvoke(nameof(SpawnComponents));
                playerAnimator.ResetTrigger(Spell);
                playerAnimator.SetTrigger(Die);
                foreach (GameObject component in activeTowerComponents)
                {
                    Destroy(component.GetComponent<HingeJoint>());
                }
                Debug.Log("Destroying tower due to angle between tower components being " + Vector3.Angle(activeTowerComponents[i].transform.up, activeTowerComponents[i + 1].transform.up) + " degrees at location " + i);
                break;
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
        CancelInvoke(nameof(SpawnTowerPieces));
        CancelInvoke(nameof(CheckTowerBalance));
        CancelInvoke(nameof(SpawnTowerComponent));
        CancelInvoke(nameof(SpawnTowerBurst));
        CancelInvoke(nameof(SpawnComponents));
        CancelInvoke(nameof(IdleAnimations));
        playerAnimator.ResetTrigger("Spell");
        playerAnimator.ResetTrigger("Die");
        towerCentre.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        towerCentre.transform.rotation = Quaternion.Euler(5, 0, 0);
        InvokeRepeating(nameof(IdleAnimations), 0, 10);
        maxScore = 0;
        gyroRotate = false;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        virtualCamera.Lens.Dutch = 0;
        scoreText.text = maxScore.ToString("N0", CultureInfo.InvariantCulture);
        startButton.SetActive(true);
        restartButton.SetActive(false);
    }
}
