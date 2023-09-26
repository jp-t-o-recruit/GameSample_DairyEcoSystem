using UnityEngine;



public class GameManager : MonoBehaviour, ILayeredSceneResident
{
    [SerializeField]
    private Camera _camera;
    private ScaleMode _mode;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        InputControllerService.CheckInput();
    }

    void FixedUpdate()
    {
    }
}