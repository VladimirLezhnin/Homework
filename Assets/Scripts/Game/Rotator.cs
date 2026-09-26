using UnityEngine;

namespace Game
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] 
        private float rotationSpeed;
        
        [SerializeField] 
        private Vector3 rotationAxis = Vector3.up;
    
        private void FixedUpdate() => transform.Rotate(rotationAxis , Time.deltaTime * rotationSpeed);
    }
}
