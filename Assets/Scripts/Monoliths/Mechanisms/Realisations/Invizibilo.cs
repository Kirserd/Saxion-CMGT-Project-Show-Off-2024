﻿using UnityEngine;

namespace Monoliths.Mechanisms
{
    public class Invizibilo : Actuator
    {
        public const string INVIZIBILO_ACCOMPANING = "CurrentInvizibiloCompanion";
        private enum State
        {
            Inactive,
            Active,
        }

        [SerializeField]
        private State _state;
        [Space(5)]
        [Header("PARAMETERS")]
        [SerializeField]
        private float _followingRadius;
        [SerializeField]
        private float _maxSpeed;
        [SerializeField]
        private float _accelerationMultiplier;

        [Space(5)]
        [Header("Activity code animator")]
        [SerializeField]
        private Renderer _renderer;
        [SerializeField]
        [Range(0, 1)]
        private float _changeStep;
        [SerializeField]
        [Range(3, 16)]
        private float _visibleDistance;

        private Vector2 _accelerationBuffer;
        private Vector3 _direction;

        private GameObject _caller;

        private static Transform _player;

        private bool _isVisible;

        private void Start()
        {
            DataBridge.UpdateData<Invizibilo>(INVIZIBILO_ACCOMPANING, null);
            _state = State.Inactive;
            _player ??= GameObject.FindGameObjectWithTag("Player").transform;
        }
        public override void Interact(GameObject caller)
        {
            if (Locked)
                return;

            _caller = caller;
            Invoke();
        }
        public override void Invoke()
        {
            if (_state == State.Inactive)
            {
                Locked = true;
                _state = State.Active;

                DataBridge.UpdateData(INVIZIBILO_ACCOMPANING, this);
            }
        }
        private void Update()
        {
            var data = DataBridge.TryGetData<Invizibilo>(INVIZIBILO_ACCOMPANING).EncodedData;
            if (data == this)
                _isVisible = true;
            else if(data == null)
            {
                if (Vector3.Distance(_player.position, transform.position) < _visibleDistance && 
                    Mathf.Abs(_player.position.y - transform.position.y) < _visibleDistance / 4)
                    _isVisible = true;
                else
                    _isVisible = false;
            }
            if(_state == State.Active)
            {
                _direction = Vector3.Distance(_caller.transform.position, transform.position) > _followingRadius? 
                            (_caller.transform.position - transform.position).normalized : 
                            Vector3.zero;
            }

            Move();
            AnimateVisibility();
        }

        private void Move()
        {
            _accelerationBuffer += _accelerationMultiplier * Time.deltaTime * 
                new Vector2(_direction.x, _direction.z);
            _accelerationBuffer /= Mathf.Clamp(0.5f * Time.deltaTime, 0.001f, 5f);

            _accelerationBuffer = new Vector2
            (
                Mathf.Clamp(Mathf.Abs(_accelerationBuffer.x), 0, _maxSpeed), 
                Mathf.Clamp(Mathf.Abs(_accelerationBuffer.y), 0, _maxSpeed)
            );
            transform.position += new Vector3
            (
                _direction.x * _accelerationBuffer.x, 
                0, 
                _direction.z * _accelerationBuffer.y
            ) * Time.deltaTime;
        }

        private void AnimateVisibility()
        {
            var color = _renderer.material.GetColor("_Color");
            var opacity = color.a;
            if (_isVisible)
            {
                if (opacity < 1)
                    opacity += _changeStep * Time.deltaTime;
            }
            else if (opacity > 0) 
                opacity -= _changeStep * Time.deltaTime;

            _renderer.material.SetColor("_Color", 
                new Color(color.r, color.g, color.b, opacity));
        }

        public void FinalizeAct()
        {
            _state = State.Inactive;
            DataBridge.UpdateData<Invizibilo>(INVIZIBILO_ACCOMPANING, null);
            Destroy(gameObject);
        }
    }
}
