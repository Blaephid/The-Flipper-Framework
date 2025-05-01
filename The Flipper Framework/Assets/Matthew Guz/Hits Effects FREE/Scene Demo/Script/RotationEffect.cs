using UnityEngine;

namespace MatthewAssets
{


    public class ParticleRotationController : MonoBehaviour
    {
        public ParticleSystem particleSystem; // Asigna el sistema de partículas en el Inspector
        public float minZRotation = 0f; // Valor mínimo para la rotación en Z
        public float maxZRotation = 360f; // Valor máximo para la rotación en Z

        void Start()
        {
            SetRandomRotation();
            particleSystem.Play(); // Inicia el sistema de partículas (opcional)
        }

        void SetRandomRotation()
        {
            // Genera un ángulo aleatorio entre los valores mínimo y máximo
            float randomZRotation = Random.Range(minZRotation, maxZRotation);
            // Aplica la rotación al transform del sistema de partículas
            particleSystem.transform.rotation = Quaternion.Euler(0f, 0f, randomZRotation);
        }
    }
}