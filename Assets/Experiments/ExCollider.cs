using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExCollider : MonoBehaviour
{
    [SerializeField] private ExperimentHapticPlugin EHP;
    [SerializeField] Vector3 center;
    [SerializeField] float radius;


    public void overlap()
    {
        //center = transform.position;
        //Collider[] hitColliders = Physics.OverlapSphere(center,radius*0.16f);
        

        //foreach (var hitCollider in hitColliders)
        //{
        //    hitCollider.
        //    Debug.Log(hitCollider);
        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
        EHP.UpdateCollision(collision, true, false, false);
        Debug.Log("entered collision ");
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("entered collision ");

    }

    private void OnCollisionExit(Collision collision)
    {
        EHP.UpdateCollision(collision, false, false, true);

    }

    private void OnCollisionStay(Collision collision)
    {
        EHP.UpdateCollision(collision, false, true, false);

    }
}
