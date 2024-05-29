﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Monoliths.Player
{

    public class CharacterAnimationMaster : Monolith
    {
        public const string CHARACTER_ANIMATOR_STACK_DATA_ID = "CharacterAnimatorStack";

        private CharacterAnimatorStackPacket _dataPacket;
        private Transform _cameraYaw;
        public override void Defaults()
        {
            base.Defaults();
            _priority = -2;
        }
        public override bool Init()
        {
            _cameraYaw = Camera.main.transform.parent.parent;

            var player = GameObject.FindGameObjectWithTag("Player")
                .transform.Find("Display").GetComponent<Animator>();

            _dataPacket = new CharacterAnimatorStackPacket();
            _dataPacket.RegisterPlayerSpecific(player);
            _dataPacket.TryRegisterRotations(player);

            DataBridge.UpdateData(CHARACTER_ANIMATOR_STACK_DATA_ID, _dataPacket);
            return base.Init();
        }

        private void LateUpdate()
        {
            foreach (var pair in _dataPacket.Rotational) 
            {
                int turn = Mathf.RoundToInt(_cameraYaw.rotation.eulerAngles.y / 52f);
                turn -= DataBridge.TryGetData<CharacterAnimatorData>(pair.Key).EncodedData.Direction;

                if(turn < 0)
                    turn += 7;

                turn = 7 - turn + 1;

                var prevTurn = pair.Value.GetInteger("Turn");
                if(prevTurn != turn)
                {
                    pair.Value.SetFloat
                    (
                        "CycleOffset", 
                        pair.Value.GetFloat("CycleOffsetBuffer")
                    );
                }
                pair.Value.SetInteger("Turn", turn);
                
                var offset = pair.Value.GetFloat("CycleOffsetBuffer");
                if (offset > 32)
                    offset = 0;

                offset += Time.deltaTime;
                pair.Value.SetFloat("CycleOffsetBuffer", offset);
            }
        }

        private void Scan()
        {
            var suitables = GameObject.FindGameObjectsWithTag("2DCharacter");
            foreach (var suitable in suitables)
            {
                suitable.TryGetComponent(out Animator animator);
                if(animator is null)
                {
                    Debug.Log($"GameObject named \"{suitable.name}\" had no animator");
                    continue;
                }
                _dataPacket.TryRegisterRotations(animator);
            }
        }

        private void OnGameStateEnter(GameState state)
        {
            switch (state)
            {
                case LevelStartState:
                    Scan();
                    break;
                case LevelFinishState:
                    _dataPacket.Defaults();
                    break;
                default:
                    break;
            }
        }

        private void OnEnable() => GameState.OnEnter += OnGameStateEnter;
        private void OnDisable() => GameState.OnEnter -= OnGameStateEnter;
    }
}
