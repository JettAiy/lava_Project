using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHandler : MonoBehaviour
{

    public BulletParametersSO parametersSO;
    public Vector3 direction;
    private float speed = 2f;
    private float maxLiveTime = 5f; //for deleting bullets that goes far away without collision
    private byte team;

    public void Init(Vector3 position, byte team, BulletParametersSO parametersSO)
    {
        this.parametersSO = parametersSO;
        this.team = team;

        transform.LookAt(position);
        direction = (position - transform.position).normalized;
        //transform.forward = direction;

        GetComponent<Rigidbody>().AddForce(direction * speed, ForceMode.Impulse);


    }

    // Update is called once per frame
    void Update()
    {
        maxLiveTime -= Time.deltaTime;

        if (maxLiveTime <= 0)
        {
            Destroy(gameObject);
        }

        //transform.position += direction * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {

        //Debug.Log(collision.collider.name);

        CharacterController characterController = collision.collider.GetComponent<CharacterController>();

        if (characterController != null)
        {
            //no frendly fire
            if (characterController.parametersSO.team != team)
            {
                characterController.GetImpact(-transform.forward * parametersSO.impactForce, collision.contacts[0].point);
                GameHandler.instance.CreateHitParticleEffect(collision.contacts[0].point, transform.rotation);
            }     
            else
            {
                return; // we hit our own hitbox
            }
        }
        else
            GameHandler.instance.CreateDieParticleEffect(collision.contacts[0].point);


        Destroy(gameObject);
    }

}
