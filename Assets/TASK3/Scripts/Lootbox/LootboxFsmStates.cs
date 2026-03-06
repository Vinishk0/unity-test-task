using AxGrid;
using AxGrid.FSM;
using AxGrid.Model;
using System.Diagnostics;

namespace SlotExample
{
    internal static class LootboxButtons
    {
        public const string Start = "Start";
        public const string Stop = "Stop";
    }

    internal static class LootboxFsm
    {
        public static void SetStartEnabled(bool value) => Settings.Model.Set(LootboxModelKeys.BtnStartEnable, value);
        public static void SetStopEnabled(bool value) => Settings.Model.Set(LootboxModelKeys.BtnStopEnable, value);
        public static void SetSpinning(bool value) => Settings.Model.Set(LootboxModelKeys.IsSpinning, value);

        public static void DisableAllButtons()
        {
            SetStartEnabled(false);
            SetStopEnabled(false);
        }
    }

    [State("LootboxIdleState")]
    public class LootboxIdleState : FSMState
    {
        public void Enter()
        {
            LootboxFsm.SetSpinning(false);
            LootboxFsm.SetStartEnabled(true);
            LootboxFsm.SetStopEnabled(false);
        }

        [Bind(LootboxEvents.Fsm_OnBtn)]
        public void OnBtn(string buttonName)
        {
            if (buttonName != LootboxButtons.Start) return;

            LootboxFsm.SetSpinning(true);
            LootboxFsm.DisableAllButtons();

            Settings.Invoke(LootboxEvents.Cmd_ReelsStart);
            Settings.Invoke(LootboxEvents.ParticleSystem_Confeti_OnStart);

            Parent.Change("LootboxSpinLockStopState");
        }
    }

    [State("LootboxSpinLockStopState")]
    public class LootboxSpinLockStopState : FSMState
    {
        public void Enter()
        {
            LootboxFsm.DisableAllButtons();
            Log.Info("ENTER LockStop");
        }

        [One(3f)]
        public void UnlockStop()
        {
            Log.Info("UNLOCK Stop now");
            LootboxFsm.SetStopEnabled(true);
            Parent.Change("LootboxSpinCanStopState");
        }

        [Bind(LootboxEvents.Fsm_OnBtn)]
        public void OnBtn(string buttonName)
        {
        }
    }

    [State("LootboxSpinCanStopState")]
    public class LootboxSpinCanStopState : FSMState
    {
        public void Enter()
        {
            Log.Info("Enter SpinCanStopState - enabling Stop button");
            LootboxFsm.SetStopEnabled(true);

            Log.Info($"Stop button enabled: {Settings.Model.GetBool(LootboxModelKeys.BtnStopEnable)}");
        }

        [Bind(LootboxEvents.Fsm_OnBtn)]
        public void OnBtn(string buttonName)
        {
            if (buttonName != LootboxButtons.Stop) return;

            LootboxFsm.DisableAllButtons();

            Settings.Invoke(LootboxEvents.Cmd_ReelsStop);

            Parent.Change("LootboxStoppingState");
        }
    }

    [State("LootboxStoppingState")]
    public class LootboxStoppingState : FSMState
    {
        public void Enter()
        {
            LootboxFsm.DisableAllButtons();
        }

        [Bind(LootboxEvents.Fsm_OnBtn)]
        public void OnBtn(string buttonName)
        {
            
        }

        [Bind(LootboxEvents.Reels_AllStopped)]
        public void OnAllStopped()
        {
            LootboxFsm.SetStartEnabled(true);
            Parent.Change("LootboxIdleState");
        }
    }
}
