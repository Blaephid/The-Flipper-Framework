using UnityEngine;
using UnityEngine.VFX;

namespace WindTriggerSystem
{
    public class WindTrigger : MonoBehaviour
    {
        [Header("Fan Rotation")]
        [SerializeField] private Transform _fanRotation;
        [SerializeField] private float _fanRotateSpeed;
        [SerializeField] private float _fanAcceleration = 0.2f;
        [SerializeField] private float _minFanSpeed = 0.0f;
        [SerializeField] private float _maxFanSpeed = 1500f;

        [Header("Wind Distortion")]
        [SerializeField] private Renderer _windDistortionRenderer;
        [SerializeField] private float _windDistortion;
        [SerializeField] private float _windAcceleration = 0.00003f;
        [SerializeField] private float _minWindDistortion = 0.0f;
        [SerializeField] private float _maxWindDistortion = 0.2f;

        [Header("Wind Swirl")]
        [SerializeField] private VisualEffect _windSwirl;
        [SerializeField] private float _windRotateSpeed;
        [SerializeField] private float _windSwirlAcceleration = 0.2f;
        [SerializeField] private float _minWindSpeed = 0.0f;
        [SerializeField] private float _maxWindSpeed = 1500f;

        [SerializeField] private float _windDistortionSpeed;
        [SerializeField] private float _windSwirlDistortionAcceleration = 0.0065f;
        [SerializeField] private float _minSwirlDistortionScale = 0.0f;
        [SerializeField] private float _maxSwirlDistortionScale = 50.0f;

        [Header("Wind Dust")]
        [SerializeField] private VisualEffect _windDust;
        [SerializeField] private float _attractionSpeed;
        [SerializeField] private float _attractionAcceleration = 0.00001f;
        [SerializeField] private float _minAttractionSpeed = 0.0f;
        [SerializeField] private float _maxAttractionSpeed = 0.015f;

        public bool _isFanOn = false;

        void Start()
        {
            _windDust.gameObject.SetActive(false);
        }

        void Update()
        {
            _fanRotation.Rotate(Vector3.up * _fanRotateSpeed * Time.deltaTime);

            _windDistortionRenderer.material.SetFloat("_DistortionAmount", _windDistortion);

            _windSwirl.SetFloat("SwirlRotationSpeed", _windRotateSpeed);
            _windSwirl.SetFloat("SwirlDistortionScale", _windDistortionSpeed);

            _windDust.SetFloat("AttractionSpeed", _attractionSpeed);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isFanOn = true;
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                _isFanOn = false;
            }

            if (_isFanOn)
            {
                Acceleration();
            }
            else
            {
                Deceleration();
            }

            if (_fanRotateSpeed == 0)
            {
                _windDust.gameObject.SetActive(false);
            }
            else
            {
                _windDust.gameObject.SetActive(true);
            }
        }

        private void Acceleration()
        {
            _fanRotateSpeed += _fanAcceleration;
            _windDistortion += _windAcceleration;
            _windRotateSpeed += _windSwirlAcceleration;
            _windDistortionSpeed += _windSwirlDistortionAcceleration;
            _attractionSpeed += _windAcceleration;

            if (_fanRotateSpeed > _maxFanSpeed)
            {
                _fanRotateSpeed = _maxFanSpeed;
            }

            if (_windDistortion > _maxWindDistortion)
            {
                _windDistortion = _maxWindDistortion;
            }

            if (_windRotateSpeed > _maxWindSpeed)
            {
                _windRotateSpeed = _maxWindSpeed;
            }

            if (_windDistortionSpeed > _maxSwirlDistortionScale)
            {
                _windDistortionSpeed = _maxSwirlDistortionScale;
            }

            if (_attractionSpeed > _maxAttractionSpeed)
            {
                _attractionSpeed = _maxAttractionSpeed;
            }
        }

        private void Deceleration()
        {
            _fanRotateSpeed -= _fanAcceleration;
            _windDistortion -= _windAcceleration;
            _windRotateSpeed -= _windSwirlAcceleration;
            _windDistortionSpeed -= _windSwirlDistortionAcceleration;
            _attractionSpeed -= _windAcceleration;

            if (_fanRotateSpeed < _minFanSpeed)
            {
                _fanRotateSpeed = _minFanSpeed;
            }

            if (_windDistortion < _minWindDistortion)
            {
                _windDistortion = _minWindDistortion;
            }

            if (_windRotateSpeed < _minWindSpeed)
            {
                _windRotateSpeed = _minWindSpeed;
            }

            if (_windDistortionSpeed < _minSwirlDistortionScale)
            {
                _windDistortionSpeed = _minSwirlDistortionScale;
            }

            if (_attractionSpeed < _minAttractionSpeed)
            {
                _attractionSpeed = _minAttractionSpeed;
            }
        }
    }
}