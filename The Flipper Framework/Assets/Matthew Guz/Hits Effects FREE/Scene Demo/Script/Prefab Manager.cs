using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace MatthewAssets
{

    public class PrefabManager : MonoBehaviour
    {
        public GameObject[] prefabs; // List of prefabs assigned from the Inspector
        public Collider floorCollider; // The floor to detect clicks
        public Transform cameraPivot; // Pivot for the camera
        public float cameraRotationSpeed = 10f; // Rotation speed
        public float destroyDelay = 2f; // Time to destroy prefabs
        public Text infoText;
        private int currentIndex = 0; // Index of the current prefab


        void Start()
        {
            UpdateInfoText(); // Update text at start
        }
        void Update()
        {
            // Switch between prefabs with left/right keys
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SelectPreviousPrefab();
            }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                SelectNextPrefab();
            }

            // Instantiate the prefab with one click
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                if (floorCollider.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000f))
                {
                    GameObject instance = Instantiate(prefabs[currentIndex], hit.point, Quaternion.identity);
                    Destroy(instance, destroyDelay); // Destroy in 2 seconds
                }
            }


            // Rotate the camera around the pivot
            cameraPivot.Rotate(Vector3.up * (cameraRotationSpeed * Time.deltaTime)); // Rotates on the Y axis


        }


        private void SelectPreviousPrefab() // Previous prefab
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = prefabs.Length - 1;
            }
            UpdateInfoText();
        }

        private void SelectNextPrefab() // Next prefab
        {
            currentIndex++;
            if (currentIndex >= prefabs.Length)
            {
                currentIndex = 0;
            }
            UpdateInfoText();
        }
        private void UpdateInfoText()  // Name and number of the prefab
        {
            int currentNumber = currentIndex + 1;
            int totalNumber = prefabs.Length;

            infoText.text = $"({currentNumber}/{totalNumber}) \nCurrent effect: {prefabs[currentIndex].name} ";
        }

    }
}