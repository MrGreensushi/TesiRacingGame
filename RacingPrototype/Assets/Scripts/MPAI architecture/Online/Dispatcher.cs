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

        [Tooltip("Testing: The data for the prediction is retrieved from the actual player\n" +
                    "Real-Case: The data is retrieved from the ghost car")]
        public OperatingMode operatingMode;


        //DELTA VERSION
        public void Routine(Player_Ghost pg)
        {

           
                PhysicRoutine(pg);

        }

        public void PhysicRoutine(Player_Ghost pg)
        {

            var (delta, infos) = pg.ghost.PhysicInfos();
            var (delta_T, infos_T) = pg.player.DispatcherInfos();
            pg.ghost.lastInfo = infos;
            pg.player.lastInfo = infos_T;

            if (operatingMode != OperatingMode.Testing)
                UpdateMatrix(delta, infos, pg);

            else
                UpdateMatrix(delta_T, infos_T, pg);

        }

       

        void UpdateMatrix(float[] d, float[] i, Player_Ghost pg)
        {
            
            
                pe.UpdateMatrix(d, timesteps, featuresNumber, i, pg);
        }

    }
}
