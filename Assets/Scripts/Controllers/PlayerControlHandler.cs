using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControlHandler : MonoBehaviour
{

    private CharacterController characterController;

    [SerializeField] private BulletParametersSO[] bulletParametersSO;
    private int currentBullet;

    Camera camera;
    [SerializeField] TMPro.TextMeshProUGUI bulletText;

    private void Start()
    {
        camera = Camera.main;
    }

    public void Init(CharacterController characterController)
    {
        this.characterController = characterController;
        currentBullet = 0;
        SetBulletType();
    }

    private void Update()
    {       
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            OnLeftMouseBtnDown();
        }

        if (Input.GetMouseButtonDown(1))
        {
            OnRightMouseBtnDown();
        }

        if (Input.mouseScrollDelta != Vector2.zero)
        {
            OnMouseScroll();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

    }


    private Vector3? GetRayHitPosition()
    {

        if (EventSystem.current.IsPointerOverGameObject()) return null;

        Ray ray = camera.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);

        RaycastHit[] hits = Physics.RaycastAll(ray);

        //Debug.DrawLine(ray.origin, ray.direction * 100, Color.blue, 10f);
        
        Vector3? point = null;

        foreach (var hit in hits)
        {

            if (hit.collider != null)
            {

                //Debug.Log("ray hits " + hit.collider.name);

                float dist_new = Vector3.Distance(ray.origin, hit.point);
                float dist_cur = Mathf.Infinity;
                if (point != null)
                {
                    dist_cur = Vector3.Distance(ray.origin, point.Value);
                }
                 

                if (dist_new  <= dist_cur)
                {
                    //Debug.Log("ray hits " + hit.collider.name);
                    point = hit.point;
                }
                    
            }

        }
        

        return point;
    }

    private void OnLeftMouseBtnDown()
    {

        if (characterController == null) return;

        Vector3? hitPosition = GetRayHitPosition();

        if (hitPosition != null)
        {
            characterController.SetAtackPoint(hitPosition);
            characterController.SetState(CharacterController.CharacterStates.Atack);
        }
        
    }

    private void OnRightMouseBtnDown()
    {
        if (characterController == null) return;

        Vector3? hitPosition = GetRayHitPosition();

        if (hitPosition != null)
        {
            characterController.SetMovePoint(hitPosition);
            characterController.SetState(CharacterController.CharacterStates.Move);
        }
    }
    

    private void SetBulletType()
    {
        characterController.SetBullet(bulletParametersSO[currentBullet]);
        bulletText.SetText("Bullet: " + bulletParametersSO[currentBullet].bulletName 
            + " pwr: " + bulletParametersSO[currentBullet].impactForce.ToString("N"));
    }
    
    private void OnMouseScroll()
    {
        currentBullet += (int)Input.mouseScrollDelta.y;
        
        if (currentBullet > bulletParametersSO.Length-1)
        {
            currentBullet = 0;
        }

        if (currentBullet < 0)
        {
            currentBullet = bulletParametersSO.Length - 1;
        }

        SetBulletType();
    }
}
