/*
* Copyright (c) 2021 AoiKamishiro
*
* This code is provided under the MIT license.
*/

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Kamishiro.VRChatUDON.EventCalendar
{
    public class SyncCalendar : UdonSharpBehaviour
    {
        [SerializeField] private MeshRenderer _CalendarMeshRenderer;
        [SerializeField] private Scrollbar _Scrollbar;
        [SerializeField] private string _ScrollParam = "_Scroll";
        [SerializeField] private float _ResetTime = 60.0f;
        private float _ResetTimeTimer = 0.0f;
        [UdonSynced(UdonSyncMode.Smooth)] private float _SyncedValue = 0.0f;
        private float _LocalValue = 0.0f;
        private VRCPlayerApi _LocalPlayer;
        private const float FloatZero = 0.0f;
        private const string InitializeError = "[<color=red>VRC Scroll Event Calendar</color>] ScrollCalendar Initialization Failed. Please Check the UdonBehaviour.";
        private float _ResetStartPoint = FloatZero;
        private float _ResetRatio = FloatZero;
        private float _ResetValue = FloatZero;

        private void Start()
        {
            _LocalPlayer = Networking.LocalPlayer;

            if (_CalendarMeshRenderer == null ||
                  _Scrollbar == null ||
                  string.IsNullOrWhiteSpace(_ScrollParam) ||
                  _ResetTime < 5.0f)
            {
                Debug.LogError(InitializeError);
                this.enabled = false;
                return;
            }
        }
        private void Update()
        {
            SendCustomEvent(nameof(Deserialize));
            SendCustomEvent(nameof(ScrollReset));
        }

        public void SetScrollValue()
        {
            if (_CalendarMeshRenderer != null)
                _CalendarMeshRenderer.material.SetFloat(_ScrollParam, _LocalValue);
        }
        public void SliderVaueChanged()
        {
            if (!Networking.IsOwner(_LocalPlayer, gameObject))
                return;

            _LocalValue = _Scrollbar.value;
            _SyncedValue = _LocalValue;
            _ResetTimeTimer = _ResetTime;
            SendCustomEvent(nameof(SetScrollValue));
        }
        public void TakeOwnerShip()
        {
            if (Networking.IsOwner(_LocalPlayer, gameObject))
                return;

            Networking.SetOwner(_LocalPlayer, gameObject);
            _ResetTimeTimer = FloatZero;
        }
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(_LocalPlayer, gameObject))
                return;

            _ResetTimeTimer = _ResetTime;
        }
        public void ScrollReset()
        {
            if (!Networking.IsOwner(_LocalPlayer, gameObject))
                return;

            if (_LocalValue < 0.001f)
                return;

            if (_ResetTimeTimer > FloatZero)
            {
                _ResetTimeTimer -= Time.deltaTime;
                if (_ResetTimeTimer <= FloatZero)
                {
                    _ResetStartPoint = _Scrollbar.value;
                    _ResetRatio = FloatZero;
                }
            }
            else
            {
                if (_ResetRatio > 1.0f)
                    return;

                _ResetRatio += 0.1f * Time.deltaTime;
                _ResetValue = Mathf.Lerp(_ResetStartPoint, 0, _ResetRatio);
                _Scrollbar.value = _ResetValue;
                _LocalValue = _ResetValue;
                _SyncedValue = _ResetValue;
                SendCustomEvent(nameof(SetScrollValue));
            }

        }
        public void Deserialize()
        {
            if (Networking.IsOwner(_LocalPlayer, gameObject))
                return;

            if (_LocalValue == _SyncedValue)
                return;

            _LocalValue = _SyncedValue;
            _Scrollbar.value = _LocalValue;
            SendCustomEvent(nameof(SetScrollValue));
        }
    }
}
