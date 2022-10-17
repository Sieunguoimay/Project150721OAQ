using System;
using System.Linq;
using Common;
using CommonActivities;
using Gameplay.Board;
using SNM;
using Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Gameplay.Piece
{
    public static class PieceScheduler
    {
        public static void MovePiecesOutOfTheBoard(Gameplay.Piece.Piece[] pieces, Vector3[] positions,
            Vector3 centerPoint)
        {
            Array.Sort(pieces, (a, b) =>
            {
                var da = Vector3.SqrMagnitude(centerPoint - a.transform.position);
                var db = Vector3.SqrMagnitude(centerPoint - b.transform.position);
                return da < db ? -1 : 1;
            });
            var delay = 0f;
            for (var i = 0; i < pieces.Length; i++)
            {
                var flocking = new Flocking(pieces[i].FlockingConfigData, positions[i], pieces[i].transform,
                    null);

                pieces[i].PieceActivityQueue.Add(new Delay(delay += 0.2f));
                CreateAnimActivity(pieces[i], LegHashes.stand_up, null);
                pieces[i].PieceActivityQueue.Add(flocking);
                CreateAnimActivity(pieces[i], LegHashes.sit_down, null);
                pieces[i].PieceActivityQueue.Begin();
            }
        }

        public static void MovePieceToTheBoardOnGameStart(Citizen p, Tile t, Activity triggerActivity,
            float delay)
        {
            var position = t.GetPositionInFilledCircle(Mathf.Max(0, t.Pieces.Count - 1));
            if (triggerActivity != null)
            {
                p.PieceActivityQueue.Add(triggerActivity);
            }

            CreateAnimActivity(p, LegHashes.sit_down, null);
            p.PieceActivityQueue.Add(new Delay(delay));
            p.PieceActivityQueue.Add(new Flocking(p.FlockingConfigData, position, p.transform, null));
            p.PieceActivityQueue.Begin();
        }

        public static Activity CreateJumpTimelineActivity(Piece p, Vector3 target)
        {
            if (p.JumpTimeline == null) return null;
            Activity activity = null;
            activity = new Lambda(() =>
            {
                p.JumpTimeline.Stop();
                var euler = Quaternion
                    .LookRotation(p.transform.InverseTransformDirection(target - p.transform.position))
                    .eulerAngles;

                var tracks =
                    p.JumpTimeline.playableAsset.outputs.Where(tr => tr.sourceObject is TransformControlTrack)
                        .Select(tr => tr.sourceObject as TransformControlTrack);

                var jumping = tracks.FirstOrDefault(t => t.label.Equals("jumping"));
                var facing = tracks.FirstOrDefault(t => t.label.Equals("facing"));
                SetTrack(jumping, target.x, target.z, 0);
                SetTrack(facing, 0, 0, ClampEuler(euler.y, ((Transform) p.JumpTimeline.GetGenericBinding(facing)).localEulerAngles.y));

                static float ClampEuler(float newEuler, float oldEuler)
                {
                    var offset = newEuler - oldEuler;
                    if (offset > 180f)
                    {
                        return newEuler - 360f;
                    }
                    if (offset < -180f)
                    {
                        return newEuler + 360f;
                    }
                    return newEuler;
                }

                static void SetTrack(TrackAsset tr, float x, float z, float eulerY)
                {
                    if (tr == null) return;
                    var clips = tr.GetClips();
                    foreach (var c in clips)
                    {
                        ((TransformControlClip) c.asset).Template.position.x = x;
                        ((TransformControlClip) c.asset).Template.position.z = z;
                        ((TransformControlClip) c.asset).Template.eulerAngles.y = eulerY;
                    }
                }

                var transform = p.transform;
                var pos = transform.position;
                pos.y = 0f;
                transform.position = pos;

                p.JumpTimeline.Play();
                p.Delay((float) p.JumpTimeline.duration, () =>
                {
                    activity?.NotifyDone();

                    var clips = jumping.GetClips().Concat(facing.GetClips());
                    foreach (var c in clips)
                    {
                        var clip = ((TransformControlClip) c.asset);
                        clip.Template.position = Vector3.zero;
                        clip.Template.eulerAngles = Vector3.zero;
                    }
                });
            }, null);

            p.PieceActivityQueue.Add(activity);
            return activity;
        }

        public static void CreateAnimActivity(Piece p, int animHash, Action onDone)
            => CreateAnimActivity(p, () => animHash, onDone);

        public static void CreateAnimActivity(Piece p, Func<int> animHash, Action onDone)
        {
            if (p.Animator == null) return;

            var anim = -1;
            Activity animActivity = new Lambda(() =>
            {
                anim = animHash.Invoke();
                p.Animator.Play(anim, -1, 0f);
            }, () =>
            {
                var info = p.Animator.GetCurrentAnimatorStateInfo(0);
                if (info.shortNameHash != anim) return false;

                if (info.loop)
                    Debug.LogError("Loop... :(");

                return info.normalizedTime >= 1f;
            });
            if (onDone != null)
            {
                animActivity.Done += onDone;
            }

            p.PieceActivityQueue.Add(animActivity);
        }
    }
}