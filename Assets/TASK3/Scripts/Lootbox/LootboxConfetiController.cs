using AxGrid.Base;
using AxGrid.Model;
using UnityEngine;

namespace SlotExample
{
    public class LootboxConfetiController : MonoBehaviourExtBind
    {
        [SerializeField] private ParticleSystem onStartParticles;

        // Метод проигрования системы частиц, при нажатии на кнопку "Play"
        [Bind(LootboxEvents.ParticleSystem_Confeti_OnStart)]
        private void PlayStart()
        {
            if (onStartParticles)
            {
                onStartParticles.Play(true);
            }
        }
    }
}
