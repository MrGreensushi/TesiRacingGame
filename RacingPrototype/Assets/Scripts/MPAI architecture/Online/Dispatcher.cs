using QuickStart;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace QuickStart
{
    public class Dispatcher : MonoBehaviour
    {
        public int timesteps, featuresNumber;
        [SerializeField] Physic_Engine pe;
        public bool updateSelf = false;

        public bool UpdateSelf { set { updateSelf = value; } }

        //DELTA VERSION
        public void Routine(Player_Ghost pg)
        {

            var (delta, infos) = pg.ghost.PhysicInfos();
            var (delta_T, infos_T) = pg.player.DispatcherInfos();
            pg.ghost.lastInfo = infos;
            pg.player.lastInfo = infos_T;

            if (updateSelf)
                UpdateMatrix(delta, infos, pg);

            else
                UpdateMatrix(delta_T, infos_T, pg);

        }


        void UpdateMatrix(float[] d, float[] i,Player_Ghost pg)
        {
            pe.UpdateMatrix(d, timesteps, featuresNumber, i, pg);
        }

    }
}
