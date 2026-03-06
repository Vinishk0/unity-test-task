using System.Collections;
using System.Collections.Generic;
using AxGrid;
using AxGrid.Base;
using AxGrid.Model;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

namespace SlotExample
{
    public class SlotReelController : MonoBehaviourExtBind
    {
        [Header("Reels (Content ñ VerticalLayoutGroup)")]
        [SerializeField] private RectTransform[] reelContents;

        [Header("Snap Center")]
        [SerializeField] private RectTransform snapCenter;

        [Header("Layout")]
        [SerializeField] private float cellHeight = 200f;
        [SerializeField] private float spacing = 50f;

        [Header("Spin Settings")]
        [SerializeField] private float startSpeed = 400f;
        [SerializeField] private float maxSpeed = 1200f;
        [SerializeField] private float acceleration = 800f;
        [SerializeField] private float deceleration = 600f;

        [Header("Stop Settings")]
        [SerializeField] private float reelStopDelay = 0.35f;
        [SerializeField] private float snapTriggerSpeed = 60f;
        [SerializeField] private float snapLerpSpeed = 14f;

        [Header("Symbols")]
        [SerializeField] private Sprite[] symbols;

        private enum ReelPhase { Idle, Spinning, Decelerating, Snapping, Stopped }

        private class ReelRuntime
        {
            public RectTransform Content;
            public Image[] Images;
            public ReelPhase Phase = ReelPhase.Idle;
            public float Speed;
            public float SnapTarget;
        }

        private readonly List<ReelRuntime> _reels = new();
        private bool _anySpinning;

        private float Step => ReelMath.Step(cellHeight, spacing);

        [OnStart((RunLevel)Priority.PriorityHigh)]
        private void CacheReels()
        {
            _reels.Clear();

            foreach (var content in reelContents)
            {
                if (!content) { Log.Error("ReelContent is null"); continue; }

                var images = content.GetComponentsInChildren<Image>(false);
                if (images == null || images.Length == 0) { Log.Error("ReelContent: no Image children"); continue; }

                foreach (var img in images)
                    img.sprite = GetRandomSymbol();

                _reels.Add(new ReelRuntime { Content = content, Images = images });
            }
        }

        [Bind(LootboxEvents.Cmd_ReelsStart)]
        private void OnCmdStart() => StartSpin();

        [Bind(LootboxEvents.Cmd_ReelsStop)]
        private void OnCmdStop() => StopSpin();

        private void StartSpin()
        {
            if (_anySpinning) return;

            _anySpinning = true;
            Model.Set(LootboxModelKeys.IsSpinning, true);

            foreach (var rd in _reels)
            {
                rd.Phase = ReelPhase.Spinning;
                rd.Speed = startSpeed;
            }
        }

        private void StopSpin()
        {
            if (!_anySpinning) return;

            bool alreadyStopping = _reels.TrueForAll(
                r => r.Phase == ReelPhase.Decelerating
                  || r.Phase == ReelPhase.Snapping
                  || r.Phase == ReelPhase.Stopped);
            if (alreadyStopping) return;

            StartCoroutine(StopReelsSequentially());
        }

        private IEnumerator StopReelsSequentially()
        {
            for (int i = 0; i < _reels.Count; i++)
            {
                var rd = _reels[i];

                if (rd.Phase == ReelPhase.Spinning)
                    rd.Phase = ReelPhase.Decelerating;

                yield return new WaitUntil(() => rd.Phase == ReelPhase.Stopped || rd.Phase == ReelPhase.Idle);

                if (i < _reels.Count - 1)
                    yield return new WaitForSeconds(reelStopDelay);
            }
        }

        [OnUpdate]
        private void Tick()
        {
            if (!_anySpinning) return;

            bool allStopped = true;

            foreach (var rd in _reels)
            {
                TickReel(rd);
                if (rd.Phase != ReelPhase.Stopped && rd.Phase != ReelPhase.Idle)
                    allStopped = false;
            }

            if (allStopped)
            {
                _anySpinning = false;
                Model.Set(LootboxModelKeys.IsSpinning, false);
                Settings.Invoke(LootboxEvents.Reels_AllStopped);
            }
        }

        private void TickReel(ReelRuntime rd)
        {
            switch (rd.Phase)
            {
                case ReelPhase.Spinning:
                    rd.Speed = Mathf.Min(rd.Speed + acceleration * Time.deltaTime, maxSpeed);
                    MoveAndRecycle(rd);
                    break;

                case ReelPhase.Decelerating:
                    rd.Speed = Mathf.Max(rd.Speed - deceleration * Time.deltaTime, 0f);
                    MoveAndRecycle(rd);

                    if (rd.Speed <= snapTriggerSpeed)
                    {
                        rd.Speed = 0f;
                        rd.SnapTarget = ReelMath.CalculateSnapTarget(rd.Content, rd.Images, snapCenter);
                        rd.Phase = ReelPhase.Snapping;
                    }
                    break;

                case ReelPhase.Snapping:
                    var pos = rd.Content.anchoredPosition;
                    pos.y = Mathf.Lerp(pos.y, rd.SnapTarget, snapLerpSpeed * Time.deltaTime);

                    if (Mathf.Abs(pos.y - rd.SnapTarget) < 1f)
                    {
                        pos.y = rd.SnapTarget;
                        rd.Phase = ReelPhase.Stopped;
                    }

                    rd.Content.anchoredPosition = pos;
                    break;
            }
        }

        private void MoveAndRecycle(ReelRuntime rd)
        {
            float step = Step;
            var pos = rd.Content.anchoredPosition;
            pos.y -= rd.Speed * Time.deltaTime;

            while (pos.y <= -step)
            {
                pos.y += step;

                var last = rd.Images[^1];
                for (int i = rd.Images.Length - 1; i > 0; i--)
                    rd.Images[i] = rd.Images[i - 1];
                rd.Images[0] = last;

                last.transform.SetAsFirstSibling();
                last.sprite = GetRandomSymbol();
            }

            rd.Content.anchoredPosition = pos;
        }

        private Sprite GetRandomSymbol()
        {
            if (symbols == null || symbols.Length == 0) return null;
            return symbols[Random.Range(0, symbols.Length)];
        }
    }
}
