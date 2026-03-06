using AxGrid;
using AxGrid.Base;
using AxGrid.FSM;
using UnityEngine;

namespace SlotExample
{
    public class LootboxFsmBootstrap : MonoBehaviourExt
    {
        [OnAwake]
        private void SetupDefaults()
        {
            Model.Set(LootboxModelKeys.IsSpinning, false);
  
            // Отключение кнопки "Stop" по умолчанию и активация кнопки "Start"
            Model.Set(LootboxModelKeys.BtnStartEnable, true);
            Model.Set(LootboxModelKeys.BtnStopEnable, false);
        }

        [OnAwake]
        private void CreateFsm()
        {
            // Создание и добавление состояний
            Settings.Fsm = new FSM();

            Settings.Fsm.Add(new LootboxIdleState());
            Settings.Fsm.Add(new LootboxSpinLockStopState()); 
            Settings.Fsm.Add(new LootboxSpinCanStopState());
            Settings.Fsm.Add(new LootboxStoppingState());
        }

        [OnStart]
        private void StartFsm()
        {
            // Применение первого состояния
            Settings.Fsm.Start("LootboxIdleState");
        }

        [OnUpdate]
        private void UpdateFsm()
        {
            Settings.Fsm.Update(Time.deltaTime);
        }
    }
}
